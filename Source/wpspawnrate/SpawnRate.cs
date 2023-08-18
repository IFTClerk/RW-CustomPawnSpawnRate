using System;
using Verse;
using HarmonyLib;
using UnityEngine;
using static WorldPawnSpawnRate.SpawnRateSettings;

namespace WorldPawnSpawnRate
{
    public class SpawnRateSettings : ModSettings
    {
        public static float minSpawnRate = 0.8f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref minSpawnRate, "minSpawnRate", 0.8f);
            base.ExposeData();
        }
    }

    public class SpawnRateMod : Mod
    {
        // SpawnRateSettings settings;

        public SpawnRateMod(ModContentPack content) : base(content)
        {
            base.GetSettings<SpawnRateSettings>();

            // Patch with Harmony
            Harmony harmony = new Harmony("iftclerk.wpspawnrate");
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "ChanceToRedressAnyWorldPawn"),
                postfix: new HarmonyMethod(typeof(SpawnRateMod), nameof(PatchSpawnRate)));
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "WorldPawnSelectionWeight"),
                postfix: new HarmonyMethod(typeof(SpawnRateMod), nameof(PatchWeight)));

            Log.Message("World pawns spawn rates patched.");
        }

        // The patches
        public static void PatchSpawnRate(ref float __result)
        {
            __result = Mathf.Max(minSpawnRate, __result);
        }
        public static void PatchWeight(ref float __result)
        {
            __result = 1.0f;
        }

        // Settings GUI
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Minimum spawn rate for world pawns: " + Math.Round(minSpawnRate, 2));
            minSpawnRate = listingStandard.Slider(minSpawnRate, 0.0f, 1.0f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        // Show in settings menu
        public override string SettingsCategory()
        {
            return "WorldPawnSpawnRate";
        }
    }
}
