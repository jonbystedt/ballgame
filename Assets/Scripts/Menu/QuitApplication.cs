using UnityEngine;

public class QuitApplication : MonoBehaviour {

	public void Quit()
	{

	#if UNITY_STANDALONE
		Application.Quit();
	#endif


	#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
	#endif

	}

	void OnApplicationQuit()
	{
		if (Config.CoroutineTiming == 100000)
		{
			Config.CoroutineTiming = 20000;
		}
		World.DestroyChunks();
        Serialization.WriteWorldConfig();
        Serialization.Compress();
		Serialization.WriteConfig();
		Serialization.WriteNoiseConfig();
	}
}
