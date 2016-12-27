using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshData {

	public List<Vector3> vertices = new List<Vector3>();
	public List<int> triangles = new List<int>();
	public List<Vector2> uv = new List<Vector2>();
	public List<Vector3> colliderVerts = new List<Vector3>();
	public List<int> colliderTris = new List<int>();

	public bool complete = false;

	public MeshData() {}

	public void Clear()
	{
		vertices.Clear();
		triangles.Clear();
		uv.Clear();
		colliderVerts.Clear();
		colliderTris.Clear();
		complete = false;
	}

	public void AddVertex(Vector3 vertex)
	{
		vertices.Add (vertex);
		colliderVerts.Add(vertex);
	}

	public void AddTriangle(int tri)
	{
		triangles.Add(tri);
		colliderTris.Add (tri - (vertices.Count - colliderVerts.Count));
	}

	public void AddQuadTriangles()
	{
		triangles.Add(vertices.Count - 4);
		triangles.Add(vertices.Count - 3);
		triangles.Add(vertices.Count - 2);

		triangles.Add(vertices.Count - 4);
		triangles.Add(vertices.Count - 2);
		triangles.Add(vertices.Count - 1);

		colliderTris.Add(colliderVerts.Count - 4);
		colliderTris.Add(colliderVerts.Count - 3);
		colliderTris.Add(colliderVerts.Count - 2);
		
		colliderTris.Add(colliderVerts.Count - 4);
		colliderTris.Add(colliderVerts.Count - 2);
		colliderTris.Add(colliderVerts.Count - 1);
	}
}
