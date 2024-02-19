using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour {

	[SerializeField] private GameObject uiContainer;
	[SerializeField] private Button okayButton;

	private Action<bool> caller;

	private void OnEnable() {
		okayButton.onClick.AddListener(delegate { OnNextClick(); });
	}

	private void OnDisable() {
		okayButton.onClick.RemoveListener(delegate { OnNextClick(); });
	}

	public void ShowPopup(Action<bool> callback) {
		caller = callback;
		uiContainer.SetActive(true);
	}

	public void HidePopup() {
		uiContainer.SetActive(false);
	}

	private void OnNextClick() {
		HidePopup();
		caller?.Invoke(true);
	}
}
