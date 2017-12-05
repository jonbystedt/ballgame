using System.IO;
using System;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

public static class Serialization 
{
	public static string SaveFolderName = "ballgame_Worlds";
	public static string SettingsFileName = "ballgame_Settings.config";
	public static string NoiseSettingsFileName = "ballgame_Noise.config";

	static string saveLocation;
	static string fileName;
	static Save save;
	static string saveFile;
	static IFormatter formatter = new BinaryFormatter();
	static Serializer yaml = new Serializer();
	static FileStream fileStream;

	public static void Reset()
	{
		saveLocation = "";
	}

	public static string SaveLocation
	{
		get
		{
			if (!String.IsNullOrEmpty(saveLocation))
			{
				return saveLocation;
			}

			saveLocation = Path.Combine(SaveFolderName, World.Seed);

			if (!Directory.Exists(saveLocation))
			{
				Directory.CreateDirectory(saveLocation);
                Directory.CreateDirectory(Path.Combine(saveLocation, "chunks"));
            }

			return saveLocation;
		}
	}

	public static string FileName(World3 chunkLocation)
	{
		fileName = chunkLocation.x + "," + chunkLocation.y + "," + chunkLocation.z + ".bin";
		return fileName;
	}

	// TODO: Need to write to single file at offset as this won't scale?
	public static void SaveChunk(Chunk chunk)
	{
		save = new Save(chunk);
		if (save.blocks.Count == 0)
		{
			return;
		}

		saveFile = Path.Combine(SaveLocation, "chunks", FileName(chunk.pos));

		try
		{
			using (fileStream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				formatter.Serialize(fileStream, save);
			}
		}
		catch (Exception ex)
		{
			Game.Log(ex.Message);
		}

	}

	public static bool Load(Chunk chunk)
	{
		saveFile = Path.Combine(SaveLocation, "chunks", FileName(chunk.pos));

		if (!File.Exists(saveFile))
		{
			return false;
		}

		using (fileStream = new FileStream(saveFile, FileMode.Open))
		{
			save = (Save)formatter.Deserialize(fileStream);

			foreach (var block in save.blocks)
			{
				chunk._blocks[Chunk.BlockIndex(block.Key.x, block.Key.y, block.Key.z)] = block.Value;
			}
		}

		return true;
	}

	public static void Compress()
	{
		if (String.IsNullOrEmpty(World.Seed))
		{
			return;
		}

		string[] files = Directory.GetFiles(Path.Combine(SaveLocation, "chunks"));

		if (files.Length == 0)
		{
			Directory.Delete(SaveLocation, true);
			return;
		}

        using (fileStream = File.Create(Path.Combine(SaveLocation, "chunks.bin")))
        {
            ZipOutputStream zipStream = new ZipOutputStream(fileStream);
            zipStream.SetLevel(3);

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = files[i];
                FileInfo fi = new FileInfo(fileName);

                string entryName = fileName.Substring(SaveLocation.Length);
                entryName = ZipEntry.CleanName(entryName);
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(fileName))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }

