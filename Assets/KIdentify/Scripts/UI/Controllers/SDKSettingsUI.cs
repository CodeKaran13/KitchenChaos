using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KIdentify.UI {
	public class SDKSettingsUI : BaseUI {

		[SerializeField] private TMP_Dropdown ageGateDropdown;
		[SerializeField] private TMP_Dropdown successScreenDropdown;
		[SerializeField] private TMP_Dropdown successScreenDisplayDropdown;
		[SerializeField] private Toggle showLogsToggle;
		[SerializeField] private Toggle debugOverlayToggle;
		[SerializeField] private GameObject loggerGO;


		protected override void Start() {
			base.Start();

			ageGateDropdown.onValueChanged.AddListener(delegate {
				OnAgeGateValueChanged(ageGateDropdown);
			});

			successScreenDropdown.onValueChanged.AddListener(delegate {
				OnSuccessScreenValueChanged(successScreenDropdown);
			});

			successScreenDisplayDropdown.onValueChanged.AddListener(delegate {
				OnSuccessScreenDisplayValueChanged(successScreenDisplayDropdown);
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

		private void OnAgeGateValueChanged(TMP_Dropdown dropdown) {
			uiManager.SetAgeGateOption(dropdown.value);
		}

		private void OnSuccessScreenValueChanged(TMP_Dropdown dropdown) {
			uiManager.SetSuccessScreenOption(dropdown.value);
			switch (dropdown.value) {
				case 0:
					successScreenDisplayDropdown.gameObject.SetActive(false);
					break;

				case 1:
					successScreenDisplayDropdown.gameObject.SetActive(true);
					break;

				default:
					successScreenDisplayDropdown.gameObject.SetActive(false);
					break;
			}
		}

		private void OnSuccessScreenDisplayValueChanged(TMP_Dropdown dropdown) {
			uiManager.SetSuccessScreenDisplayOption(dropdown.value);
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
}