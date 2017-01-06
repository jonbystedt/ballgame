using UnityEngine;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public static class Serialization 
{
	public static string saveFolderName = "worlds";

	public static DirectoryInfo SaveDirectory()
	{
		DirectoryInfo saveDirectory;
		string saveLocation = saveFolderName + "/" + World.Seed + "/";

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

	public static string SaveLocation()
	{
		string saveLocation = saveFolderName + "/" + World.Seed + "/";

		if (!Directory.Exists(saveLocation))
		{
			Directory.CreateDirectory(saveLocation);
		}

		return saveLocation;
	}

	public static string SaveName()
	{
		return saveFolderName + "/" + World.Seed;
	}

	public static string FileName(WorldPosition chunkLocation)
	{
		string fileName = chunkLocation.x + "," + chunkLocation.y + "," + chunkLocation.z + ".bin";
		return fileName;
	}

	public static void SaveChunk(Chunk chunk)
	{
		Save save = new Save(chunk);
		if (save.blocks.Count == 0)
		{
			return;
		}

		string saveFile = SaveLocation();
		saveFile += FileName(chunk.pos);

		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, save);
		stream.Close();
	}

	public static bool Load(Chunk chunk)
	{
		string saveFile = SaveLocation();
		saveFile += FileName(chunk.pos);

		if (!File.Exists (saveFile))
		{
			return false;
		}

		IFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(saveFile, FileMode.Open);

		Save save = (Save)formatter.Deserialize(stream);

		foreach (var block in save.blocks)
		{
			chunk._blocks[Chunk.BlockIndex(block.Key.x, block.Key.y, block.Key.z)] = block.Value;
		}
		stream.Close();
		return true;
	}

	public static void Compress()
	{
		string[] files = Directory.GetFiles(SaveLocation());

		if (files.Length == 0)
		{
			return;
		}

		FileStream saveFile = File.Create(SaveName() + ".world");
		ZipOutputStream zipStream = new ZipOutputStream(saveFile);

		for(int i = 0; i < files.Length; i++)
		{
			string fileName = files[i];
			FileInfo fi = new FileInfo(fileName);

			string entryName = fileName.Substring(World.Seed.Length - 1);
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
	}
}


