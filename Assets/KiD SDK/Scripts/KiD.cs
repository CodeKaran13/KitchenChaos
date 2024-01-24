using GLTFast.Schema;
using KiD_SDK.Scripts.Services;
using KiD_SDK.Scripts.Tools;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TensorFlowLite;
using UnityEngine;

namespace Kidentify {
	public class KiD {

		private static readonly string baseUrl = "https://api.k-id.com/api/v1";
		private readonly string apiKey = "";

		// Privately
		private static string modelPath = "model_reg_mug.tflite.enc";

		private AgeEstimator ageEstimator;
		private Network network;
		private PrimitiveDraw draw;

		public AgeEstimator AgeEstimator { get { return ageEstimator; } }

		public KiD(string apiKey) {
			this.apiKey = apiKey;

			// Privately
			Debug.Log("Starting");
			network = new Network();
			Debug.Log("Network loaded");
			draw = new PrimitiveDraw(UnityEngine.Camera.main);
			LoadEstimator();
		}

		// Privately
		private async void LoadEstimator() {
			ageEstimator = new AgeEstimator();
			var key = await network.Authenticate("44b91c5f-487d-4d6a-bac2-f68b199603aa", "QNcsPkmVzv4veX9n-drhd6IHS1THNl");

			var binary = ModelDecryptor.Decryptor.DecryptModel(modelPath, key);
			ageEstimator.LoadModel(binary);
			Debug.Log($"age estimator loaded: {ageEstimator.IsLoaded()}");
		}

		public async Task<CreateSessionResponse> CreateSession(CreateSessionSchema createSessionSchema) {

			string content = JsonUtility.ToJson(createSessionSchema);
			Debug.Log(content);
			var (success, response, code) = await PostRequest($"{Endpoints.CREATE_SESSION}", content);

			if (success) {
				try {
					CreateSessionResponse createSessionResponse = JsonUtility.FromJson<CreateSessionResponse>(response);
					if (createSessionResponse != null) {
						createSessionResponse.success = true;
						return createSessionResponse;
					}
					else {
						return new CreateSessionResponse { success = false, errorMessage = "CreateSessionResponse is null." };
					}
				}
				catch (ArgumentNullException ane) {
					Debug.Log($"Error: {ane.Message}");
					return new CreateSessionResponse { success = false, errorMessage = "The data to parse is null." };
				}
				catch (ArgumentException ae) {
					Debug.Log($"Error: {ae.Message}");
					return new CreateSessionResponse { success = false, errorMessage = "Invalid JSON format." };
				}
			}
			else {
				return new CreateSessionResponse { success = false, errorMessage = code.ToString() };
			}
		}

		public async Task<SessionData> GetSession(string sessionId) {
			var (success, response, code) = await Get($"{Endpoints.GET_SESSION}?sessionId={sessionId}");

			if (success) {
				try {
					SessionData sessionData = JsonUtility.FromJson<SessionData>(response);
					if (sessionData != null) {
						sessionData.success = true;
						return sessionData;
					}
					else {
						return new SessionData { success = false, errorMessage = "SessionData is null." };
					}
				}
				catch (ArgumentNullException ane) {
					Debug.Log($"Error: {ane.Message}");
					return new SessionData { success = false, errorMessage = "The data to parse is null." };
				}
				catch (ArgumentException ae) {
					Debug.Log($"Error: {ae.Message}");
					return new SessionData { success = false, errorMessage = "Invalid JSON format." };
				}
			}
			else {
				return new SessionData { success = false, errorMessage = code.ToString() };
			}
		}

		public async Task<(bool, string, int)> SendEmailChallenge(ChallengeEmailSchema challengeEmailSchema) {
			string content = JsonUtility.ToJson(challengeEmailSchema);
			Debug.Log(content);
			return await PostRequest($"{Endpoints.SEND_EMAIL_REQUEST}", content);
		}

