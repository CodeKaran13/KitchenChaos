using System.Collections.Generic;
using Kidentify.Example;
using ReadyPlayerMe;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kidentify.UI {
	public class KidUIManager : MonoBehaviour {
		public enum ActiveScreen {
			None,
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
			KiDManager.OnKIDFlowCompleted += KiDManager_OnKIDFlowCompleted;

			CameraPhotoSelection.OnImageCaptured += CameraPhotoSelection_OnImageCaptured;
		}

		private void OnDisable() {
			KiDManager.OnSignUpRequired -= KiDManager_OnSignUpRequired;
			KiDManager.OnKIDFlowCompleted -= KiDManager_OnKIDFlowCompleted;

			CameraPhotoSelection.OnImageCaptured -= CameraPhotoSelection_OnImageCaptured;
		}

		//Temp for testing
		public void OnSessionContinue() {
			CloseAnyActiveScreen();
			KiDManager.Instance.GetSession();
		}

		#region UI

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

		public void ShowVPC(string qrCodeUrl, string otp) {
			qrCodeURL = qrCodeUrl;
			this.otp = otp;

			ShowQR();
		}

		public void ShowPreviousUI() {
			CloseAnyActiveScreen();
			ShowPreviousScreen();
		}

		public void ShowSignUp() {
			signUpUI.ShowUI();
			SetCurrentScreen(ActiveScreen.SignUp);
		}

		public void ShowMinimumAgeUI() {
			minimumAgeUI.ShowUI();
			SetCurrentScreen(ActiveScreen.MinimumAge);
		}

		public void ShowQR() {
			CloseAnyActiveScreen();
			// Show QR Code
			qrCodeUI.ShowUI();
			SetCurrentScreen(ActiveScreen.QRCode);
		}

		public void ShowMoreVerificationUI() {
			// No need to close active screen because it's part of back button implementation stack
			// Show more verification screen
			moreVerificationUI.ShowUI();
			SetCurrentScreen(ActiveScreen.MoreVerificationMethods);
		}

		public void ShowEmail() {
			// No need to close active screen because it's part of back button implementation stack
			// Show Email UI
			sendEmailUI.ShowUI();
			SetCurrentScreen(ActiveScreen.Email);
		}

		//public void ShowSentEmailSuccessUI() {
		//	CloseAnyActiveScreen();
		//	// Show email sent success UI
		//	sendEmailUI.ShowUI();
		//	SetCurrentScreen(ActiveScreen.EmailSentSuccess);
		//}

		public void ShowApprovalSuccessUI(bool success = false) {
			CloseAndClearActiveScreenStack();
			// Show Approval Success UI
			approvalSuccess = success;
			approvalSuccessUI.ShowUI();
			SetCurrentScreen(ActiveScreen.ApprovalSuccess);
		}

		public void ShowApprovalProcessUI() {
			CloseAndClearActiveScreenStack();
			// Show Approval Process UI
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
			if (!SceneManager.GetActiveScene().name.Equals("MainMenuScene")) {
				SceneManager.LoadScene("MainMenuScene");
			}
		}

		public void OnPlayButtonClick() {
			CloseAnyActiveScreen();
			if (!SceneManager.GetActiveScene().name.Equals("MainMenuScene")) {
				SceneManager.LoadScene("MainMenuScene");
			}
		}

		#endregion

		private void CloseAnyActiveScreen() {
			if (activeScreensStack.TryPop(out ActiveScreen currentScreen)) {
				Debug.Log($"Closing {currentScreen}");
				switch (currentScreen) {
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

		private void KiDManager_OnKIDFlowCompleted() {
			//SceneManager.LoadScene("MainMenuScene");
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
			Debug.Log("--------------------");
			foreach (var screen in activeScreensStack) {
				Debug.Log($"Stack: {screen}");
			}
			Debug.Log("--------------------");
		}
	}
}

