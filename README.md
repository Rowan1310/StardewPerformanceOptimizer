# Stardew Performance Optimizer 1.0.0

Target: Stardew Valley Android 1.6.x with SMAPI Android.
Project basis: the uploaded StardewFPS project settings (net6.0, ModBuildConfig 4.1.1).

## What this first version does
- Samples the time between SMAPI `UpdateTicked` events.
- Counts update gaps longer than 33.3 ms as possible stutter events.
- Reports maximum observed update gap, managed memory, and GC collection counts every 1800 update ticks.
- Ignores pauses longer than 2 seconds, such as app backgrounding.

## Important limitations
This is a diagnostic baseline, not an FPS booster yet. `UpdateTicked` timing is not a direct measurement of GPU-rendered FPS. `GC.GetTotalMemory(False)` reports .NET managed memory, not total Android RAM use. The mod does not force garbage collection, change frame limits, or patch game internals.

## Build
Build with the .NET SDK and the Stardew ModBuildConfig package version specified in the project file. Confirm the resulting assembly loads on the exact SMAPI Android build before adding active optimizations.
