using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour {

	[SerializeField] private GameObject uiContainer;
	[SerializeField] private Button agreeButton;
	[SerializeField] private Button disagreeButton;

	private Action<bool> caller;

	private void OnEnable() {
		agreeButton.onClick.AddListener(delegate { OnAgreeButtonClick(); });
		disagreeButton.onClick.AddListener(delegate { OnDisagreeButtonClick(); });
	}

	private void OnDisable() {
		agreeButton.onClick.RemoveListener(delegate { OnAgreeButtonClick(); });
		disagreeButton.onClick.RemoveListener(delegate { OnDisagreeButtonClick(); });
	}

	public void ShowPopup(Action<bool> callback) {
		caller = callback;
		uiContainer.SetActive(true);
	}

	public void HidePopup() {
		uiContainer.SetActive(false);
	}

	private void OnAgreeButtonClick() {
		HidePopup();
		caller?.Invoke(true);
	}

	private void OnDisagreeButtonClick() {
		HidePopup();
		caller?.Invoke(false);
	}
}
