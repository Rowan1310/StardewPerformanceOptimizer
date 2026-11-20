Imports System
Imports System.Diagnostics
Imports StardewModdingAPI
Imports StardewModdingAPI.Events

Public Class Entry
    Inherits [Mod]

    Private Const REPORT_INTERVAL_TICKS As Integer = 1800
    Private Const STUTTER_THRESHOLD_MS As Double = 33.3
    Private Const IGNORE_LONG_PAUSE_MS As Double = 2000.0

    Private _ticksSinceReport As Integer
    Private _lastTimestamp As Long
    Private _maxUpdateGapMs As Double
    Private _stutterCount As Integer

    Private _gc0Start As Integer
    Private _gc1Start As Integer
    Private _gc2Start As Integer

    Public Overrides Sub Entry(helper As IModHelper)
        _lastTimestamp = Stopwatch.GetTimestamp()

        _gc0Start = GC.CollectionCount(0)
        _gc1Start = GC.CollectionCount(1)
        _gc2Start = GC.CollectionCount(2)

        AddHandler helper.Events.GameLoop.UpdateTicked,
            AddressOf OnUpdateTicked

        Monitor.Log(
            "Stardew Performance Optimizer 1.0.0 initialized. Diagnostic mode only; no gameplay settings are changed.",
            LogLevel.Info
        )
    End Sub

    Private Sub OnUpdateTicked(sender As Object, e As UpdateTickedEventArgs)
        Dim now As Long = Stopwatch.GetTimestamp()
        Dim elapsedMs As Double =
            (now - _lastTimestamp) * 1000.0 / Stopwatch.Frequency

        _lastTimestamp = now

        ' Ignore long pauses, such as app backgrounding or a suspended game.
        ' These are not useful indicators of normal gameplay stuttering.
        If elapsedMs <= 0 OrElse elapsedMs > IGNORE_LONG_PAUSE_MS Then
            Return
        End If

        If elapsedMs > _maxUpdateGapMs Then
            _maxUpdateGapMs = elapsedMs
        End If

        If elapsedMs > STUTTER_THRESHOLD_MS Then
            _stutterCount += 1
        End If

        _ticksSinceReport += 1

        If _ticksSinceReport >= REPORT_INTERVAL_TICKS Then
            ReportPerformance()
            ResetReportWindow()
        End If
    End Sub

    Private Sub ReportPerformance()
        Dim managedMemoryMB As Double =
            GC.GetTotalMemory(False) / 1024.0 / 1024.0

        Dim gc0 As Integer = GC.CollectionCount(0) - _gc0Start
        Dim gc1 As Integer = GC.CollectionCount(1) - _gc1Start
        Dim gc2 As Integer = GC.CollectionCount(2) - _gc2Start

        Monitor.Log(
            String.Format(
                "Performance | Managed memory: {0:F1} MB | Max update gap: {1:F1} ms | Gaps >33.3ms: {2} | GC collections (0/1/2): {3}/{4}/{5}",
                managedMemoryMB,
                _maxUpdateGapMs,
                _stutterCount,
                gc0,
                gc1,
                gc2
            ),
            LogLevel.Info
        )
    End Sub

    Private Sub ResetReportWindow()
        _ticksSinceReport = 0
        _maxUpdateGapMs = 0
        _stutterCount = 0

        _gc0Start = GC.CollectionCount(0)
        _gc1Start = GC.CollectionCount(1)
        _gc2Start = GC.CollectionCount(2)
    End Sub
End Class
