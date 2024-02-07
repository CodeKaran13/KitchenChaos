using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kidentify.Example;
using ReadyPlayerMe;
using UnityEngine;
using UnityEngine.UI;

public class AgeGateNoAvatarUI : BaseUI {
	private const string TERMS_URL = "https://readyplayer.me/terms";
	private const string PRIVACY_URL = "https://readyplayer.me/privacy";

	[Header("Selfie Screen UI")]
	[SerializeField] private GameObject selfieScreenUiContainer;
	[SerializeField] private Button photoButton;
	[SerializeField] private Button termsButton;
	[SerializeField] private Button privacyButton;
	[Header("Camera Screen UI")]
	[SerializeField] private GameObject cameraScreenUiContainer;
	[SerializeField] private RawImage rawImage;
	[SerializeField] private Text timerText;

	private WebCamTexture camTexture;
	private int videoOrientationAngle;
	private int camTimer = 10;

	public override void ShowUI() {
		base.ShowUI();
		ShowSelfieScreenUI();
	}

	public override void HideUI() {
		base.HideUI();
		
	}

	private void ShowSelfieScreenUI() {
		photoButton.onClick.AddListener(OnPhotoButton);
		termsButton.onClick.AddListener(OnTermsButton);
		privacyButton.onClick.AddListener(OnPrivacyButton);

		selfieScreenUiContainer.SetActive(true);
		cameraScreenUiContainer.SetActive(false);
	}

	private void HideSelfieScreenUI() {
		photoButton.onClick.RemoveListener(OnPhotoButton);
		termsButton.onClick.RemoveListener(OnTermsButton);
		privacyButton.onClick.RemoveListener(OnPrivacyButton);

		selfieScreenUiContainer.SetActive(false);
		cameraScreenUiContainer.SetActive(true);
	}

	private void ShowCameraScreenUI() {
		OpenCamera();
	}

	private void HideCameraScreenUI() {
		CloseCamera();
		cameraScreenUiContainer.SetActive(false);
		KiDManager.Instance.ValidateAge();
	}

	private void OpenCamera() {
		WebCamDevice[] devices = WebCamTexture.devices;
		if (devices.Length == 0) {
			return;
		}

		rawImage.color = Color.white;

		WebCamDevice webCamDevice = devices.FirstOrDefault(device => device.isFrontFacing);
		if (webCamDevice.Equals(default(WebCamDevice))) {
			webCamDevice = devices[0];
		}

		Vector2 size = rawImage.rectTransform.sizeDelta;
		camTexture = new WebCamTexture(webCamDevice.name, (int)size.x, (int)size.y);
		camTexture.Play();

		videoOrientationAngle = -camTexture.videoRotationAngle;
		Debug.Log($"angle: {videoOrientationAngle}");

		rawImage.texture = camTexture;
		rawImage.SizeToParent();
		rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, videoOrientationAngle);

		camTimer = 10;
		InvokeRepeating(nameof(UpdateCamTexture), 0f, 0.5f);
		InvokeRepeating(nameof(ShowTimer), 1f, 1f);
	}

	private void CloseCamera() {
		if (camTexture != null && camTexture.isPlaying) {
			camTexture.Stop();
		}
	}

	private void UpdateCamTexture() {
		uiManager.SendImageTexture(rawImage.texture);
	}

	private void ShowTimer() {
		camTimer--;
		if (camTimer > 0) {
			timerText.text = $" {camTimer}";
		}
		else {
			CancelInvoke();
			timerText.text = "";
			HideCameraScreenUI();
		}
	}

	private Texture2D RotateTexture(Texture2D texture, float eulerAngles) {
		int x;
		int y;
		int i;
		int j;
		float phi = eulerAngles / (180 / Mathf.PI);
		float sn = Mathf.Sin(phi);
		float cs = Mathf.Cos(phi);
		Color32[] arr = texture.GetPixels32();
		Color32[] arr2 = new Color32[arr.Length];
		int W = texture.width;
		int H = texture.height;
		int xc = W / 2;
		int yc = H / 2;

		for (j = 0; j < H; j++) {
			for (i = 0; i < W; i++) {
				arr2[j * W + i] = new Color32(0, 0, 0, 0);

				x = (int)(cs * (i - xc) + sn * (j - yc) + xc);
				y = (int)(-sn * (i - xc) + cs * (j - yc) + yc);

				if ((x > -1) && (x < W) && (y > -1) && (y < H)) {
					arr2[j * W + i] = arr[y * W + x];
				}
			}
		}

		Texture2D newImg = new Texture2D(W, H);
		newImg.SetPixels32(arr2);
		newImg.Apply();

		return newImg;
	}

	#region BUTTON ONCLICK

	private void OnPhotoButton() {
		HideSelfieScreenUI();
		ShowCameraScreenUI();
	}

	private void OnTermsButton() {
		Application.OpenURL(TERMS_URL);
	}

	private void OnPrivacyButton() {
		Application.OpenURL(PRIVACY_URL);
	}

	#endregion
}
