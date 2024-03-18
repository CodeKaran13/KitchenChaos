using KIdentify.Services;
using KIdentify.Sample.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.Sample.UI
{
	public class SessionUI : BaseUI
	{
		[SerializeField] private Button clearSessionButton;
		[SerializeField] private Button continueButton;

		private void Awake()
		{
			clearSessionButton.onClick.AddListener(OnClearSessionButtonClick);
			continueButton.onClick.AddListener(OnContinueButtonClick);
		}

		public override void ShowUI()
		{
			base.ShowUI();
		}

		public override void HideUI()
		{
			base.HideUI();
		}

		private void OnClearSessionButtonClick()
		{
			var playerPrefsManager = ServiceLocator.Current.Get<PlayerPrefsManager>();
			playerPrefsManager.ClearSession();
			KiDManager.Instance.UIManager.CheckForPreviousSession();
		}

		private void OnContinueButtonClick()
		{
			KiDManager.Instance.UIManager.OnSessionContinue();
		}
	}
}