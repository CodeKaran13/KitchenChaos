using Kidentify.Scripts.Services;
using Kidentify.Scripts.Tools;
using UnityEngine;
using UnityEngine.UI;

public class SessionUI : BaseUI {
	[SerializeField] private Button clearSessionButton;
	[SerializeField] private Button continueButton;

	private void Awake() {
		clearSessionButton.onClick.AddListener(OnClearSessionButtonClick);
		continueButton.onClick.AddListener(OnContinueButtonClick);
	}

	public override void ShowUI() {
		base.ShowUI();
	}

	public override void HideUI() {
		base.HideUI();
	}

	private void OnClearSessionButtonClick() {
		var playerPrefsManager = ServiceLocator.Current.Get<PlayerPrefsManager>();
		playerPrefsManager.ClearSession();
		uiManager.CheckForPreviousSession();
	}

	private void OnContinueButtonClick() {
		uiManager.OnSessionContinue();
	}
}
