using System.IO;    
using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(WebCamInput))]
public class AgeEstimationSample : MonoBehaviour
{
    private static string modelPath = "Assets/StreamingAssets/model_reg_mug.tflite.enc";

    private RawImage cameraView = null;

    private AgeEstimator ageEstimator;
    private PrimitiveDraw draw;
    private Network network;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Starting");
        network = new Network();
        Debug.Log("Network loaded");
        draw = new PrimitiveDraw(Camera.main, gameObject.layer);

        var webCamInput = GetComponent<WebCamInput>();
        webCamInput.OnTextureUpdate.AddListener(OnTextureUpdate);

        LoadEstimator();
    }

    async void LoadEstimator()
    {
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
        ageEstimator.OnTextureUpdate(texture);

        if (ageEstimator.IsResultReady())
        {
            var result = ageEstimator.GetResult();
            Debug.Log("Min age: " + result.minAge + ", max age: " + result.maxAge);
        }
    }
}

