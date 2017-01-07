﻿using System.IO;
using System;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

public static class Serialization 
{
	public static string SaveFolderName = "worlds";
	public static string SettingsFileName = "ballgame_settings.config";

	static DirectoryInfo saveDirectory;
	static string saveLocation;
	static string fileName;
	static Save save;
	static string saveFile;
	static IFormatter formatter = new BinaryFormatter();
	static Serializer yaml = new Serializer();
	static FileStream fileStream;

	public static void Reset()
	{
		saveDirectory = null;
		saveLocation = "";
	}

	public static DirectoryInfo SaveDirectory
	{
		get
		{
			if (saveDirectory != null)
			{
				return saveDirectory;
			}

			saveLocation = SaveFolderName + "/" + World.Seed + "/";

			if (!Directory.Exists(saveLocation))
			{
				saveDirectory = Directory.CreateDirectory(saveLocation);
			}
			else
			{
				saveDirectory = new DirectoryInfo(saveLocation);
			}

			return saveDirectory;
		}
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
			}

			return saveLocation;
		}
	}

	public static string FileName(WorldPosition chunkLocation)
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

		saveFile = Path.Combine(SaveLocation, FileName(chunk.pos));

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
		saveFile = Path.Combine(SaveLocation, FileName(chunk.pos));

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

		string[] files = Directory.GetFiles(SaveLocation);

		if (files.Length == 0)
		{
			return;
		}

		using (fileStream = File.Create(SaveLocation + ".world"))
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

		Directory.Delete(SaveLocation, true);
	}

	public static void Decompress()
	{
		string saveFile = SaveLocation + ".world";

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
					string zipPath = Path.Combine(SaveLocation, fileName);

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
}


