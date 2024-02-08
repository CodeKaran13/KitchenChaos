using System;
using System.Collections.Generic;
using System.Linq;
using Kidentify.Scripts.Interfaces;
using Kidentify.Scripts.Services;
using Kidentify.Scripts.Tools;
using Kidentify.PlayerInfo;
using Kidentify.UI;
using ReadyPlayerMe;
using UnityEngine;

namespace Kidentify.Example {
	public class KiDManager : MonoBehaviour {
		public static KiDManager Instance { get; private set; }

		public delegate void PermissionsChanged(List<Permission> permissions);
		public static event PermissionsChanged OnPermissionsChanged;

		public delegate void KIDFlowComplete();
		public static event KIDFlowComplete OnKIDFlowCompleted;

		// Location contains country name
		public static string Location { get; internal set; }
		public static DateTime DateOfBirth { get; internal set; }
		public static List<Permission> Permissions { get; internal set; }

		private readonly string apiKey = "d2bea511202d07ee38afcf38ae2f188a13c0cba0e52cf5cdfb042a27cd61cdf6";

		[SerializeField] private CountryCodesManager _countryCodesManager;
		[SerializeField] private PermissionsManager permissionsManager;
		[SerializeField] private PlayerPrefsManager playerPrefsManager;
		[Header("SDK Settings")]
		[SerializeField] private bool useSdkUi;
		[SerializeField] private bool useMagicAgeGate;
		[SerializeField] private string sceneToLoadAfterAgeVerification;
		[Space(5)]
		[SerializeField] private int awaitChallengeRetriesMax = 3;
		[SerializeField] private int awaitResponseTimeout = 60;
		[Header("UI"), Space(5)]
		public KidUIManager uiManager;

		private KiD kidSdk;
		private KiDPlayer currentPlayer;

		private int retryAttemptCount = 0;
		private int minAgeEstimated = 0;
		private bool ageEstimationCalculated = false;

		public bool IsPollingOn { get; private set; }

		// Ready Player Me
		public string SelectedRPMUrl { get; set; }

		public KiDPlayer CurrentPlayer {
			get {
				return currentPlayer;
			}
		}

		public bool UseMagicAgeGate {
			get {
				return useMagicAgeGate;
			}
			set {
				useMagicAgeGate = value;
			}
		}

		public bool ShowDebugOverlay { get; set; }

		public string SceneToLoad { get { return sceneToLoadAfterAgeVerification; } }

		private void Awake() {
			if (Instance == null) {
				Instance = this;
			}
			else {
				if (Instance != this) {
					Destroy(this.gameObject);
				}
			}
			DontDestroyOnLoad(this.gameObject);

			// Initialize default service locator.
			ServiceLocator.Initiailze();

			ServiceLocator.Current.Register(_countryCodesManager);
			ServiceLocator.Current.Register(permissionsManager);

			PlayerStorage playerStorage = new();
			ServiceLocator.Current.Register<IPlayerStorage>(playerStorage);
			playerStorage.CurrentPlayer = new KiDPlayer();
			currentPlayer = playerStorage.CurrentPlayer;

			playerPrefsManager = new PlayerPrefsManager(currentPlayer);
			ServiceLocator.Current.Register(playerPrefsManager);

			kidSdk = new(apiKey);
		}

		private void OnEnable() {
			AvatarCreatorSelection.OnAvatarCreationCompleted += AvatarCreatorSelection_OnAvatarCreationCompleted;
		}

		private void OnDisable() {
			AvatarCreatorSelection.OnAvatarCreationCompleted -= AvatarCreatorSelection_OnAvatarCreationCompleted;
		}

		private void Start() {
			// To get camera permissions early in iOS
			WebCamDevice[] devices = WebCamTexture.devices;
			WebCamDevice webCamDevice = devices.FirstOrDefault(device => device.isFrontFacing);
			if (webCamDevice.Equals(default(WebCamDevice))) {
			}

			// First screen to show if SDK UI is turned ON
			if (useSdkUi) {
				SetLocationByIP();
				uiManager.ShowSDKSettingsUI();
			}
		}

		public void OnTextureUpdate(Texture texture) {
			kidSdk.AgeEstimator.OnTextureUpdate(texture);
			if (kidSdk.AgeEstimator.IsResultReady() && !ageEstimationCalculated) {
				ageEstimationCalculated = true;
				var result = kidSdk.AgeEstimator.GetResult();
				Debug.Log("Min age: " + (int)result.minAge + ", max age: " + (int)result.maxAge);
				minAgeEstimated = (int)result.minAge;
				SetLocationByIP();
			}
		}

		public void ValidateAge() {
			if (!ageEstimationCalculated) {
				SetLocationByIP();
				Invoke(nameof(AgeGateCheck_PostMagicAgeGate), 3f);
			}
			else {
				AgeGateCheck_PostMagicAgeGate();
			}
		}

