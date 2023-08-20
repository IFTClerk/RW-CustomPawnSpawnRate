using System;
using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection;

namespace CustomPawnSpawnRate
{
    public static class SpawnRatePatches
    {
        // C# Reflection to get pawn validation method
        private static MethodInfo validPawn = AccessTools.Method(typeof(Verse.PawnGenerator), "IsValidCandidateToRedress");
        
        // Helper function to find custom pawns via their ideology
        private static bool _IsCustomPawn(Pawn pawn)
        {
            if (pawn.Ideo == Find.FactionManager.OfPlayer.ideos.PrimaryIdeo)
            {
                return true;
            }
            return false;
        }

        // The patches
        // Patch minimum probablity to redress any world pawns, obsolete
        public static void PatchSpawnRate(ref float __result)
        {
            __result = Mathf.Max(SpawnRateSettings.minSpawnRate, __result);
        }
        // Patches weights to never select custom pawns in the original method
        // This is to make sure that the game doesn't try to generate more custom pawns than it should
        public static void PatchSelectionWeight(ref float __result, Pawn p)
        {
            if (p.Ideo == Find.FactionManager.OfPlayer.ideos.PrimaryIdeo)
            {
                __result = 0.0f;
            }
        }
        // Do not Garbage Collect custom pawns
        public static void PatchWorldPawnGC(ref string __result, Pawn pawn)
        {
            if (_IsCustomPawn(pawn))
            {
                __result = "CustomPawn";
            }
        }
        // Add prefix method to try spawn custom pawns before anything else
        public static bool PatchPawnGenerator(ref Pawn __result, PawnGenerationRequest request)
        {
            if (request.AllowedDevelopmentalStages.Newborn() || request.ForceGenerateNewPawn)
            {
                // Run original method if game wants new pawn
                return true;
            }

            IEnumerable<Pawn> customPawns = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free).Where((Pawn p) => _IsCustomPawn(p));
            // Somehow this makes a clone of the request object???
            PawnGenerationRequest newReq = request;
            // Modify the request for custom pawns
            newReq.WorldPawnFactionDoesntMatter = true; // Custom pawns can spawn cross-faction
            newReq.FixedIdeo = Find.FactionManager.OfPlayer.ideos.PrimaryIdeo; // Prevents the game from overriding the ideology when spawning in wrong faction
            // Filter available pawns by new request
            IEnumerable<Pawn> freePawns = customPawns.Where((Pawn p) => (bool)validPawn.Invoke(null, new object[] { p, newReq }));
            // Try spawning custom pawns with equal weight
            if (Rand.Chance(SpawnRateSettings.probSpawnRate) && freePawns.TryRandomElementByWeight((Pawn x) => 1.0f, out Pawn pawn))
            {
                // Debug
                //Log.Message("[WP]Got world pawn, redressing.");

                // Same processing as original method
                Verse.PawnGenerator.RedressPawn(pawn, newReq);
                Find.WorldPawns.RemovePawn(pawn);

                pawn.Ideo?.Notify_MemberGenerated(pawn, newReq.AllowedDevelopmentalStages.Newborn());
                Find.Scenario?.Notify_PawnGenerated(pawn, newReq.Context, true);

                __result = pawn;
                return false;
            }
            // Debug
            //Log.Message("[WP]Did not get pawn, proceeding to original method.");
            return true;
        }


    }
}
