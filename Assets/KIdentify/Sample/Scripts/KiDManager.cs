using System;
using System.Collections.Generic;
using System.Linq;
using KIdentify.Models;
using KIdentify.Sample.Interfaces;
using KIdentify.Services;
using KIdentify.Sample.PlayerInfo;
using ReadyPlayerMe;
using UnityEngine;
using KIdentify.Sample.Logger;
using UnityEngine.SceneManagement;
using KIdentify.Sample.Tools;

namespace KIdentify.Sample
{
	public class KiDManager : MonoBehaviour
	{
		public static KiDManager Instance { get; private set; }

		public delegate void KIDFlowComplete();
		public static event KIDFlowComplete OnKIDFlowCompleted;

		private readonly string apiKey = "d2bea511202d07ee38afcf38ae2f188a13c0cba0e52cf5cdfb042a27cd61cdf6";

		[SerializeField] private CountryCodesManager countryCodesManager;
		[SerializeField] private PermissionsManager permissionsManager;
		private PlayerPrefsManager playerPrefsManager;
		private LocationManager locationManager;

		[Header("SDK Settings")]
		[SerializeField] private bool useSdkUi;
		[SerializeField] private bool useMagicAgeGate;
		[SerializeField] private string sceneToLoadAfterAgeVerification;
		[Space(5)]
		[SerializeField] private int awaitChallengeRetriesMax = 3;
		[SerializeField] private int awaitResponseTimeout = 60;
		[Header("UI"), Space(5)]
		[SerializeField] private KidUIManager uiManager;

		private KiD kidSdk;
		private KiDPlayer currentPlayer;
		private int retryAttemptCount = 0;
		private int minAgeEstimated = 0;
		private bool ageEstimationCalculated = false;

		// Privately
		private static readonly string modelPath = "model_reg_mug.tflite.enc";
		private AgeEstimator ageEstimator;
		private Network network;

		// Location contains country name
		public static string Location { get; internal set; }
		public static DateTime DateOfBirth { get; internal set; }
		public static List<Permission> Permissions { get; private set; }
		public KidUIManager UIManager { get { return uiManager; } }
		public KiDPlayer CurrentPlayer { get { return currentPlayer; } }
		public bool IsPollingOn { get; private set; }
		public bool UseMagicAgeGate { get { return useMagicAgeGate; } set { useMagicAgeGate = value; } }
		public bool ShowCameraAccess { get; set; }
		public bool ShowDebugOverlay { get; set; }
		public string SceneToLoad { get { return sceneToLoadAfterAgeVerification; } }

		// Ready Player Me
		public string SelectedRPMUrl { get; set; }

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				if (Instance != this)
				{
					Destroy(this.gameObject);
				}
			}
			DontDestroyOnLoad(this.gameObject);

