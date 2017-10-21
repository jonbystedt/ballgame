using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralToolkit;

public class GlassFactory : MonoBehaviour 
{
	MeshDraft template;
	MeshDraft draft;
	Mesh mesh;

	public void Start()
	{
		template = MeshDraft.Plane();
	}

	public void SpawnPane(World3 block, Block.Direction side, Color color)
	{
		template.colors.Clear();

		for (int i = 1; i < template.vertices.Count; i++)
		{
			template.colors.Add(color);
		}
	}
}