		public async Task<ChallengeAwaitResponse> AwaitChallenge(string challengeId, int responseTimeout) {
			var (success, response, code) = await Get($"{Endpoints.AWAIT_CHALLENGE}?challengeId={challengeId}&timeout={responseTimeout}");

			if (success) {
				try {
					ChallengeAwaitResponse challengeAwaitResponse = JsonUtility.FromJson<ChallengeAwaitResponse>(response);
					if (challengeAwaitResponse != null) {
						challengeAwaitResponse.success = true;
						return challengeAwaitResponse;
					}
					else {
						return new ChallengeAwaitResponse { success = false, errorMessage = "ChallengeAwaitResponse is null." };
					}
				}
				catch (ArgumentNullException ane) {
					Debug.Log($"Error: {ane.Message}");
					return new ChallengeAwaitResponse { success = false, errorMessage = "The data to parse is null." };
				}
				catch (ArgumentException ae) {
					Debug.Log($"Error: {ae.Message}");
					return new ChallengeAwaitResponse { success = false, errorMessage = "Invalid JSON format." };
				}
			}
			else {
				return new ChallengeAwaitResponse { success = false, errorMessage = code.ToString() };
			}
		}

		/// <summary>
		/// For getting the resources from a web api
		/// </summary>
		/// <param name="endpoint">API endpoint</param>
		/// <returns>A Task with result object of type T</returns>
		private async Task<(bool, string, int)> Get(string endpoint, CancellationToken cancellationToken = default) {

			try {
				var httpClient = new HttpClient();
				string url = baseUrl + endpoint;
				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(url), cancellationToken);
				response.EnsureSuccessStatusCode();
				var res = await response.Content.ReadAsStringAsync();

				Debug.Log($"Response: {res}");

				if (response.IsSuccessStatusCode) {
					return (true, res, (int)response.StatusCode);
				}
				else {
					return (false, res, (int)response.StatusCode);
				}
			}
			catch (HttpRequestException ex) {
				Debug.Log("HTTP request error: " + ex.Message);
				return (false, "HTTP request error: " + ex.Message, (int)HttpStatusCode.BadRequest);
			}
		}


		/// <summary>
		/// For creating a new item over a web api using POST
		/// </summary>
		/// <param name="apiUrl">API Url</param>
		/// <param name="postObject">The object to be created</param>
		/// <returns>A Task with created item</returns>
		private async Task<(bool, string, int)> PostRequest(string endpoint, string contentString, CancellationToken cancellationToken = default) {

			try {
				var httpClient = new HttpClient();
				string url = baseUrl + endpoint;

				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
				HttpContent httpContent = new StringContent(contentString, Encoding.UTF8, "application/json");
				HttpResponseMessage response = await httpClient.PostAsync(url, httpContent, cancellationToken);
				response.EnsureSuccessStatusCode();

				var res = await response.Content.ReadAsStringAsync();

				Debug.Log($"Response: {res}");

				if (response.IsSuccessStatusCode) {
					return (true, res, (int)response.StatusCode);
				}
				else {
					return (false, res, (int)response.StatusCode);
				}
			}
			catch (HttpRequestException ex) {
				Debug.Log("HTTP request error: " + ex.Message);
				return (false, "HTTP request error: " + ex.Message, (int)HttpStatusCode.BadRequest);
			}
		}

		/// <summary>
		/// For updating an existing item over a web api using PUT
		/// </summary>
		/// <param name="apiUrl">API Url</param>
		/// <param name="putObject">The object to be edited</param>
		private async Task PutRequest(string endpoint, string contentString, CancellationToken cancellationToken = default) {
			try {
				var httpClient = new HttpClient();
				string url = baseUrl + endpoint;

				httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
				HttpContent httpContent = new StringContent(contentString, Encoding.UTF8, "application/json");

				var response = await httpClient.PutAsync(url, httpContent, cancellationToken);

				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException ex) {
				Debug.Log("HTTP request error: " + ex.Message);
			}
		}

		public static bool TryConvertCountryStringToCode(string country, out string code) {
			code = ServiceLocator.Current.Get<CountryCodesManager>().GetCountryCode(country);
			return !string.IsNullOrEmpty(code);
		}
	}
}