		public async void AgeGateCheck() {
			if (TryConvertCountryStringToCode(Location, out string countryCode)) {
				AgeGateCheckRequest ageGateCheckRequest = new();
				if (string.IsNullOrEmpty(currentPlayer.RegionCode)) {
					ageGateCheckRequest.jurisdiction = countryCode;
				}
				else {
					ageGateCheckRequest.jurisdiction = string.Join("-", countryCode, currentPlayer.RegionCode);
				}
				ageGateCheckRequest.dateOfBirth = DateOfBirth.ToString("yyyy-MM-dd");

				AgeGateCheckResponse ageGateCheckResponse = await kidSdk.AgeGateCheck(ageGateCheckRequest);
				if (ageGateCheckResponse.success) {
					Debug.Log($"AgeGateCheck Response: {ageGateCheckResponse}");

					if (ageGateCheckResponse.status == "PASS") {

						currentPlayer.SessionId = ageGateCheckResponse.session.sessionId;
						currentPlayer.Etag = ageGateCheckResponse.session.etag;
						currentPlayer.Permissions = ageGateCheckResponse.session.permissions;
						currentPlayer.Status = PlayerStatus.Verified;
						currentPlayer.IsAdult = true;

						playerPrefsManager.SaveSession();
						playerPrefsManager.SaveEtag();

						PermissionsReceived(ageGateCheckResponse.session.permissions, true);

						OnKIDFlowCompleted?.Invoke();
					}
					else if (ageGateCheckResponse.status == "CHALLENGE") {

						currentPlayer.ChallengeId = ageGateCheckResponse.challenge.challengeId;
						currentPlayer.ChildLiteAccessEnabled = ageGateCheckResponse.challenge.childLiteAccessEnabled;
						//currentPlayer.Etag = ageGateCheckResponse.challenge.etag;
						currentPlayer.Status = PlayerStatus.Pending;
						currentPlayer.IsAdult = false;

						playerPrefsManager.SaveChallenge();
						//playerPrefsManager.SaveEtag();

						currentPlayer.ChallengeType = Enum.Parse<ChallengeType>(ageGateCheckResponse.challenge.type);
						Debug.Log($"Challenge Type: {currentPlayer.ChallengeType}");

						//if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_PARENTAL_CONSENT) {
						//	Debug.Log("CHALLENGE_PARENTAL_CONSENT");
						//}
						//else if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_DIGITAL_CONSENT_AGE) {
						//	Debug.Log("CHALLENGE_DIGITAL_CONSENT_AGE");
						//}
						//else if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_AGE_APPEAL) {
						//	Debug.Log("CHALLENGE_AGE_APPEAL");
						//}

						if (useSdkUi) {
							uiManager.ShowVPC(ageGateCheckResponse.challenge.url, ageGateCheckResponse.challenge.oneTimePassword);
						}
					}
					else if (ageGateCheckResponse.status == "PROHIBITED") {
						if (useSdkUi) {
							uiManager.ShowMinimumAgeUI();
						}
					}
				}
				else {
					Debug.Log($"Error: {ageGateCheckResponse.errorMessage}");
				}
			}
			else {
				Debug.Log("Invalid Location.");
			}
		}

		public async void GetChallenge() {
			GetChallengeResponse getChallengeResponse = await kidSdk.GetChallenge(currentPlayer.ChallengeId);
			if (getChallengeResponse.success) {
				currentPlayer.ChallengeId = getChallengeResponse.challenge.challengeId;
				currentPlayer.ChildLiteAccessEnabled = getChallengeResponse.challenge.childLiteAccessEnabled;
				//currentPlayer.Etag = getChallengeResponse.challenge.etag;
				currentPlayer.Status = PlayerStatus.Pending;
				currentPlayer.IsAdult = false;

				playerPrefsManager.SaveChallenge();
				//playerPrefsManager.SaveEtag();

				currentPlayer.ChallengeType = Enum.Parse<ChallengeType>(getChallengeResponse.challenge.type);
				Debug.Log($"Challenge Type: {currentPlayer.ChallengeType}");

				//if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_PARENTAL_CONSENT) {
				//	Debug.Log("CHALLENGE_PARENTAL_CONSENT");
				//}
				//else if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_DIGITAL_CONSENT_AGE) {
				//	Debug.Log("CHALLENGE_DIGITAL_CONSENT_AGE");
				//}
				//else if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_AGE_APPEAL) {
				//	Debug.Log("CHALLENGE_AGE_APPEAL");
				//}

				if (useSdkUi) {
					uiManager.ShowVPC(getChallengeResponse.challenge.url, getChallengeResponse.challenge.oneTimePassword);
				}
			}
			else {
				Debug.Log($"Error: {getChallengeResponse.errorMessage}");
			}
		}

