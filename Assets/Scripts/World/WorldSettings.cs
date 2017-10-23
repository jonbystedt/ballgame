using System;

[Serializable]
public class TerrainSettings
{
    public NoiseOptions terrain { get; set; }
    public NoiseOptions mountain { get; set; }
    public NoiseOptions cave { get; set; }
    public NoiseOptions pattern { get; set; }
    public NoiseOptions stripe { get; set; }

    public NoiseOptions[] driftMaps { get; set; }

    public string flags { get; set; }
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
