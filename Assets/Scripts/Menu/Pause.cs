using UnityEngine;

public class Pause : MonoBehaviour 
{

	private ShowPanels showPanels;						
	private bool isPaused;		
	private bool playerActive = false;					
	private StartGame startScript;		
	
	void Awake()
	{
		showPanels = GetComponent<ShowPanels> ();
		startScript = GetComponent<StartGame> ();
	}

	void Update () 
	{
		if (GameInput.Pause && !isPaused && !startScript.inMainMenu) 
		{
			DoPause();
		} 
		else if (GameInput.Pause && isPaused && !startScript.inMainMenu) 
		{
			UnPause ();
		}
	}

	public void DoPause()
	{
		isPaused = true;

		if (Game.Active)
		{
			Time.timeScale = 0;
			Game.Loader.loading = false;
			Game.Active = false;
			playerActive = true;
		}

		showPanels.ShowPausePanel();
	}

	public void UnPause()
	{
		isPaused = false;

		if (playerActive)
		{
			Time.timeScale = 1;
			Game.Loader.loading = true;
			Game.Active = true;
			playerActive = false;
		}

		showPanels.HidePausePanel();
	}
}
