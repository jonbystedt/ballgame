using System.Collections.Generic;

public class Column 
{
	public bool spawned;
	public bool rendered;

	public List<PooledObject> spawns;
	public List<WorldPosition> chunks;

	public Region region;
	
	public Column(Region r, Chunk[] _chunks)
	{
		region = r;
		spawned = false;
		rendered = false;
		spawns = new List<PooledObject>();
		chunks = new List<WorldPosition>();

		for(int i = 0; i < _chunks.Length; i++)
		{
			_chunks[i].column = this;
			chunks.Add(_chunks[i].pos);
		}
	}

	public void SpawnColumn(WorldPosition pos, SpawnManager spawn)
	{
		spawn.SpawnColumn(pos, region, spawns);
		spawned = true;
	}

	public void AddSpawn(PooledObject spawn)
	{
		if (spawns == null)
		{
			spawns = new List<PooledObject>();
		}
		spawns.Add(spawn);
	}
}


