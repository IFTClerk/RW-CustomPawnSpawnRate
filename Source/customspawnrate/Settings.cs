using System;
using Verse;

namespace CustomPawnSpawnRate
{
    public class SpawnRateSettings : ModSettings
    {
        public static float minSpawnRate = 0.0f; // Minimum spawn rate of world pawns (obsolete)
        public static float probSpawnRate = 0.5f; // Spawn rate of custom pawns

        public override void ExposeData()
        {
            Scribe_Values.Look(ref minSpawnRate, "minSpawnRate", 0.0f);
            Scribe_Values.Look(ref probSpawnRate, "probSpawnRate", 0.5f);
            base.ExposeData();
        }
    }

}
