using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SDKSettingsUI : BaseUI {

	[SerializeField] private TMP_Dropdown ageGateDropdown;
	[SerializeField] private Toggle showLogsToggle;
	[SerializeField] private Toggle debugOverlayToggle;
	[SerializeField] private GameObject loggerGO;


	protected override void Start() {
		base.Start();

		ageGateDropdown.onValueChanged.AddListener(delegate {
			OnAgeGateValueChanged(ageGateDropdown);
		});

		OnLogsToggle();
		OnDebugOverlayValueChanged();
	}

	public override void ShowUI() {
		base.ShowUI();
	}

	public override void HideUI() {
		base.HideUI();
	}

	#region ON VALUE CHANGED

	public void OnAgeGateValueChanged(TMP_Dropdown dropdown) {
		uiManager.SetAgeGateOption(dropdown.value);
	}

	public void OnLogsToggle() {
		if (showLogsToggle.isOn) {
			ShowLogs();
		}
		else {
			HideLogs();
		}
	}

	public void OnDebugOverlayValueChanged() {
		uiManager.EnableDebugOverlay(debugOverlayToggle.isOn);
	}

	#endregion

	private void ShowLogs() {
		loggerGO.SetActive(true);
	}

	private void HideLogs() {
		loggerGO.SetActive(false);
	}
}
