using System;
using System.Collections.Generic;
using System.Linq;
using Assets.KiD_SDK.Scripts.Interfaces;
using Assets.KiD_SDK.Scripts.Services;
using KiD_SDK.Scripts.Services;
using KiD_SDK.Scripts.Tools;
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

		public static string Location { get; internal set; }
		public static DateTime DateOfBirth { get; internal set; }
		public static List<Permission> Permissions { get; internal set; }

		public delegate void SignUp(bool signUpRequired);
		public static event SignUp OnSignUpRequired;
		private readonly string apiKey = "60d777719025e94e256ec9582fa607f0362c2c0e287213867f0c71d5f50187d3";

		[SerializeField] private CountryCodesManager _countryCodesManager;
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
		private PlayerPrefsManager playerPrefsManager;
		private int retryAttemptCount = 0;
		private int minAgeEstimated = 0;
		private bool ageEstimationCalculated = false;

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
		}

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

			PlayerStorage playerStorage = new();
			ServiceLocator.Current.Register<IPlayerStorage>(playerStorage);
			playerStorage.CurrentPlayer = new KiDPlayer();
			currentPlayer = playerStorage.CurrentPlayer;

			playerPrefsManager = new PlayerPrefsManager(currentPlayer);

			kidSdk = new(apiKey);
		}

		private void OnEnable() {
			AvatarCreatorSelection.OnAvatarCreationCompleted += AvatarCreatorSelection_OnAvatarCreationCompleted;
		}

		private void OnDisable() {
			AvatarCreatorSelection.OnAvatarCreationCompleted -= AvatarCreatorSelection_OnAvatarCreationCompleted;
		}

		private void Start() {
			if (!string.IsNullOrEmpty(playerPrefsManager.GetSession())) {
				//Debug.Log($"SessionId: {playerPrefsManager.GetSession()}");
				currentPlayer.SessionId = playerPrefsManager.GetSession();

				uiManager.ShowSessionUI();
			}
			else {
				if (useMagicAgeGate) {
					uiManager.ShowMagicAgeUI();
				}
				else {
					uiManager.HideMagicAgeUI();
					OnSignUpRequired?.Invoke(true);
				}
			}
		}

		public void OnTextureUpdate(Texture texture) {
			//Debug.Log("On Texture Update");
			kidSdk.AgeEstimator.OnTextureUpdate(texture);
			if (kidSdk.AgeEstimator.IsResultReady() && !ageEstimationCalculated) {
				ageEstimationCalculated = true;
				var result = kidSdk.AgeEstimator.GetResult();
				Debug.Log("Min age: " + (int)result.minAge + ", max age: " + (int)result.maxAge);
				minAgeEstimated = (int)result.minAge;
				SetLocationByIP();
			}
		}

		public async void CreateSession() {
			if (TryConvertCountryStringToCode(Location, out string countryCode)) {
				CreateSessionSchema createSessionSchema = new() {
					jurisdiction = countryCode,
					dateOfBirth = DateOfBirth.ToString("yyyy-MM-dd")
				};

				CreateSessionResponse result = await kidSdk.CreateSession(createSessionSchema);
				if (result.success) {
					Debug.Log($"CreateSessionResponse: {result}");
					Debug.Log($"Session Id: {result.sessionId}");
					currentPlayer.SessionId = result.sessionId;
					playerPrefsManager.SaveSession();

					if (result.status == "ACTIVE") {
						currentPlayer.Permissions = result.permissions;
						currentPlayer.Status = PlayerStatus.Verified;
						currentPlayer.IsAdult = true;

						PermissionsReceived(result.permissions);

						OnKIDFlowCompleted?.Invoke();
					}
					else if (result.status == "CHALLENGE") {
						currentPlayer.ChallengeId = result.challenge.challengeId;
						currentPlayer.ChallengeType = Enum.Parse<ChallengeType>(result.challenge.type);
						if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_PARENTAL_CONSENT) {
							Debug.Log("CHALLENGE_PARENTAL_CONSENT");
						}
						else if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_DIGITAL_CONSENT_AGE) {
							Debug.Log("CHALLENGE_DIGITAL_CONSENT_AGE");
						}

						if (useSdkUi) {
							uiManager.ShowVPC(result.challenge.url, result.challenge.oneTimePassword);
						}

						// Await for challenge completion
						AwaitChallenge();
					}
					else if (result.status == "PROHIBITED") {
						if (useSdkUi) {
							uiManager.ShowMinimumAgeUI();
						}
					}
				}
			}
			else {
				Debug.Log("Invalid Location.");
			}
		}

		public async void GetSession() {
			SessionData sessionData = await kidSdk.GetSession(currentPlayer.SessionId);
			if (sessionData.success) {
				if (sessionData.status == "ACTIVE") {
					//foreach (var permission in sessionData.permissions) {
					//	Debug.Log($"Name: {permission.name}");
					//}
					currentPlayer.Permissions = sessionData.permissions;
					currentPlayer.Status = PlayerStatus.Verified;
					currentPlayer.IsAdult = true;

					PermissionsReceived(sessionData.permissions);

					OnKIDFlowCompleted?.Invoke();
				}
				else if (sessionData.status == "CHALLENGE") {
					currentPlayer.ChallengeId = sessionData.challenge.challengeId;
					currentPlayer.ChallengeType = Enum.Parse<ChallengeType>(sessionData.challenge.type);

					if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_PARENTAL_CONSENT) {
						if (useMagicAgeGate) {
							uiManager.ShowMagicAgeUI();
						}
						else {
							if (useSdkUi) {
								uiManager.ShowVPC(sessionData.challenge.url, sessionData.challenge.oneTimePassword);
							}
						}
					}
					else if (currentPlayer.ChallengeType == ChallengeType.CHALLENGE_DIGITAL_CONSENT_AGE) {
						Debug.Log("CHALLENGE_DIGITAL_CONSENT_AGE");
					}



					// Await for challenge completion
					AwaitChallenge();
				}
				else if (sessionData.status == "PROHIBITED") {
					if (useSdkUi) {
						uiManager.ShowMinimumAgeUI();
					}
				}
			}
		}

		public async void SendEmail(string email) {
			ChallengeEmailSchema challengeEmailSchema = new() {
				challengeId = currentPlayer.ChallengeId,
				email = email
			};

			var (success, response, code) = await kidSdk.SendEmailChallenge(challengeEmailSchema);
			Debug.Log($"responseCode: {code}");
			if (success) {
				if (code == 200) {
					if (useSdkUi) {
						uiManager.ShowApprovalProcessUI();
					}
				}
			}
			else {
			}
		}

		public async void AwaitChallenge() {
			ChallengeAwaitResponse challengeAwaitResponse = await kidSdk.AwaitChallenge(currentPlayer.ChallengeId, awaitResponseTimeout);
			if (challengeAwaitResponse.success) {
				if (challengeAwaitResponse.status == "PASS") {
					Debug.Log("Challenge PASS");
					IsPollingOn = false;
					retryAttemptCount = 0;
					currentPlayer.Status = PlayerStatus.Verified;
					currentPlayer.IsAdult = true;

					// Challenge has PASSED so get the permissions for user
					GetSession();
				}
				else if (challengeAwaitResponse.status == "FAIL") {
					Debug.Log("Challenge FAIL");
					IsPollingOn = false;
					retryAttemptCount = 0;
					currentPlayer.Status = PlayerStatus.Failed;

					if (useSdkUi) {
						uiManager.ShowApprovalSuccessUI(false);
					}
				}
				else if (challengeAwaitResponse.status == "POLL_TIMEOUT") {
					Debug.Log("Challenge TIMEOUT");
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
		}

		private void PermissionsReceived(List<Permission> permissions) {
			Permissions = permissions;
			currentPlayer.Permissions = permissions;
			OnPermissionsChanged?.Invoke(permissions);
		}

		private void AvatarCreatorSelection_OnAvatarCreationCompleted(object sender, EventArgs e) {
			if (!ageEstimationCalculated) {
				SetLocationByIP();
				Invoke(nameof(CreateSession_PostMagicAgeGate), 3f);
			}
			else {
				CreateSession_PostMagicAgeGate();
			}
		}

		private void CreateSession_PostMagicAgeGate() {
			// TODO:- Calculate DOB based on minAgeEstimated
			if (minAgeEstimated >= 18) {
				DateOfBirth = DateTime.Parse("1993-10-13");
			}
			else {
				DateOfBirth = DateTime.Parse("2015-10-13");
			}

			CreateSession();
		}

		private async void SetLocationByIP() {
			var ipManager = new CountryAndIPManager();
			string country = await ipManager.FindCountryByExternalIP();
			string countryName = GetCountry(country);
			Location = countryName;
			Debug.Log($"Country: {countryName}");
		}

		private string GetCountry(string countryCode) {
			int defaultResult = 0;

			var countryCodesManager = ServiceLocator.Current.Get<CountryCodesManager>();
			var countries = countryCodesManager.Countries.ToList();

			string countryName;

			int index = countries.FindIndex(cnt => cnt.Value == countryCode);

			if (index == -1) {
				countryName = countries[defaultResult].Key;
			}
			countryName = countries[index].Key;
			return countryName;
		}

		private bool TryConvertCountryStringToCode(string country, out string code) {
			code = ServiceLocator.Current.Get<CountryCodesManager>().GetCountryCode(country);
			return !string.IsNullOrEmpty(code);
		}
	}
}