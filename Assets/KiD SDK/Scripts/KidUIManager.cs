using System.Collections.Generic;
using Kidentify.Example;
using Kidentify.Scripts.Services;
using Kidentify.Scripts.Tools;
using ReadyPlayerMe;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kidentify.UI {
	public class KidUIManager : MonoBehaviour {

		public enum ActiveScreen {
			None,
			SDKSettings,
			Session,
			MagicAgeGate,
			SignUp,
			MinimumAge,
			QRCode,
			Email,
			MoreVerificationMethods,
			EmailSentSuccess,
			ApprovalProcess,
			ApprovalSuccess
		}

		[Header("UI"), Space(5)]
		[SerializeField] private SDKSettingsUI sdkSettingsUI;
		[SerializeField] private SessionUI sessionUI;
		[SerializeField] private SignUpUI signUpUI;
		[SerializeField] private MinimumAgeUI minimumAgeUI;
		[SerializeField] private QRCodeUI qrCodeUI;
		[SerializeField] private MoreVerificationUI moreVerificationUI;
		[SerializeField] private SendEmailUI sendEmailUI;
		[SerializeField] private EmailSentUI emailSentUI;
		[SerializeField] private ApprovalSuccessUI approvalSuccessUI;
		[SerializeField] private ApprovalProcessUI approvalProcessUI;
		[SerializeField] private GameObject magicAgeGateUI;
		[SerializeField] private GameObject debugOverlayUI;

		private string qrCodeURL;
		private string otp;
		private bool approvalSuccess;

		private readonly Stack<ActiveScreen> activeScreensStack = new();

		public string QRCodeURL {
			get {
				return qrCodeURL;
			}
		}

		public string OTP {
			get {
				return otp;
			}
		}

		public bool ApprovalSuccess {
			get {
				return approvalSuccess;
			}
		}

		private void OnEnable() {
			KiDManager.OnSignUpRequired += KiDManager_OnSignUpRequired;

			CameraPhotoSelection.OnImageCaptured += CameraPhotoSelection_OnImageCaptured;
		}

		private void OnDisable() {
			KiDManager.OnSignUpRequired -= KiDManager_OnSignUpRequired;

			CameraPhotoSelection.OnImageCaptured -= CameraPhotoSelection_OnImageCaptured;
		}

		#region TEMP (TESTING)

		public void EnableMagicAgeGate(bool enable) {
			KiDManager.Instance.UseMagicAgeGate = enable;
		}

		public void EnableDebugOverlay(bool enable) {
			KiDManager.Instance.ShowDebugOverlay = enable;
		}

		public void OnSessionContinue() {
			CloseAnyActiveScreen();
			var playerPrefsManager = ServiceLocator.Current.Get<PlayerPrefsManager>();
			if (!string.IsNullOrEmpty(playerPrefsManager.GetChallenge())) {
				KiDManager.Instance.GetChallenge();
			}
			else if(!string.IsNullOrEmpty(playerPrefsManager.GetSession())) {
				KiDManager.Instance.GetSession();
			}
		}

		#endregion

		#region UI

		public void ShowSDKSettingsUI() {
			CloseAnyActiveScreen();
			sdkSettingsUI.ShowUI();
			SetCurrentScreen(ActiveScreen.SDKSettings);
		}

		public void ShowSessionUI() {
			CloseAnyActiveScreen();
			sessionUI.ShowUI();
			SetCurrentScreen(ActiveScreen.Session);
		}

		public void ShowMagicAgeUI() {
			CloseAnyActiveScreen();
			magicAgeGateUI.SetActive(true);
			SetCurrentScreen(ActiveScreen.MagicAgeGate);
		}

		public void HideMagicAgeUI() {
			magicAgeGateUI.SetActive(false);
		}

		/// <summary>
		/// Show VPC screen
		/// </summary>
		/// <param name="qrCodeUrl"> URL to redirect </param>
		/// <param name="otp"> One time password for parent K-ID app. </param>
		public void ShowVPC(string qrCodeUrl, string otp) {
			qrCodeURL = qrCodeUrl;
			this.otp = otp;

			ShowQR();
		}

		/// <summary>
		/// Show Sign up Screen
		/// </summary>
		public void ShowSignUp() {
			CloseAnyActiveScreen();
			signUpUI.ShowUI();
			SetCurrentScreen(ActiveScreen.SignUp);
		}

		/// <summary>
		/// Show Minimum Age screen
		/// </summary>
		public void ShowMinimumAgeUI() {
			CloseAnyActiveScreen();
			minimumAgeUI.ShowUI();
			SetCurrentScreen(ActiveScreen.MinimumAge);
		}

		/// <summary>
		/// Show QR Code
		/// </summary>
		public void ShowQR() {
			CloseAnyActiveScreen();
			qrCodeUI.ShowUI();
			if (KiDManager.Instance.ShowDebugOverlay) {
				debugOverlayUI.SetActive(true);
			}
			SetCurrentScreen(ActiveScreen.QRCode);

			// Await for challenge completion
			KiDManager.Instance.AwaitChallenge();
		}

		/// <summary>
		/// Show more verification screen
		/// </summary>
		public void ShowMoreVerificationUI() {
			// No need to close active screen because it's part of back button implementation stack
			moreVerificationUI.ShowUI();
			SetCurrentScreen(ActiveScreen.MoreVerificationMethods);
		}

		/// <summary>
		/// Show Email screen
		/// </summary>
		public void ShowEmail() {
			// No need to close active screen because it's part of back button implementation stack
			sendEmailUI.ShowUI();
			SetCurrentScreen(ActiveScreen.Email);
		}

		/// <summary>
		/// Show Approval Success screen
		/// </summary>
		/// <param name="success"> If parent consent was a success or not </param>
		public void ShowApprovalSuccessUI(bool success = false) {
			CloseAndClearActiveScreenStack();
			
			approvalSuccess = success;
			approvalSuccessUI.ShowUI();
			if (KiDManager.Instance.ShowDebugOverlay) {
				debugOverlayUI.SetActive(false);
			}
			SetCurrentScreen(ActiveScreen.ApprovalSuccess);
		}

		/// <summary>
		/// Show Approval Process screen
		/// </summary>
		public void ShowApprovalProcessUI() {
			CloseAndClearActiveScreenStack();
			approvalProcessUI.ShowUI();
			SetCurrentScreen(ActiveScreen.ApprovalProcess);

			// Await Challenge again here
			if (!KiDManager.Instance.IsPollingOn) {
				KiDManager.Instance.AwaitChallenge();
			}
		}

		#endregion

		#region BUTTON ONCLICK

		public void OnSkipButtonClick() {
			Debug.Log($"OnSkipButtonClick");
			CloseAnyActiveScreen();
			//TODO:- Show warning popup.
			if (!SceneManager.GetActiveScene().name.Equals(KiDManager.Instance.SceneToLoad)) {
				SceneManager.LoadScene(KiDManager.Instance.SceneToLoad);
			}
		}

		public void OnPlayButtonClick() {
			CloseAnyActiveScreen();
			if (!SceneManager.GetActiveScene().name.Equals(KiDManager.Instance.SceneToLoad)) {
				SceneManager.LoadScene(KiDManager.Instance.SceneToLoad);
			}
		}

		public void OnSDKSettingsNextButtonClick() {
			CloseAnyActiveScreen();
			KiDManager.Instance.CheckForPreviousSession();
		}

		public void OnCopyButtonClick() {
			if (string.IsNullOrEmpty(KiDManager.Instance.CurrentPlayer.ChallengeId)) {
				Debug.Log($"No challenge id to copy");
			}
			else {
				Debug.Log("Challenge id copied to clipboard");
				GUIUtility.systemCopyBuffer = KiDManager.Instance.CurrentPlayer.ChallengeId;
			}
		}

		#endregion

		public void ShowPreviousUI() {
			CloseAnyActiveScreen();
			ShowPreviousScreen();
		}

		private void CloseAnyActiveScreen() {
			if (activeScreensStack.TryPop(out ActiveScreen currentScreen)) {
				switch (currentScreen) {
					case ActiveScreen.SDKSettings:
						sdkSettingsUI.HideUI();
						break;

					case ActiveScreen.Session:
						sessionUI.HideUI();
						break;

					case ActiveScreen.MagicAgeGate:
						HideMagicAgeUI();
						break;

					case ActiveScreen.SignUp:
						signUpUI.HideUI();
						break;

					case ActiveScreen.MinimumAge:
						minimumAgeUI.HideUI();
						break;

					case ActiveScreen.QRCode:
						qrCodeUI.HideUI();
						break;

					case ActiveScreen.Email:
						sendEmailUI.HideUI();
						break;

					case ActiveScreen.MoreVerificationMethods:
						moreVerificationUI.HideUI();
						break;

					case ActiveScreen.EmailSentSuccess:
						emailSentUI.HideUI();
						break;

					case ActiveScreen.ApprovalSuccess:
						approvalSuccessUI.HideUI();
						break;

					case ActiveScreen.ApprovalProcess:
						approvalProcessUI.HideUI();
						break;

					case ActiveScreen.None:
						break;
				}
			}
		}

		private void CloseAndClearActiveScreenStack() {
			var totalScreensActive = activeScreensStack.Count;
			for (int screenIndex = 0; screenIndex < totalScreensActive; screenIndex++) {
				CloseAnyActiveScreen();
			}
		}

		private void ShowPreviousScreen() {
			if (activeScreensStack.TryPop(out ActiveScreen previousScreen)) {
				switch (previousScreen) {
					case ActiveScreen.MinimumAge:
						ShowMinimumAgeUI();
						break;

					case ActiveScreen.QRCode:
						ShowQR();
						break;

					case ActiveScreen.Email:
						ShowEmail();
						break;

					case ActiveScreen.MoreVerificationMethods:
						ShowMoreVerificationUI();
						break;

					case ActiveScreen.None:
						break;
				}
			}

		}

		private void KiDManager_OnSignUpRequired(bool signUpRequired) {
			if (signUpRequired) {
				ShowSignUp();
			}
		}

		private void CameraPhotoSelection_OnImageCaptured(object sender, CameraPhotoSelection.OnImageCapturedEventArgs eventArgs) {
			KiDManager.Instance.OnTextureUpdate(eventArgs.texture);
		}

		private void SetCurrentScreen(ActiveScreen activeScreen) {
			activeScreensStack.Push(activeScreen);
			//Debug.Log("--------------------");
			//foreach (var screen in activeScreensStack) {
			//	Debug.Log($"Stack: {screen}");
			//}
			//Debug.Log("--------------------");
		}
	}
}

