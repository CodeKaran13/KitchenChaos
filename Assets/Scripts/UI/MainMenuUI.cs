using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

	[SerializeField] private GameObject uiContainer;

	[SerializeField] private Button playButton;
	[SerializeField] private Button quitButton;
	[SerializeField] private Button logoutButton;


	private void Awake()
	{
		playButton.onClick.AddListener(() =>
		{
			Loader.Load(Loader.Scene.GameScene);
		});

		quitButton.onClick.AddListener(() =>
		{
			Application.Quit();
		});

		logoutButton.onClick.AddListener(delegate
		{
			PlayerPrefs.DeleteKey("Session-Id");
			PlayerPrefs.DeleteKey("Challenge-Id");
			PlayerPrefs.DeleteKey("etag");

			SceneManager.LoadScene(0);
		});

		Time.timeScale = 1.0f;
	}

	public void ShowUI()
	{
		uiContainer.SetActive(true);
	}

	public void HideUI()
	{
		uiContainer.SetActive(false);
	}
}