using System.Collections;
using System.Collections.Generic;
using KiD_SDK.Scripts.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KidUIManager : MonoBehaviour {
	public enum ActiveScreen {
		None,
		SignUp,
		MinimumAge,
		QRCode,
		Email,
		MoreVerificationMethods,
		EmailSentSuccess,
		ApprovalProcess,
		ApprovalSuccess
	}

	[SerializeField] private CountryCodesManager _countryCodesManager;
	[Header("API Properties"), Space(5)]
	[SerializeField] private int awaitChallengeRetriesMax = 3;
	[SerializeField] private int awaitResponseTimeout = 60;

	[Header("Canvas"), Space(5)]
	[SerializeField] private GameObject portraitCanvas;
	[SerializeField] private GameObject landscapeCanvas;

	[Header("UI"), Space(5)]
	[SerializeField] private SignUpUI signUpUI;
	[SerializeField] private MinimumAgeUI minimumAgeUI;
	[SerializeField] private QRCodeUI qrCodeUI;
	[SerializeField] private MoreVerificationUI moreVerificationUI;
	[SerializeField] private SendEmailUI sendEmailUI;
	[SerializeField] private EmailSentUI emailSentUI;
	[SerializeField] private ApprovalSuccessUI approvalSuccessUI;
	[SerializeField] private ApprovalProcessUI approvalProcessUI;

	private string qrCodeURL;
	private string otp;
	private bool approvalSuccess;

	private Stack<ActiveScreen> activeScreensStack = new Stack<ActiveScreen>();

	public int AwaitChallengeRetriesMax {
		get {
			return awaitChallengeRetriesMax;
		}
	}

	public int AwaitResponseTimeout {
		get {
			return awaitResponseTimeout;
		}
	}

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
	}

	private void OnDisable() {
		KiDManager.OnSignUpRequired -= KiDManager_OnSignUpRequired;
		KiDManager.OnKIDFlowCompleted -= KiDManager_OnKIDFlowCompleted;
	}

	public void SetCurrentScreen(ActiveScreen activeScreen) {
		activeScreensStack.Push(activeScreen);
		Debug.Log("--------------------");
		foreach (var screen in activeScreensStack) {
			Debug.Log($"Stack: {screen}");
		}
		Debug.Log("--------------------");
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
	}

	#region BUTTON ONCLICK

	public void OnSkipButtonClick() {
		Debug.Log($"OnSkipButtonClick");
		CloseAnyActiveScreen();
		//TODO:- Show warning popup.
		if (!SceneManager.GetActiveScene().name.Equals("Main")) {
			SceneManager.LoadScene("Main");
		}
	}

	public void OnPlayButtonClick() {
		CloseAnyActiveScreen();
		if (!SceneManager.GetActiveScene().name.Equals("Main")) {
			SceneManager.LoadScene("Main");
		}
	}

	#endregion

	private void CloseAnyActiveScreen() {
		if (activeScreensStack.TryPop(out ActiveScreen currentScreen)) {
			Debug.Log($"Closing {currentScreen}");
			switch (currentScreen) {
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

				//case ActiveScreen.EmailSentSuccess:
				//	ShowSentEmailSuccessUI();
				//	break;

				//case ActiveScreen.ApprovalSuccess:
				//	ShowApprovalSuccessUI();
				//	break;

				//case ActiveScreen.ApprovalProcess:
				//	ShowApprovalProcessUI();
				//	break;

				case ActiveScreen.None:
					break;
			}
		}

	}

	private void KiDManager_OnKIDFlowCompleted() {
		//SceneManager.LoadScene("Main");
	}

	private void KiDManager_OnSignUpRequired(bool signUpRequired) {
		if (signUpRequired) {
			ShowSignUp();
		}
	}
}
