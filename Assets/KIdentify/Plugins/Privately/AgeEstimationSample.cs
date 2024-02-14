using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(WebCamInput))]
public class AgeEstimationSample : MonoBehaviour
{
    private static string modelPath = "model_reg_mug.tflite.enc";

    private RawImage cameraView = null;

    private AgeEstimator ageEstimator;
    private PrimitiveDraw draw;
    private Network network;
    private List<Texture> images = new();
    private long lastImageCapture = GetCurrentTimestamp();
    private RenderTexture renderTexture; // RenderTexture to render the source texture
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();



    // Use this for initialization
    void Start()
    {
        Debug.Log("Starting");
        network = new Network();
        Debug.Log("Network loaded");
        // draw = new PrimitiveDraw(Camera.main, gameObject.layer);

        var webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.AddListener(OnTextureUpdate);

        LoadEstimator();
    }

    async void LoadEstimator()
    {
        Debug.Log("Loading estimator");
        ageEstimator = new AgeEstimator();
        var key = await network.Authenticate("f6c90326-1cee-40fd-9dd9-d8ada6873449", "SjdGcae-17rNBCnQV1hvYFcWj0v037");
        var binary = ModelDecryptor.Decryptor.DecryptModel(modelPath, key);
        ageEstimator.LoadModel(binary);
        Debug.Log(ageEstimator.IsLoaded());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTextureUpdate(Texture texture)
    {
        /* var currentTime = GetCurrentTimestamp();

        if (currentTime > lastImageCapture + 500 && images.Count < 10)
        {
            images.Add(CopyTexture(texture));
            lastImageCapture = currentTime;

            if (images.Count > 10)
            {
                images.RemoveAt(0);
            }

            if (images.Count >= 7)
            {
                var result = ageEstimator.EstimateAge(images, 24f);
                Debug.Log("Age gate " + result.ageGate + " : " + result.ageGatePassed + ", " + result.isReady + ", " + result.errorMessage);
            }
        } */
        ageEstimator.OnTextureUpdate(texture);

        if (ageEstimator.IsResultReady())
        {
            var result = ageEstimator.GetResult(22f);
            Debug.Log("Age gate " + result.ageGate + " : " + result.ageGatePassed + " estimation: " + result.minAge + " - " + result.maxAge);
        }
    }

    private static long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private Texture CopyTexture(Texture sourceTexture)
    {
        RenderTexture resizeTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(sourceTexture, resizeTexture);

        var prevRT = RenderTexture.active;

        Texture2D fetchTexture = new Texture2D(resizeTexture.width, resizeTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = resizeTexture;

        fetchTexture.ReadPixels(new Rect(0, 0, resizeTexture.width, resizeTexture.height), 0, 0);
        fetchTexture.Apply();

        RenderTexture.active = prevRT;

        return fetchTexture;
    }
}

