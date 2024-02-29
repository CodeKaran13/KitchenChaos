using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KIdentify.Example;

namespace KIdentify.UI {
	public class SDKSettingsUI : BaseUI {

		[Header("Settings")]
		[SerializeField] private TMP_Dropdown ageGateDropdown;
		[SerializeField] private TMP_Dropdown successScreenDropdown;
		[SerializeField] private TMP_Dropdown successScreenDisplayDropdown;
		[SerializeField] private Toggle cameraAccessToggle;
		[SerializeField] private Toggle showLogsToggle;
		[SerializeField] private Toggle debugOverlayToggle;
		[SerializeField] private GameObject loggerGO;

		private void Start()
		{
			ageGateDropdown.onValueChanged.AddListener(delegate {
				OnAgeGateValueChanged(ageGateDropdown);
			});

			successScreenDropdown.onValueChanged.AddListener(delegate {
				OnSuccessScreenValueChanged(successScreenDropdown);
			});

			successScreenDisplayDropdown.onValueChanged.AddListener(delegate {
				OnSuccessScreenDisplayValueChanged(successScreenDisplayDropdown);
			});
		}

		private void SetSettings()
		{
			OnAgeGateValueChanged(ageGateDropdown);
			OnSuccessScreenValueChanged(successScreenDropdown);
			OnSuccessScreenDisplayValueChanged(successScreenDisplayDropdown);

			OnCameraAccessToggle();
			OnLogsToggle();
			OnDebugOverlayValueChanged();
		}

		public override void ShowUI() {
			base.ShowUI();
			SetSettings();
		}

		public override void HideUI() {
			base.HideUI();
		}

		#region ON VALUE CHANGED

		private void OnAgeGateValueChanged(TMP_Dropdown dropdown) {
			KiDManager.Instance.UIManager.SetAgeGateOption(dropdown.value);
		}

		private void OnSuccessScreenValueChanged(TMP_Dropdown dropdown) {
			KiDManager.Instance.UIManager.SetSuccessScreenOption(dropdown.value);
			switch (dropdown.value) {
				case 0:
					successScreenDisplayDropdown.gameObject.SetActive(true);
					break;

				case 1:
					successScreenDisplayDropdown.gameObject.SetActive(false);
					break;

				case 2:
					successScreenDisplayDropdown.gameObject.SetActive(false);
					break;

				default:
					successScreenDisplayDropdown.gameObject.SetActive(false);
					break;
			}
		}

		private void OnSuccessScreenDisplayValueChanged(TMP_Dropdown dropdown) {
			KiDManager.Instance.UIManager.SetSuccessScreenDisplayOption(dropdown.value);
		}

		public void OnCameraAccessToggle() {
			KiDManager.Instance.UIManager.EnableCameraAccess(cameraAccessToggle.isOn);
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
			KiDManager.Instance.UIManager.EnableDebugOverlay(debugOverlayToggle.isOn);
		}

		#endregion

		private void ShowLogs() {
			loggerGO.SetActive(true);
		}

		private void HideLogs() {
			loggerGO.SetActive(false);
		}
	}
}