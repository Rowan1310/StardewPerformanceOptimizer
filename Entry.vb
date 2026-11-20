
Imports System
Imports System.Diagnostics
Imports System.Reflection
Imports HarmonyLib
Imports Microsoft.Xna.Framework
Imports StardewModdingAPI
Imports StardewModdingAPI.Events
Imports StardewValley

Public Class Entry
    Inherits [Mod]

    Private Const REPORT_INTERVAL_TICKS As Integer = 1800

    Private _harmony As Harmony
    Private _ticksSinceReport As Integer

    Private _gc0Start As Integer
    Private _gc1Start As Integer
    Private _gc2Start As Integer

    Private Shared _updateSamples As Long
    Private Shared _drawSamples As Long

    Private Shared _updateTotalMs As Double
    Private Shared _drawTotalMs As Double

    Private Shared _updateMaxMs As Double
    Private Shared _drawMaxMs As Double

    Public Overrides Sub Entry(helper As IModHelper)

        _harmony = New Harmony(
            "Custom.StardewPerformanceOptimizer"
        )

        _gc0Start = GC.CollectionCount(0)
        _gc1Start = GC.CollectionCount(1)
        _gc2Start = GC.CollectionCount(2)

        InstallProfilerPatches()

        AddHandler helper.Events.GameLoop.UpdateTicked,
            AddressOf OnUpdateTicked

        Monitor.Log(
            "Stardew Performance Optimizer 2.0.0 initialized.",
            LogLevel.Info
        )

    End Sub

    Private Sub InstallProfilerPatches()

        Try
            Dim updateMethod As MethodBase =
                AccessTools.Method(
                    GetType(Game1),
                    "Update",
                    New Type() {GetType(GameTime)}
                )

            If updateMethod IsNot Nothing Then

                _harmony.Patch(
                    updateMethod,
                    New HarmonyMethod(
                        GetType(Entry),
                        NameOf(UpdatePrefix)
                    ),
                    New HarmonyMethod(
                        GetType(Entry),
                        NameOf(UpdatePostfix)
                    )
                )

                Monitor.Log(
                    "Game1.Update profiler installed.",
                    LogLevel.Info
                )

            Else
                Monitor.Log(
                    "Game1.Update method not found. Update profiler disabled.",
                    LogLevel.Warn
                )
            End If

        Catch ex As Exception
            Monitor.Log(
                "Failed to install Update profiler: " & ex.Message,
                LogLevel.Warn
            )
        End Try

        Try
            Dim drawMethod As MethodBase =
                AccessTools.Method(
                    GetType(Game1),
                    "Draw",
                    New Type() {GetType(GameTime)}
                )

            If drawMethod IsNot Nothing Then

                _harmony.Patch(
                    drawMethod,
                    New HarmonyMethod(
                        GetType(Entry),
                        NameOf(DrawPrefix)
                    ),
                    New HarmonyMethod(
                        GetType(Entry),
                        NameOf(DrawPostfix)
                    )
                )

                Monitor.Log(
                    "Game1.Draw profiler installed.",
                    LogLevel.Info
                )

            Else
                Monitor.Log(
                    "Game1.Draw method not found. Draw profiler disabled.",
                    LogLevel.Warn
                )
            End If

        Catch ex As Exception
            Monitor.Log(
                "Failed to install Draw profiler: " & ex.Message,
                LogLevel.Warn
            )
        End Try

    End Sub

    ' ==========================================
    ' UPDATE PROFILER
    ' ==========================================

    Private Shared Sub UpdatePrefix(
        ByRef __state As Long
    )
        __state = Stopwatch.GetTimestamp()
    End Sub

    Private Shared Sub UpdatePostfix(
        __state As Long
    )

        Dim elapsedMs As Double =
            (Stopwatch.GetTimestamp() - __state) *
            1000.0 / Stopwatch.Frequency

        If elapsedMs <= 0 Then Return

        _updateSamples += 1
        _updateTotalMs += elapsedMs

        If elapsedMs > _updateMaxMs Then
            _updateMaxMs = elapsedMs
        End If

    End Sub

    ' ==========================================
    ' DRAW PROFILER
    ' ==========================================

    Private Shared Sub DrawPrefix(
        ByRef __state As Long
    )
        __state = Stopwatch.GetTimestamp()
    End Sub

    Private Shared Sub DrawPostfix(
        __state As Long
    )

        Dim elapsedMs As Double =
            (Stopwatch.GetTimestamp() - __state) *
            1000.0 / Stopwatch.Frequency

        If elapsedMs <= 0 Then Return

        _drawSamples += 1
        _drawTotalMs += elapsedMs

        If elapsedMs > _drawMaxMs Then
            _drawMaxMs = elapsedMs
        End If

    End Sub

    ' ==========================================
    ' REPORTING
    ' ==========================================

    Private Sub OnUpdateTicked(
        sender As Object,
        e As UpdateTickedEventArgs
    )

        _ticksSinceReport += 1

        If _ticksSinceReport >= REPORT_INTERVAL_TICKS Then

            ReportPerformance()
            ResetReportWindow()

        End If

    End Sub

    Private Sub ReportPerformance()

        Dim updateAverage As Double = 0
        Dim drawAverage As Double = 0

        If _updateSamples > 0 Then
            updateAverage =
                _updateTotalMs / _updateSamples
        End If

        If _drawSamples > 0 Then
            drawAverage =
                _drawTotalMs / _drawSamples
        End If

        Dim managedMemoryMB As Double =
            GC.GetTotalMemory(False) /
            1024.0 / 1024.0

        Dim gc0 As Integer =
            GC.CollectionCount(0) - _gc0Start

        Dim gc1 As Integer =
            GC.CollectionCount(1) - _gc1Start

        Dim gc2 As Integer =
            GC.CollectionCount(2) - _gc2Start

        Monitor.Log(
            String.Format(
                "Performance Profiler | " &
                "Update Avg: {0:F2} ms | Update Max: {1:F2} ms | " &
                "Draw Avg: {2:F2} ms | Draw Max: {3:F2} ms | " &
                "Samples U/D: {4}/{5} | Memory: {6:F1} MB | " &
                "GC (0/1/2): {7}/{8}/{9}",
                updateAverage,
                _updateMaxMs,
                drawAverage,
                _drawMaxMs,
                _updateSamples,
                _drawSamples,
                managedMemoryMB,
                gc0,
                gc1,
                gc2
            ),
            LogLevel.Info
        )

    End Sub

    Private Sub ResetReportWindow()

        _ticksSinceReport = 0

        _updateSamples = 0
        _drawSamples = 0

        _updateTotalMs = 0
        _drawTotalMs = 0

        _updateMaxMs = 0
        _drawMaxMs = 0

        _gc0Start = GC.CollectionCount(0)
        _gc1Start = GC.CollectionCount(1)
        _gc2Start = GC.CollectionCount(2)

    End Sub

End Class
        
