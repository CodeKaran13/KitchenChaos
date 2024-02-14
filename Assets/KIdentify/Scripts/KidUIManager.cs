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
			HoldGameAccess
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
		[SerializeField] private HoldGameAccessUI holdGameAccessUI;
		[SerializeField] private GameObject magicAgeGateUI;
		[SerializeField] private AgeGateNoAvatarUI ageGateNoAvatarUI;
		[SerializeField] private AgeGateMiniGameUI ageGateMiniGameUI;
		[SerializeField] private GameObject debugOverlayUI;

		private string qrCodeURL;
		private string otp;
		private bool approvalSuccess;
		private KiDPlayer currentPlayer;
		private PlayerPrefsManager playerPrefsManager;

		private readonly Stack<ActiveScreen> activeScreensStack = new();

		private AgeGateOptions selectedAgeGateOption = AgeGateOptions.StandardAgeGate;

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

		#region TEMP (TESTING)

		public void SetAgeGateOption(int index) {
			selectedAgeGateOption = index switch {
				0 => AgeGateOptions.StandardAgeGate,
				1 => AgeGateOptions.MagicAgeGate_LikenessAvatar,
				2 => AgeGateOptions.MagicAgeGate_NoAvatar,
				3 => AgeGateOptions.MagicAgeGate_NonPersonalAvatar,
				_ => AgeGateOptions.StandardAgeGate,
			};
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
			else if (!string.IsNullOrEmpty(playerPrefsManager.GetSession())) {
				KiDManager.Instance.GetSession();
			}
			else {
				// This case will never happen because session UI only opens if there was any active user session (ex. Challenge/Session)
			}
		}

		private void ShowAgeGate() {
			switch (selectedAgeGateOption) {
				case AgeGateOptions.StandardAgeGate:
					HideMagicAgeUI();
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

		public void SendImageTexture(Texture texture) {
			KiDManager.Instance.OnTextureUpdate(texture);
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

		private void ShowAgeGateNoAvatarUI() {
			CloseAnyActiveScreen();
			ageGateNoAvatarUI.ShowUI();
			SetCurrentScreen(ActiveScreen.AgeGate_NoAvatar);
		}

		private void ShowAgeGateMiniGame() {
			CloseAnyActiveScreen();
			ageGateMiniGameUI.ShowUI();
			SetCurrentScreen(ActiveScreen.AgeGate_MiniGame);
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
		public void ShowApprovalSuccessUI() {
			approvalSuccess = true;

			switch (selectedAgeGateOption) {
				case AgeGateOptions.StandardAgeGate:
					CloseAndClearActiveScreenStack();
					approvalSuccessUI.ShowUI();
					SetCurrentScreen(ActiveScreen.ApprovalSuccess);
					break;

				case AgeGateOptions.MagicAgeGate_LikenessAvatar:
					approvalSuccessUI.ShowUI();
					SetCurrentScreen(ActiveScreen.ApprovalSuccess);
					break;

				case AgeGateOptions.MagicAgeGate_NoAvatar:
					ageGateNoAvatarUI.ShowSuccessUI();
					SetCurrentScreen(ActiveScreen.AgeGate_NoAvatar);
					break;

				case AgeGateOptions.MagicAgeGate_NonPersonalAvatar:
					ageGateMiniGameUI.ShowSuccessUI();
					SetCurrentScreen(ActiveScreen.AgeGate_MiniGame);
					break;
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

		private void CameraPhotoSelection_OnImageCaptured(object sender, CameraPhotoSelection.OnImageCapturedEventArgs eventArgs) {
			SendImageTexture(eventArgs.texture);
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

