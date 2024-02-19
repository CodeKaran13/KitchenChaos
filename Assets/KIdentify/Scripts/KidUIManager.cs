using System;
using System.Collections.Generic;
using KIdentify.Example;
using KIdentify.PlayerInfo;
using KIdentify.Services;
using Kidentify.Scripts.Tools;
using ReadyPlayerMe;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KIdentify.UI {
	public class KidUIManager : MonoBehaviour {

		public enum AgeGateOptions {
			StandardAgeGate,
			MagicAgeGate_LikenessAvatar,
			MagicAgeGate_NoAvatar,
			MagicAgeGate_NonPersonalAvatar
		}

		public enum SuccessScreenOptions {
			Hide,
			Show
		}

		public enum SuccessScreenDisplayOptions {
			Simple,
			Game
		}

		public enum ActiveScreen {
			None,
			SDKSettings,
			Session,
			MagicAgeGate,
			AgeGate_NoAvatar,
			AgeGate_MiniGame,
			SignUp,
			MinimumAge,
			QRCode,
			Email,
			MoreVerificationMethods,
			EmailSentSuccess,
			ApprovalProcess,
			ApprovalSuccess,
			GameApprovalSuccess,
			HoldGameAccess
		}

		[Header("UI-Controllers"), Space(5)]
		[SerializeField] private SDKSettingsUI sdkSettingsUI;
		[SerializeField] private SessionUI sessionUI;
		[SerializeField] private SignUpUI signUpUI;
		[SerializeField] private MinimumAgeUI minimumAgeUI;
		[SerializeField] private QRCodeUI qrCodeUI;
		[SerializeField] private MoreVerificationUI moreVerificationUI;
		[SerializeField] private SendEmailUI sendEmailUI;
		[SerializeField] private EmailSentUI emailSentUI;
		[SerializeField] private ApprovalProcessUI approvalProcessUI;
		[SerializeField] private HoldGameAccessUI holdGameAccessUI;
		[Header("Success Screen")]
		[SerializeField] private ApprovalSuccessUI approvalSuccessUI;
		[SerializeField] private GameApprovalSuccessUI gameSuccessUI;
		[Header("Magic Age Gate UI")]
		[SerializeField] private AgeGateNoAvatarUI ageGateNoAvatarUI;
		[SerializeField] private AgeGateMiniGameUI ageGateMiniGameUI;
		[SerializeField] private GameObject magicAgeGateUI;
		[Header("-----------------")]
		[SerializeField] private GameObject debugOverlayUI;
		[SerializeField] private Popup popup;

		private string qrCodeURL;
		private string otp;
		private bool approvalSuccess;
		private KiDPlayer currentPlayer;
		private PlayerPrefsManager playerPrefsManager;

		private readonly Stack<ActiveScreen> activeScreensStack = new();

		private AgeGateOptions selectedAgeGateOption = AgeGateOptions.StandardAgeGate;
		private SuccessScreenOptions selectedSuccessScreen = SuccessScreenOptions.Hide;
		private SuccessScreenDisplayOptions selectedSuccessDisplay = SuccessScreenDisplayOptions.Simple;

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
			CameraPhotoSelection.OnImageCaptured += CameraPhotoSelection_OnImageCaptured;
		}

		private void OnDisable() {
			CameraPhotoSelection.OnImageCaptured -= CameraPhotoSelection_OnImageCaptured;
		}

		private void Start() {
			currentPlayer = KiDManager.Instance.CurrentPlayer;
			playerPrefsManager = ServiceLocator.Current.Get<PlayerPrefsManager>();
		}

		public void CheckForPreviousSession() {
			if (!string.IsNullOrEmpty(playerPrefsManager.GetChallenge())) {
				currentPlayer.ChallengeId = playerPrefsManager.GetChallenge();
				Debug.Log($"ChallengeId: {currentPlayer.ChallengeId}");
				ShowSessionUI();
			}
			else if (!string.IsNullOrEmpty(playerPrefsManager.GetSession())) {
				currentPlayer.SessionId = playerPrefsManager.GetSession();
				Debug.Log($"SessionId: {currentPlayer.SessionId}");
				ShowSessionUI();
			}
			else {
				ShowAgeGate();
			}
		}

		#region SDK Settings (TESTING)

		public void SetAgeGateOption(int index) {
			selectedAgeGateOption = index switch {
				0 => AgeGateOptions.StandardAgeGate,
				1 => AgeGateOptions.MagicAgeGate_LikenessAvatar,
				2 => AgeGateOptions.MagicAgeGate_NoAvatar,
				3 => AgeGateOptions.MagicAgeGate_NonPersonalAvatar,
				_ => AgeGateOptions.StandardAgeGate
			};
		}

		public void SetSuccessScreenOption(int index) {
			selectedSuccessScreen = index switch {
				0 => SuccessScreenOptions.Hide,
				1 => SuccessScreenOptions.Show,
				_ => SuccessScreenOptions.Hide
			};
		}

		public void SetSuccessScreenDisplayOption(int index) {
			selectedSuccessDisplay = index switch {
				0 => SuccessScreenDisplayOptions.Simple,
				1 => SuccessScreenDisplayOptions.Game,
				_ => SuccessScreenDisplayOptions.Simple
			};
		}

		public void EnableDebugOverlay(bool enable) {
			KiDManager.Instance.ShowDebugOverlay = enable;
		}

		#endregion

		/// <summary>
		/// Continue with previous session of the user
		/// </summary>
		public void OnSessionContinue() {
			CloseAnyActiveScreen();
			var playerPrefsManager = ServiceLocator.Current.Get<PlayerPrefsManager>();

			if (!string.IsNullOrEmpty(playerPrefsManager.GetChallenge())) {
				KiDManager.Instance.GetChallenge();
			}
			else if (!string.IsNullOrEmpty(playerPrefsManager.GetSession())) {
				KiDManager.Instance.GetSession();
			}
			else {
				// This case will never happen because session UI only opens if there was any active user session (ex. Challenge/Session)
			}
		}

		/// <summary>
		/// Show different Age gate options depending on the selected option from SDK settings
		/// </summary>
		private void ShowAgeGate() {
			switch (selectedAgeGateOption) {
				case AgeGateOptions.StandardAgeGate:
					magicAgeGateUI.SetActive(false);
					ShowSignUp();
					break;

				case AgeGateOptions.MagicAgeGate_LikenessAvatar:
					ShowMagicAgeUI();
					break;

				case AgeGateOptions.MagicAgeGate_NoAvatar:
					ShowAgeGateNoAvatarUI();
					break;

				case AgeGateOptions.MagicAgeGate_NonPersonalAvatar:
					ShowAgeGateMiniGame();
					break;
			}
		}

		/// <summary>
		/// Send Webcam texture to Privately for Age estimation
		/// </summary>
		/// <param name="texture"> Captured image </param>
		public void SendImageTexture(Texture texture) {
			KiDManager.Instance.OnTextureUpdate(texture);
		}

		#region UI

		/// <summary>
		/// Show SDK settings screen
		/// </summary>
		public void ShowSDKSettingsUI() {
			CloseAnyActiveScreen();
			sdkSettingsUI.ShowUI();
			SetCurrentScreen(ActiveScreen.SDKSettings);
		}

		/// <summary>
		/// Show previos session screen
		/// </summary>
		public void ShowSessionUI() {
			CloseAnyActiveScreen();
			sessionUI.ShowUI();
			SetCurrentScreen(ActiveScreen.Session);
		}

		#region Magic Age Gates

		/// <summary>
		/// Show ReadyPlayerMe Magic Age gate
		/// </summary>
		public void ShowMagicAgeUI() {
			CloseAnyActiveScreen();
			popup.ShowPopup((access) => {
				if (access) {
					magicAgeGateUI.SetActive(true);
					SetCurrentScreen(ActiveScreen.MagicAgeGate);
				}
			});
		}

		/// <summary>
		/// Show No avatar magic age gate
		/// </summary>
		private void ShowAgeGateNoAvatarUI() {
			CloseAnyActiveScreen();
			popup.ShowPopup((access) => {
				if (access) {
					ageGateNoAvatarUI.ShowUI();
					SetCurrentScreen(ActiveScreen.AgeGate_NoAvatar);
				}
			});
		}

		/// <summary>
		/// Show mini-game magic age gate
		/// </summary>
		private void ShowAgeGateMiniGame() {
			CloseAnyActiveScreen();
			popup.ShowPopup((access) => {
				if (access) {
					ageGateMiniGameUI.ShowUI();
					SetCurrentScreen(ActiveScreen.AgeGate_MiniGame);
				}
			});
		}

		#endregion

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
		public void ShowApprovalSuccessUI() {
			approvalSuccess = true;
			if (selectedSuccessScreen == SuccessScreenOptions.Show) {
				if (selectedSuccessDisplay == SuccessScreenDisplayOptions.Simple) {
					CloseAndClearActiveScreenStack();
					approvalSuccessUI.ShowUI();
					SetCurrentScreen(ActiveScreen.ApprovalSuccess);
				}
				else {
					CloseAndClearActiveScreenStack();
					gameSuccessUI.ShowUI();
					SetCurrentScreen(ActiveScreen.GameApprovalSuccess);
				}
			}
			else {
				CloseAndClearActiveScreenStack();
				OnPlayButtonClick();
			}

			if (KiDManager.Instance.ShowDebugOverlay) {
				debugOverlayUI.SetActive(false);
			}
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

		/// <summary>
		/// Show Hold game access screen
		/// </summary>
		public void ShowHoldGameAccessUI() {
			CloseAndClearActiveScreenStack();
			holdGameAccessUI.ShowUI();
			SetCurrentScreen(ActiveScreen.HoldGameAccess);
		}

		#endregion

		#region BUTTON ONCLICK

		public void OnPlayButtonClick() {
			CloseAnyActiveScreen();
			if (!SceneManager.GetActiveScene().name.Equals(KiDManager.Instance.SceneToLoad)) {
				SceneManager.LoadScene(KiDManager.Instance.SceneToLoad);
			}
		}

		public void OnSDKSettingsNextButtonClick() {
			CloseAnyActiveScreen();
			CheckForPreviousSession();
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

		/// <summary>
		/// Part of back button implementation (Stack)
		/// </summary>
		public void ShowPreviousUI() {
			CloseAnyActiveScreen();
			ShowPreviousScreen();
		}

		/// <summary>
		/// Closes current active screen
		/// </summary>
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
						magicAgeGateUI.SetActive(false);
						break;

					case ActiveScreen.AgeGate_NoAvatar:
						ageGateNoAvatarUI.HideUI();
						break;

					case ActiveScreen.AgeGate_MiniGame:
						ageGateMiniGameUI.HideUI();
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

					case ActiveScreen.GameApprovalSuccess:
						gameSuccessUI.HideUI();
						break;

					case ActiveScreen.ApprovalProcess:
						approvalProcessUI.HideUI();
						break;

					case ActiveScreen.HoldGameAccess:
						holdGameAccessUI.HideUI();
						break;

					case ActiveScreen.None:
						break;
				}
			}
		}

		/// <summary>
		/// Closes all active screens
		/// </summary>
		private void CloseAndClearActiveScreenStack() {
			var totalScreensActive = activeScreensStack.Count;
			for (int screenIndex = 0; screenIndex < totalScreensActive; screenIndex++) {
				CloseAnyActiveScreen();
			}
		}

		/// <summary>
		/// Shows previous active screen
		/// </summary>
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

		/// <summary>
		/// Event called when Webcam is active, for age estimation
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void CameraPhotoSelection_OnImageCaptured(object sender, CameraPhotoSelection.OnImageCapturedEventArgs eventArgs) {
			SendImageTexture(eventArgs.texture);
		}

		/// <summary>
		/// Push current active screen in stack
		/// </summary>
		/// <param name="activeScreen"> current active screen </param>
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