			// Initialize k-ID SDK
			InitializeKIdentify();
		}

		private void OnEnable()
		{
			AvatarCreatorSelection.OnAvatarCreationCompleted += AvatarCreatorSelection_OnAvatarCreationCompleted;
			SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
		}

		private void OnDisable()
		{
			AvatarCreatorSelection.OnAvatarCreationCompleted -= AvatarCreatorSelection_OnAvatarCreationCompleted;
			SceneManager.sceneLoaded -= SceneManager_OnSceneLoaded;
		}

		private async void SceneManager_OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			// First scene is loaded
			if (scene.buildIndex == 0)
			{
				// First screen to show if SDK UI is turned ON
				if (useSdkUi)
				{
					var (countryCode, regionCode) = await locationManager.GetLocationByIP();
					currentPlayer.CountryCode = countryCode;
					currentPlayer.RegionCode = regionCode;
					Location = locationManager.GetCountry(countryCode);

					uiManager.ShowSDKSettingsUI();
				}
			}
		}

		private void Start()
		{
			// To get camera permissions early in iOS
			WebCamDevice[] devices = WebCamTexture.devices;
			WebCamDevice webCamDevice = devices.FirstOrDefault(device => device.isFrontFacing);
			if (webCamDevice.Equals(default(WebCamDevice)))
			{
			}

			// Initialize Privately SDK
			InitializePrivately();
		}

		/// <summary>
		/// Initialize SDK - KIdentify
		/// </summary>
		private async void InitializeKIdentify()
		{
			// Initialize default service locator.
			ServiceLocator.Initiailze();

			ServiceLocator.Current.Register(countryCodesManager);
			ServiceLocator.Current.Register(permissionsManager);

			locationManager = new();
			ServiceLocator.Current.Register(locationManager);

			PlayerStorage playerStorage = new();
			ServiceLocator.Current.Register<IPlayerStorage>(playerStorage);
			playerStorage.CurrentPlayer = new KiDPlayer();
			currentPlayer = playerStorage.CurrentPlayer;

			playerPrefsManager = new PlayerPrefsManager(currentPlayer);
			ServiceLocator.Current.Register(playerPrefsManager);

			kidSdk = new(apiKey, playerPrefsManager, new KIdentifyUnityLogger());

			if (playerPrefsManager.GetAuthToken().Equals(""))
				await kidSdk.AuthClient("test-client-id");
		}

		#region Privately SDK

		/// <summary>
		/// Initialize SDK - Privately
		/// </summary>
		private void InitializePrivately()
		{
			Debug.Log("Starting Privately.");
			network = new Network();
			Debug.Log("Network loaded.");
			LoadEstimator();
		}

		/// <summary>
		/// Load estimator - Privately
		/// </summary>
		private async void LoadEstimator()
		{
			if (useSdkUi)
			{
				uiManager.ShowLoadingUI();
			}

			ageEstimator = new AgeEstimator();
			var key = await network.Authenticate("44b91c5f-487d-4d6a-bac2-f68b199603aa", "QNcsPkmVzv4veX9n-drhd6IHS1THNl");

			var binary = ModelDecryptor.Decryptor.DecryptModel(modelPath, key);
			ageEstimator.LoadModel(binary);
			Debug.Log($"Age estimator loaded: {ageEstimator.IsLoaded()}");

			if (useSdkUi)
			{
				uiManager.HideLoadingUI();
			}
		}

		/// <summary>
		/// Sends Web Camera textures for age estimation
		/// </summary>
		/// <param name="texture"> camera captured texture </param>
		public async void OnTextureUpdate(Texture texture)
		{
			ageEstimator.OnTextureUpdate(texture);
			if (ageEstimator.IsResultReady() && !ageEstimationCalculated)
			{
				ageEstimationCalculated = true;

				var result = ageEstimator.GetResult(18f);
				Debug.Log("Age gate " + result.ageGate + " : " + result.ageGatePassed + " estimation: " + result.minAge + " - " + result.maxAge);
				Debug.Log("Min age: " + (int)result.minAge + ", max age: " + (int)result.maxAge);
				minAgeEstimated = (int)result.minAge;

				var (countryCode, regionCode) = await locationManager.GetLocationByIP();
				currentPlayer.CountryCode = countryCode;
				currentPlayer.RegionCode = regionCode;
				Location = locationManager.GetCountry(countryCode);
			}
		}

		#endregion

		/// <summary>
		/// Validate age estimated by Privately
		/// </summary>
		public async void ValidateAge()
		{
			if (!ageEstimationCalculated)
			{
				var (countryCode, regionCode) = await locationManager.GetLocationByIP();
				currentPlayer.CountryCode = countryCode;
				currentPlayer.RegionCode = regionCode;
				Location = locationManager.GetCountry(countryCode);

				Invoke(nameof(AgeGateCheck_PostMagicAgeGate), 3f);
			}
			else
			{
				AgeGateCheck_PostMagicAgeGate();
			}
		}

		#region K-ID APIs

		/// <summary>
		/// Age Gate Check API
		/// </summary>
		public async void AgeGateCheck()
		{
			if (locationManager.TryConvertCountryStringToCode(Location, out string countryCode))
			{
				if (useSdkUi)
				{
					uiManager.ShowLoadingUI();
				}

				AgeGateCheckRequest ageGateCheckRequest = new();
				if (string.IsNullOrEmpty(currentPlayer.RegionCode))
				{
					ageGateCheckRequest.jurisdiction = currentPlayer.CountryCode;
				}
				else
				{
					ageGateCheckRequest.jurisdiction = string.Join("-", countryCode, currentPlayer.RegionCode);
				}
				ageGateCheckRequest.dateOfBirth = DateOfBirth.ToString("yyyy-MM-dd");

				AgeGateCheckResponse ageGateCheckResponse = await kidSdk.AgeGateCheck(ageGateCheckRequest);

				if (useSdkUi)
				{
					uiManager.HideLoadingUI();
				}

				if (ageGateCheckResponse.success)
				{
					if (ageGateCheckResponse.status == "PASS")
					{
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
					else if (ageGateCheckResponse.status == "CHALLENGE")
					{
						currentPlayer.ChallengeId = ageGateCheckResponse.challenge.challengeId;
						currentPlayer.ChildLiteAccessEnabled = ageGateCheckResponse.challenge.childLiteAccessEnabled;
						currentPlayer.Status = PlayerStatus.Pending;
						currentPlayer.IsAdult = false;

						playerPrefsManager.SaveChallenge();

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

						if (useSdkUi)
						{
							uiManager.ShowVPC(ageGateCheckResponse.challenge.url, ageGateCheckResponse.challenge.oneTimePassword);
						}
					}
					else if (ageGateCheckResponse.status == "PROHIBITED")
					{
						if (useSdkUi)
						{
							uiManager.ShowMinimumAgeUI();
						}
					}
				}
				else
				{
					Debug.Log($"Error: {ageGateCheckResponse.errorMessage}");
				}
			}
			else
			{
				Debug.Log("Invalid Location.");
			}
		}

		/// <summary>
		/// Get Challenge API
		/// </summary>
		public async void GetChallenge()
		{
			if (useSdkUi)
			{
				uiManager.ShowLoadingUI();
			}
			GetChallengeResponse getChallengeResponse = await kidSdk.GetChallenge(currentPlayer.ChallengeId);

			if (useSdkUi)
			{
				uiManager.HideLoadingUI();
			}
			if (getChallengeResponse.success)
			{
				currentPlayer.ChallengeId = getChallengeResponse.challenge.challengeId;
				currentPlayer.ChildLiteAccessEnabled = getChallengeResponse.challenge.childLiteAccessEnabled;
				currentPlayer.Status = PlayerStatus.Pending;
				currentPlayer.IsAdult = false;

				playerPrefsManager.SaveChallenge();

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

				if (useSdkUi)
				{
					uiManager.ShowVPC(getChallengeResponse.challenge.url, getChallengeResponse.challenge.oneTimePassword);
				}
			}
			else
			{
				Debug.Log($"Error: {getChallengeResponse.errorMessage}");
			}
		}

		/// <summary>
		/// Get Session API
		/// </summary>
		public async void GetSession()
		{
			if (useSdkUi)
			{
				uiManager.ShowLoadingUI();
			}
			GetSessionResponse getSessionResponse = await kidSdk.GetSession(currentPlayer.SessionId);

			if (useSdkUi)
			{
				uiManager.HideLoadingUI();
			}
			if (getSessionResponse.success)
			{
				if (getSessionResponse.status == "ACTIVE")
				{
					currentPlayer.SessionId = getSessionResponse.sessionId;
					currentPlayer.Etag = getSessionResponse.etag;
					currentPlayer.Permissions = getSessionResponse.permissions;
					currentPlayer.Status = PlayerStatus.Verified;
					currentPlayer.IsAdult = true;

					if (!playerPrefsManager.GetEtag().Equals(getSessionResponse.etag))
					{
						playerPrefsManager.SaveEtag();
						PermissionsReceived(getSessionResponse.permissions, true);
					}
					else
					{
						uiManager.LoadGameScene();
					}

					playerPrefsManager.SaveSession();

					OnKIDFlowCompleted?.Invoke();
				}
				else if (getSessionResponse.status == "HOLD")
				{

				}
			}
			else
			{
				Debug.Log($"Error: {getSessionResponse.errorMessage}");
			}
		}

		/// <summary>
		/// Send Email API
		/// </summary>
		/// <param name="email"> email address of the parent. </param>
		public async void SendEmail(string email)
		{
			if (useSdkUi)
			{
				uiManager.ShowLoadingUI();
			}
			ChallengeEmailRequest challengeEmailRequest = new()
			{
				challengeId = currentPlayer.ChallengeId,
				email = email
			};
			var (success, _, code) = await kidSdk.SendEmailChallenge(challengeEmailRequest);
			Debug.Log($"responseCode: {code}");
			if (useSdkUi)
			{
				uiManager.HideLoadingUI();
			}
			if (success)
			{
				if (useSdkUi)
				{
					if (currentPlayer.ChildLiteAccessEnabled)
					{
						uiManager.ShowApprovalProcessUI();
					}
					else
					{
						uiManager.ShowHoldGameAccessUI();
					}
				}
			}
			else
			{
				Debug.Log($"ErrorCode: {code}");
			}
		}

		/// <summary>
		/// Await challenge status API
		/// </summary>
		public async void AwaitChallenge()
		{
			AwaitChallengeResponse awaitChallengeResponse = await kidSdk.AwaitChallenge(currentPlayer.ChallengeId, awaitResponseTimeout);
			if (awaitChallengeResponse.success)
			{
				Debug.Log($"Challenge {awaitChallengeResponse.status}");
				if (awaitChallengeResponse.status == "PASS")
				{
					IsPollingOn = false;
					retryAttemptCount = 0;
					currentPlayer.SessionId = awaitChallengeResponse.sessionId;

					playerPrefsManager.ClearChallenge();

					// Challenge has PASSED so get the permissions for user
					GetSession();
				}
				else if (awaitChallengeResponse.status == "FAIL")
				{
					IsPollingOn = false;
					retryAttemptCount = 0;
					currentPlayer.Status = PlayerStatus.Failed;

					//TODO:- Show Challenge FAIL screen
				}
				else if (awaitChallengeResponse.status == "POLL_TIMEOUT")
				{
					currentPlayer.Status = PlayerStatus.Pending;
					retryAttemptCount++;
					if (retryAttemptCount < awaitChallengeRetriesMax)
					{
						IsPollingOn = true;
						AwaitChallenge();
					}
					else
					{
						IsPollingOn = false;
						retryAttemptCount = 0;
					}
				}
			}
			else
			{
				Debug.Log($"Error: {awaitChallengeResponse.errorMessage}");
			}
		}

		#endregion

		private void PermissionsReceived(List<Permission> permissions, bool changed)
		{
			Permissions = permissions;
			if (changed)
			{
				if (useSdkUi)
				{
					uiManager.ShowApprovalSuccessUI();
				}
			}
		}

		private void AvatarCreatorSelection_OnAvatarCreationCompleted(object sender, EventArgs e)
		{
			ValidateAge();
		}

		private void AgeGateCheck_PostMagicAgeGate()
		{
			var estimatedDate = DateTime.Now.AddYears(-minAgeEstimated);
			int lastDate = DateTime.DaysInMonth(estimatedDate.Year, estimatedDate.Month);
			DateOfBirth = new DateTime(estimatedDate.Year, estimatedDate.Month, lastDate);
			Debug.Log($"Estimated D.O.B: {DateOfBirth}");
			AgeGateCheck();
		}
	}
}