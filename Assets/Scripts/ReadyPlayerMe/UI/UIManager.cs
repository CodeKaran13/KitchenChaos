using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public enum Screen {
		None,
		PlayerSelection,
		MainMenu
	}


	[SerializeField] private MainMenuUI mainMenuUI;
	[SerializeField] private PlayerSelectionUI playerSelectionUI;

	private Screen activeScreen;

	private void Start() {
		ShowPlayerSelection();
	}

	public void ShowPlayerSelection() {
		CloseActiveScreen();
		playerSelectionUI.ShowUI();
		activeScreen = Screen.PlayerSelection;
	}

	private void HidePlayerSelection() {
		playerSelectionUI.HideUI();
	}

	public void ShowMainMenu() {
		CloseActiveScreen();
		mainMenuUI.ShowUI();
		activeScreen = Screen.MainMenu;
	}

	private void HideMainMenu() {
		mainMenuUI.HideUI();
	}

	private void CloseActiveScreen() {
		switch (activeScreen) {
			case Screen.None:
				break;
			case Screen.PlayerSelection:
				HidePlayerSelection();
				break;
			case Screen.MainMenu:
				HideMainMenu();
				break;
		}
	}
}
