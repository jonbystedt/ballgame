using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class TransparentChunk : PooledObject {
	
	public static int Size = 16;
	
	public MeshFilter filter;
	private MeshCollider col;

	//private Mesh[] panes;
	
	//Use this for initialization
	void Start() 
	{
		filter = gameObject.GetComponent<MeshFilter>();
		col = gameObject.GetComponent<MeshCollider>();
	}
	
	//Sends the calculated mesh information 
	//to the mesh and collision components
	public void RenderMesh(MeshData meshData)
	{
		filter.mesh.Clear();
		filter.mesh.vertices = meshData.vertices.ToArray();
		filter.mesh.triangles = meshData.triangles.ToArray();
		filter.mesh.uv = meshData.uv.ToArray();
		filter.mesh.RecalculateNormals();
		filter.mesh.RecalculateBounds();
		;

//		for (int i = 0; i*4 < meshData.vertices.Count; i++)
//		{
//			Mesh pane = new Mesh();
//			pane.vertices = meshData.vertices.GetRange(i*4,4);
//			pane.triangles = meshData.triangles.GetRange(i*6,6);
//			pane.uv = meshData.uv.GetRange(i*4,4);
//			pane.RecalculateNormals();
//		}
		
		col.sharedMesh = null;
		Mesh mesh = new Mesh();
		mesh.vertices = meshData.colliderVerts.ToArray();
		mesh.triangles = meshData.colliderTris.ToArray();
		mesh.RecalculateNormals();
		col.sharedMesh = mesh;
	}
}
