using UnityEngine;

public class QuitApplication : MonoBehaviour {

	public void Quit()
	{
		World.DestroyChunks();
		Serialization.Compress();
		Serialization.WriteConfig();

	#if UNITY_STANDALONE
		Application.Quit();
	#endif


	#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
	#endif
	}
}
