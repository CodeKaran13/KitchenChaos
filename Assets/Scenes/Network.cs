using UnityEngine;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System;

public class Network
{
    private const string endpoint = "https://epf7ynz0ni.execute-api.eu-west-1.amazonaws.com/default/d-privately-cognito-communicator-python";
    private string apiKey;
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> Authenticate(string apiKey, string apiSecret)
    {
        var values = new Dictionary<string, string>
            {
                { "execution_mode", "authenticate_sdk_session" },
                { "session_id", apiKey },
                { "session_password", apiSecret },
                { "device_id", "42" },
                { "platform", "unity" },
                { "device_info", "macos" }
            };

        var content = new FormUrlEncodedContent(values);

        var response = await client.PostAsync("https://epf7ynz0ni.execute-api.eu-west-1.amazonaws.com/default/d-privately-cognito-communicator-python", content);

        var responseString = await response.Content.ReadAsStringAsync();

        if (responseString.Contains("\"result\": \"OK\""))
        {
            this.apiKey = apiKey;
            var keyStartIndex = responseString.IndexOf("decryption_key") + "decryption_key".Length + 4;
            var keyEndIndex = responseString.LastIndexOf("\"");

            return responseString.Substring(keyStartIndex, keyEndIndex - keyStartIndex);
        }
        else
        {
            return "";
        }
    }

    public async void SendAnalytics(float age)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            var body =
                "{\n" +
                    "\"id\": \"" + Guid.NewGuid().ToString("N") + "\",\n" +
                    "\"execution_mode\": \"logging\",\n" +
                    "\"session_id\": \"" + apiKey + "\",\n" +
                    "\"platform\": \"android\",\n" +
                    "\"sdk_version\": \"" + "0.0.1" + "\",\n" +
                    "\"app_version\": \"" + "0.0.1" + "\",\n" +
                    "\"execution_info\": {\n" +
                        "\"estimation_time\":\"" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "\",\n" +
                        "\"product\": \"age\",\n" +
                        "\"modality\": \"image\",\n" +
                        "\"analysis_result\": {\n" +
                            "\"result\":\"" + age.ToString("0.##") + "\",\n" +
                            "\"spoof_score\":\"" + "0.0" + "\"\n" +
                        "}\n" +
                    "},\n" +
                    "\"deviceInfo\":\"" + GetDeviceName() + "\"" +
                "}";
            streamWriter.Write(body);
        }

        await Task.Run(() =>
        {
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                UnityEngine.Debug.Log(result);
            }
        });
    }

    internal void SetApiKey(string apiKey)
    {
        this.apiKey = apiKey;
    }

    void GetDeviceId()
    {

    }

    string GetDeviceName()
    {
        return "unity_device";
    }
}
