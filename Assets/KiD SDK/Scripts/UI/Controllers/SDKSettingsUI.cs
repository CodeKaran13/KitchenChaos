using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SDKSettingsUI : BaseUI {
	[SerializeField] private Toggle magicAgeGateToggle;
	[SerializeField] private Toggle showLogsToggle;
	[SerializeField] private GameObject loggerGO;

	protected override void Start() {
		base.Start();
		OnMagicAgeGateValueChanged();
		OnLogsToggle();
	}

	public override void ShowUI() {
		base.ShowUI();
	}

	public override void HideUI() {
		base.HideUI();
	}

	public void OnMagicAgeGateValueChanged() {
		uiManager.EnableMagicAgeGate(magicAgeGateToggle.isOn);
	}

	public void OnLogsToggle() {
		if (showLogsToggle.isOn) {
			ShowLogs();
		}
		else {
			HideLogs();
		}
	}

	private void ShowLogs() {
		loggerGO.SetActive(true);
	}

	private void HideLogs() {
		loggerGO.SetActive(false);
	}
}
