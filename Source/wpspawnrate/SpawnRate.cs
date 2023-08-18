using System;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace WorldPawnSpawnRate
{
    [StaticConstructorOnStartup]
    public static class WorldPawnSpawnRate 
    {
        static WorldPawnSpawnRate()
        {
            Harmony harmony = new Harmony("rimworld.mod.iftclerk.wpspawnrate");
            harmony.PatchAll();

            Log.Message("World pawns spawn rates patched.");
        }
    }

    [HarmonyPatch]
    public static class FixWorldPawnSpawnRate
    {
        [HarmonyPatch(typeof(PawnGenerator), "ChanceToRedressAnyWorldPawn")]
        [HarmonyPostfix]
        public static void FixSpawnRate(ref float __result)
        {
            __result = Mathf.Max(0.8f, __result);
        }
    }
}