		public async void GetSession() {
			GetSessionResponse getSessionResponse = await kidSdk.GetSession(currentPlayer.SessionId);
			if (getSessionResponse.success) {
				if (getSessionResponse.status == "ACTIVE") {
					currentPlayer.SessionId = getSessionResponse.sessionId;
					currentPlayer.Etag = getSessionResponse.etag;
					currentPlayer.Permissions = getSessionResponse.permissions;
					currentPlayer.Status = PlayerStatus.Verified;
					currentPlayer.IsAdult = true;

					if (!playerPrefsManager.GetEtag().Equals(getSessionResponse.etag)) {
						playerPrefsManager.SaveEtag();
						PermissionsReceived(getSessionResponse.permissions, true);
					}
					else {
						uiManager.OnPlayButtonClick();
					}

					playerPrefsManager.SaveSession();

					OnKIDFlowCompleted?.Invoke();
				}
				else if (getSessionResponse.status == "HOLD") {

				}
			}
			else {
				Debug.Log($"Error: {getSessionResponse.errorMessage}");
			}
		}

		public async void SendEmail(string email) {
			ChallengeEmailRequest challengeEmailRequest = new() {
				challengeId = currentPlayer.ChallengeId,
				email = email
			};
			var (success, _, code) = await kidSdk.SendEmailChallenge(challengeEmailRequest);
			Debug.Log($"responseCode: {code}");
			if (success) {
				if (code == 200) {
					if (useSdkUi) {
						if (currentPlayer.ChildLiteAccessEnabled) {
							uiManager.ShowApprovalProcessUI();
						}
						else {
							uiManager.ShowApprovalSuccessUI(false);
						}
					}
				}
			}
			else {
				Debug.Log($"ErrorCode: {code}");
			}
		}

		public async void AwaitChallenge() {
			AwaitChallengeResponse awaitChallengeResponse = await kidSdk.AwaitChallenge(currentPlayer.ChallengeId, awaitResponseTimeout);
			if (awaitChallengeResponse.success) {
				Debug.Log($"Challenge {awaitChallengeResponse.status}");
				if (awaitChallengeResponse.status == "PASS") {
					IsPollingOn = false;
					retryAttemptCount = 0;
					currentPlayer.SessionId = awaitChallengeResponse.sessionId;

					playerPrefsManager.ClearChallenge();

					// Challenge has PASSED so get the permissions for user
					GetSession();
				}
				else if (awaitChallengeResponse.status == "FAIL") {
					IsPollingOn = false;
					retryAttemptCount = 0;
					currentPlayer.Status = PlayerStatus.Failed;

					//TODO:- Show Challenge FAIL screen
				}
				else if (awaitChallengeResponse.status == "POLL_TIMEOUT") {
					currentPlayer.Status = PlayerStatus.Pending;
					retryAttemptCount++;
					if (retryAttemptCount < awaitChallengeRetriesMax) {
						IsPollingOn = true;
						AwaitChallenge();
					}
					else {
						IsPollingOn = false;
						retryAttemptCount = 0;
					}
				}
			}
			else {
				Debug.Log($"Error: {awaitChallengeResponse.errorMessage}");
			}
		}

		private void PermissionsReceived(List<Permission> permissions, bool changed) {
			Permissions = permissions;
			if (changed) {
				OnPermissionsChanged?.Invoke(permissions);
			}
		}

		private void AvatarCreatorSelection_OnAvatarCreationCompleted(object sender, EventArgs e) {
			ValidateAge();
		}

		private void AgeGateCheck_PostMagicAgeGate() {
			// TODO:- Calculate DOB based on minAgeEstimated
			if (minAgeEstimated >= 18) {
				DateOfBirth = DateTime.Parse("1993-10-13");
			}
			else {
				DateOfBirth = DateTime.Parse("2015-10-13");
			}

			AgeGateCheck();
		}

		private async void SetLocationByIP() {
			var ipManager = new CountryAndIPManager();
			string country = await ipManager.FindCountryByExternalIP();
			Debug.Log($"Country-Region: {country}");
			var splitCountryCode = country.Split("-");
			CurrentPlayer.CountryCode = splitCountryCode[0];
			try {
				CurrentPlayer.RegionCode = splitCountryCode[1];
			}
			catch (IndexOutOfRangeException) {
				Debug.Log($"Received no region code for country {splitCountryCode[0]} from IPAPI.");
			}
			Location = GetCountry(CurrentPlayer.CountryCode);
			Debug.Log($"Location: {Location}");
		}

		private string GetCountry(string countryCode) {
			int defaultResult = 0;

			var countryCodesManager = ServiceLocator.Current.Get<CountryCodesManager>();
			var countries = countryCodesManager.Countries.ToList();

			string countryName;

			int index = countries.FindIndex(cnt => cnt.Key == countryCode);

			if (index == -1) {
				countryName = countries[defaultResult].Value;
			}
			countryName = countries[index].Value;
			return countryName;
		}

		private bool TryConvertCountryStringToCode(string country, out string code) {
			code = ServiceLocator.Current.Get<CountryCodesManager>().GetCountryCode(country);
			return !string.IsNullOrEmpty(code);
		}
	}
}