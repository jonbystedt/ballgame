using System;

[Serializable]
public class TerrainSettings
{
    public NoiseOptions hills { get; set; }
    public NoiseOptions mountain { get; set; }
    public NoiseOptions cave { get; set; }
    public NoiseOptions pattern { get; set; }
    public NoiseOptions stripe { get; set; }

    public NoiseOptions[] driftMaps { get; set; }

    public NoiseType hillType { get; set; }
    public NoiseType mountainType { get; set; }
    public NoiseType caveType { get; set; }
    public NoiseType patternType { get; set; }
    public NoiseType stripeType { get; set; }
    public NoiseType driftMapType { get; set; }

    public string flags { get; set; }

    public int beachHeight { get; set; }
    public float beachPersist { get; set; }
    public int cloudEase { get; set; }

    public int caveBreak { get; set; }
    public int patternBreak { get; set; }
    public int stripeBreak { get; set; }
    public int patternStripeBreak { get; set; }
    public int cloudBreak { get; set; }
    public int islandBreak { get; set; }
    public int trans1 { get; set; }
    public int trans2 { get; set; }
    public int modScale { get; set; }

    public float stretch { get; set; }
    public float squish { get; set; }
    public float patternAmount { get; set; }
}

[Serializable]
public class SpawnSettings
{
    public NoiseOptions type { get; set; }
    public NoiseOptions frequency { get; set; }
    public NoiseOptions intensity { get; set; }
}

[Serializable]
public class BoidSettings
{
    public NoiseOptions interaction { get; set; }
    public NoiseOptions distance { get; set; }
    public NoiseOptions alignment { get; set; }
}

[Serializable]
public class EnvironmentSettings
{
    public NoiseOptions rain { get; set; }
    public NoiseOptions lightning { get; set; }
    public NoiseOptions key { get; set; }
    public BoidSettings boids { get; set; }

    public int rainBreak { get; set; }
    public int lightningBreak { get; set; }

    public EnvironmentSettings()
    {
        boids = new BoidSettings();
    }
}

[Serializable]
public class WorldSettings
{
    public TerrainSettings terrain { get; set; }
    public SpawnSettings spawns { get; set; }
    public EnvironmentSettings environment { get; set; }

    public WorldSettings()
    {
        terrain = new TerrainSettings();
        spawns = new SpawnSettings();
        environment = new EnvironmentSettings();
    }
}
