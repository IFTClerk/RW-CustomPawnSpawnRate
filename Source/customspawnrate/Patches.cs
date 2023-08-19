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
        static MethodInfo validPawn = AccessTools.Method(typeof(Verse.PawnGenerator), "IsValidCandidateToRedress");
        
        private static bool _IsCustomPawn(Pawn pawn)
        {
            if (pawn.Ideo == Find.FactionManager.OfPlayer.ideos.PrimaryIdeo)
            {
                return true;
            }
            return false;
        }

        // The patches
        public static void PatchSpawnRate(ref float __result)
        {
            __result = Mathf.Max(SpawnRateSettings.minSpawnRate, __result);
        }
        public static void PatchWorldPawnGC(ref string __result, Pawn pawn)
        {
            if (_IsCustomPawn(pawn))
            {
                __result = "CustomPawn";
            }
        }
        public static bool PatchPawnGenerator(ref Pawn __result, PawnGenerationRequest request)
        {
            if (request.AllowedDevelopmentalStages.Newborn() || request.ForceGenerateNewPawn)
            {
                // Run original method if game wants new pawn
                return true;
            }

            IEnumerable<Pawn> customPawns = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free).Where((Pawn p) => _IsCustomPawn(p));
            bool oldParam = request.WorldPawnFactionDoesntMatter;
            request.WorldPawnFactionDoesntMatter = true;
            IEnumerable<Pawn> freePawns = customPawns.Where((Pawn p) => (bool)validPawn.Invoke(null, new object[] { p, request }));
            if (Rand.Chance(SpawnRateSettings.probSpawnRate) && freePawns.TryRandomElementByWeight((Pawn x) => 1.0f, out Pawn pawn))
            {
                // Debug
                //Log.Message("[WP]Got world pawn, redressing.");

                Verse.PawnGenerator.RedressPawn(pawn, request);
                Find.WorldPawns.RemovePawn(pawn);

                pawn.Ideo?.Notify_MemberGenerated(pawn, request.AllowedDevelopmentalStages.Newborn());
                Find.Scenario?.Notify_PawnGenerated(pawn, request.Context, true);

                __result = pawn;
                request.WorldPawnFactionDoesntMatter = oldParam;
                return false;
            }
            // Debug
            //Log.Message("[WP]Did not get pawn, proceeding to original method.");
            request.WorldPawnFactionDoesntMatter = oldParam;
            return true;
        }


    }
}