            zipStream.IsStreamOwner = true;
            zipStream.Close();
        }

		Directory.Delete(Path.Combine(SaveLocation, "chunks"), true);
	}

	public static void Decompress()
	{
        string saveFile = Path.Combine(SaveLocation, "chunks.bin");

        if (!File.Exists(saveFile))
		{
			return;
		}

		ZipFile zipFile = null;

		try
		{
			using (fileStream = File.OpenRead(saveFile))
			{
				zipFile = new ZipFile(fileStream);

				foreach (ZipEntry entry in zipFile)
				{
					if (!entry.IsFile)
					{
						continue;
					}
					fileName = entry.Name;

					byte[] buffer = new byte[4096];
					Stream zipStream =  zipFile.GetInputStream(entry);
					string zipPath = Path.Combine(SaveLocation, "chunks", fileName);

					using (FileStream streamWriter = File.Create(zipPath))
					{
						StreamUtils.Copy(zipStream, streamWriter, buffer);
					}
				}
			}

			File.Delete(saveFile);
		} 
		finally
		{
			if (zipFile != null)
			{
				zipFile.IsStreamOwner = true;
				zipFile.Close();
			}
		}
	}

	public static void WriteConfig()
	{
        var sb = new StringBuilder();
        var stringWriter = new StringWriter(sb);
        yaml.Serialize(stringWriter, Config.Settings);

		using (StreamWriter sw = File.CreateText(SettingsFileName)) 
		{
			sw.WriteLine(sb.ToString());
		} 
	}

	public static bool ReadConfig()
	{
		if (!File.Exists(SettingsFileName))
		{
			return false;
		}

		var sr = new StringReader(System.IO.File.ReadAllText(SettingsFileName));
        var deserializer = new Deserializer();
        Config.Settings = deserializer.Deserialize<GameConfig>(sr);

		return true;
	}

    public static void WriteWorldConfig()
    {
        string worldSettingsFileName = Path.Combine(SaveLocation, "world.config");

        var sb = new StringBuilder();
        var stringWriter = new StringWriter(sb);
        yaml.Serialize(stringWriter, Config.Instance);

        using (StreamWriter sw = File.CreateText(worldSettingsFileName))
        {
            sw.WriteLine(sb.ToString());
        }
    }

    public static bool ReadWorldConfig()
    {
        string worldSettingsFileName = Path.Combine(SaveLocation, "world.config");

        if (!File.Exists(worldSettingsFileName))
        {
            return false;
        }

        var sr = new StringReader(System.IO.File.ReadAllText(worldSettingsFileName));
        var deserializer = new Deserializer();
        Config.Instance = deserializer.Deserialize<WorldSettings>(sr);

        return true;
    }

    public static void WriteWorldHash()
    {
        string worldHashFileName = Path.Combine(SaveLocation,"world.hash");

        var hash = new int[256];
        Array.Copy(GameUtils.Hash, 0, hash, 0, 256);

        var sb = new StringBuilder();
        var stringWriter = new StringWriter(sb);
        yaml.Serialize(stringWriter, hash);

        using (StreamWriter sw = File.CreateText(worldHashFileName))
        {
            sw.WriteLine(sb.ToString());
        }
    }

    public static bool ReadWorldHash()
    {
        string worldHashFileName = Path.Combine(SaveLocation, "world.hash");

        if (!File.Exists(worldHashFileName))
        {
            return false;
        }

        var sr = new StringReader(System.IO.File.ReadAllText(worldHashFileName));
        var deserializer = new Deserializer();
        int[] hash = deserializer.Deserialize<int[]>(sr);

        hash.CopyTo(GameUtils.Hash, 0);
        hash.CopyTo(GameUtils.Hash, hash.Length);

        return true;
    }

    public static void WriteWorldColors()
    {
        string worldColorFileName = Path.Combine(SaveLocation, "world.colors");

        var colors = new SaveColor[64];
        for (int i = 0; i < 64; i++)
        {
            colors[i] = new SaveColor(Tile.Colors[i].r, Tile.Colors[i].g, Tile.Colors[i].b);
        }

        var sb = new StringBuilder();
        var stringWriter = new StringWriter(sb);
        yaml.Serialize(stringWriter, colors);

        using (StreamWriter sw = File.CreateText(worldColorFileName))
        {
            sw.WriteLine(sb.ToString());
        }
    }

    public static bool ReadWorldColors()
    {
        string worldColorFileName = Path.Combine(SaveLocation, "world.colors");

        if (!File.Exists(worldColorFileName))
        {
            return false;
        }

        var sr = new StringReader(System.IO.File.ReadAllText(worldColorFileName));
        var deserializer = new Deserializer();
        SaveColor[] colors = deserializer.Deserialize<SaveColor[]>(sr);

        for (int i = 0; i < 64; i++)
        {
            Tile.Colors[i] = new UnityEngine.Color(colors[i].r, colors[i].g, colors[i].b);
        }

        return true;
    }

    public static void WriteNoiseConfig()
	{
		var sb = new StringBuilder();
        var stringWriter = new StringWriter(sb);
        yaml.Serialize(stringWriter, Config.Noise);

		using (StreamWriter sw = File.CreateText(NoiseSettingsFileName)) 
		{
			sw.WriteLine(sb.ToString());
		} 
	}

	public static bool ReadNoiseConfig()
	{
		if (!File.Exists(NoiseSettingsFileName))
		{
			return false;
		}

		var sr = new StringReader(System.IO.File.ReadAllText(NoiseSettingsFileName));
		var deserializer = new Deserializer();
        Config.Noise = deserializer.Deserialize<NoiseSettings>(sr);

		return true;
	}
}


