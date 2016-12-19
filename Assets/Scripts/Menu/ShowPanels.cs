using UnityEngine;
using System.Collections;

public class ShowPanels : MonoBehaviour {

	public GameObject optionsPanel;					
	public GameObject optionsTint;				
	public GameObject menuPanel;
	public GameObject pausePanel;
	public GameObject pauseTint;
	public GameObject menuBackground;

	public StartGame launcher;

	public void ShowOptionsPanel()
	{
		optionsPanel.SetActive(true);
		optionsTint.SetActive(true);
	}

	public void HideOptionsPanel()
	{
		optionsPanel.SetActive(false);
		optionsTint.SetActive(false);
	}

	public void ShowMenu()
	{
		menuPanel.SetActive(true);
		menuBackground.SetActive(true);
		launcher.Reactivate();
	}
		
	public void HideMenu()
	{
		menuPanel.SetActive(false);
		menuBackground.SetActive(false);
	}

	public void ShowPausePanel()
	{
		pausePanel.SetActive (true);
		pauseTint.SetActive(true);
	}
		
	public void HidePausePanel()
	{
		pausePanel.SetActive(false);
		pauseTint.SetActive(false);

	}
}
