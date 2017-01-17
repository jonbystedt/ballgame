using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowPanels : MonoBehaviour {

	public GameObject optionsPanel;					
	public GameObject optionsTint;				
	public GameObject menuPanel;
	public GameObject pausePanel;
	public GameObject pauseTint;
	public GameObject menuBackground;
	public Slider pauseMusicSlider;
	public Slider pauseSfxSlider;
	public Slider optionsMusicSlider;
	public Slider optionsSfxSlider;

	public StartGame launcher;

	public void ShowOptionsPanel()
	{
		optionsMusicSlider.value = Config.MusicVolume * 0.01f;
		optionsSfxSlider.value = Config.SfxVolume * 0.01f;
		optionsPanel.SetActive(true);
		//optionsTint.SetActive(true);
	}

	public void HideOptionsPanel()
	{
		optionsPanel.SetActive(false);
		//optionsTint.SetActive(false);
	}

	public void ShowMenu()
	{
		menuPanel.SetActive(true);
		//optionsTint.SetActive(true);
		menuBackground.SetActive(true);
		launcher.Reactivate();
	}
		
	public void HideMenu()
	{
		menuPanel.SetActive(false);
		//optionsTint.SetActive(false);
		menuBackground.SetActive(false);
	}

	public void ShowPausePanel()
	{
		pauseMusicSlider.value = Config.MusicVolume * 0.01f;
		pauseSfxSlider.value = Config.SfxVolume * 0.01f;
		pausePanel.SetActive(true);
		pauseTint.SetActive(true);
	}
		
	public void HidePausePanel()
	{
		pausePanel.SetActive(false);
		pauseTint.SetActive(false);
	}
}
