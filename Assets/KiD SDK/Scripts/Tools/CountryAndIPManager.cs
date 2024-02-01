using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

internal class CountryAndIPManager {
	private readonly string _geoLocationUrl = "https://ipapi.co/{0}/json/";
	private readonly string _externalIpUrl = "http://api.ipify.org";

	public async Task<string> FindCountryByExternalIP() {
		string ipAddress = await GetExternalIP();
		if (ipAddress == null) {
			Debug.LogError("Unable to get external ip");
			return "US-CA";
		}

		string countryCode = await GetCountryByIP(ipAddress);

		if (countryCode == null) {
			return "US-CA"; //default
		}

		return countryCode;
	}

	public async Task<string> GetExternalIP() {
		(bool status, string content, int _) = await GetServerResponse(_externalIpUrl);

		if (status) {
			return content; // This is a JSON string, you will need to parse it.
		}
		else {
			KidLogger.LogError("Unable to get country by IP");
			return null;
		}
	}

	[System.Serializable]
	public class IPInfo {
		public string country_code;
		public string region_code;
	}

	public async Task<string> GetCountryByIP(string ipAddress) {
		string url = string.Format(_geoLocationUrl, ipAddress);

		(bool status, string content, int _) = await GetServerResponse(url);

		if (status) {
			IPInfo info = JsonUtility.FromJson<IPInfo>(content);
			if (string.IsNullOrEmpty(info.region_code)) {
				return info.country_code; // This is a JSON string, you will need to parse it.
			}
			else {
				return $"{info.country_code}-{info.region_code}";
			}
		}
		else {
			KidLogger.LogError("Unable to get country by IP");
			return null;
		}
	}

	public async Task<(bool, string, int)> GetServerResponse(string endpoint) {
		string result = "";

		try {
			string url = endpoint;

			HttpClientHandler handler = new() {
				AutomaticDecompression =
					DecompressionMethods.GZip | DecompressionMethods.Deflate
			};

			// Create the request
			var httpClient = new HttpClient(handler);

			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:104.0) Gecko/20100101 Firefox/104.0");

			httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

			HttpResponseMessage response;
			string res;

			response = await httpClient.GetAsync(url);

			res = await response.Content.ReadAsStringAsync();

			var headersString = string.Join(Environment.NewLine, response.Headers
				.Select(header => $"{header.Key}: {string.Join(", ", header.Value)}")
				.Concat(response.Content.Headers.Select(header =>
					$"{header.Key}: {string.Join(", ", header.Value)}")));

			result = res + " code:" + response.StatusCode + " headers:" + headersString;

			if (response.IsSuccessStatusCode) {
				return (true, res, (int)response.StatusCode);
			}
			else {
				// There was an error
				Debug.Log("URL: " + url);
				Debug.LogError("Error: " + response.StatusCode);
				Debug.LogError("Response: " + res);

				return (false, res, (int)response.StatusCode);
			}
		}
		catch (HttpRequestException ex) {
			Debug.LogError("HTTP request error: " + ex.Message);
			return (false, "HTTP request error: " + ex.Message, (int)HttpStatusCode.BadRequest);
		}
		catch (TaskCanceledException ex) {
			Debug.LogError("Request timed out: " + ex.Message);
			return (false, "Request timed out", (int)HttpStatusCode.RequestTimeout);
		}
	}
}
