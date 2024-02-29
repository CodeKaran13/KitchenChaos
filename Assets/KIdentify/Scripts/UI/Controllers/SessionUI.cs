using KIdentify.Services;
using Kidentify.Scripts.Tools;
using UnityEngine;
using UnityEngine.UI;
using KIdentify.Example;

namespace KIdentify.UI {
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
			KiDManager.Instance.UIManager.CheckForPreviousSession();
		}

		private void OnContinueButtonClick() {
			KiDManager.Instance.UIManager.OnSessionContinue();
		}
	}
}