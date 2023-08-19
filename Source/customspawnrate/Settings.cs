using System;
using Verse;

namespace CustomPawnSpawnRate
{
    public class SpawnRateSettings : ModSettings
    {
        public static float minSpawnRate = 0.0f;
        public static float probSpawnRate = 0.5f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref minSpawnRate, "minSpawnRate", 0.0f);
            Scribe_Values.Look(ref probSpawnRate, "probSpawnRate", 0.5f);
            base.ExposeData();
        }
    }

}
