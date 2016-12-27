using UnityEngine;

//Attach this to a camera
public class Pixellate : MonoBehaviour {
		
	//how chunky to make the screen
	public int pixelSize = 4;
	public FilterMode filterMode = FilterMode.Point;
	public Camera[] otherCameras;
	private Material mat;
	private Camera _camera;
	Texture2D tex;
	
	void Start () 
	{
		_camera = GetComponent<Camera>();
		_camera.pixelRect = new Rect(0,0,Screen.width/pixelSize,Screen.height/pixelSize);
		for (int i = 0; i < otherCameras.Length; i++)
			otherCameras[i].pixelRect = new Rect(0,0,Screen.width/pixelSize,Screen.height/pixelSize);
	}
	
	void OnGUI()
	{
		if (Event.current.type == EventType.Repaint)
			Graphics.DrawTexture(new Rect(0,0,Screen.width, Screen.height), tex);
	}
	

	void OnPostRender()
	{
		if(!mat) {
			Shader shader = Shader.Find("Hidden/SetAlpha");
			mat = new Material(shader); 
		}
		// Draw a quad over the whole screen with the above shader
		GL.PushMatrix ();
		GL.LoadOrtho ();
		for (var i = 0; i < mat.passCount; ++i) {
			mat.SetPass (i);
			GL.Begin( GL.QUADS );
			GL.Vertex3( 0, 0, 0.1f );
			GL.Vertex3( 1, 0, 0.1f );
			GL.Vertex3( 1, 1, 0.1f );
			GL.Vertex3( 0, 1, 0.1f );
			GL.End();
		}
		GL.PopMatrix ();	
		
		
		DestroyImmediate(tex);

		tex = new Texture2D(Mathf.FloorToInt(_camera.pixelWidth), Mathf.FloorToInt(_camera.pixelHeight));
		tex.filterMode = filterMode;
		tex.ReadPixels(new Rect(0, 0, _camera.pixelWidth, _camera.pixelHeight), 0, 0);
		tex.Apply();
	}

}