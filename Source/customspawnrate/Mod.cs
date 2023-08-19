using System;
using Verse;
using HarmonyLib;
using UnityEngine;
using static CustomPawnSpawnRate.SpawnRateSettings;

namespace CustomPawnSpawnRate
{
    public class SpawnRateMod : Mod
    {
        public SpawnRateMod(ModContentPack content) : base(content)
        {
            // Load settings from file
            base.GetSettings<SpawnRateSettings>();

            // Patch with Harmony
            Harmony harmony = new Harmony("iftclerk.wpspawnrate");
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "ChanceToRedressAnyWorldPawn"),
                postfix: new HarmonyMethod(typeof(SpawnRatePatches), nameof(SpawnRatePatches.PatchSpawnRate)));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.Planet.WorldPawnGC), "GetCriticalPawnReason"),
                postfix: new HarmonyMethod(typeof(SpawnRatePatches), nameof(SpawnRatePatches.PatchWorldPawnGC)));
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "GenerateOrRedressPawnInternal"),
                prefix: new HarmonyMethod(typeof(SpawnRatePatches), nameof(SpawnRatePatches.PatchPawnGenerator)));

            Log.Message("World pawns spawn rates patched.");
        }

        // Settings GUI
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Minimum spawn rate for custom pawns (old method, keep at 0 to disable): " + Math.Round(minSpawnRate, 2));
            minSpawnRate = listingStandard.Slider(minSpawnRate, 0.0f, 1.0f);
            listingStandard.Label("Forced spawn rate for custom pawns: " + Math.Round(probSpawnRate, 2));
            probSpawnRate = listingStandard.Slider(probSpawnRate, 0.0f, 1.0f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        // Show in settings menu
        public override string SettingsCategory()
        {
            return "CustomPawnSpawnRate";
        }
    }
}
