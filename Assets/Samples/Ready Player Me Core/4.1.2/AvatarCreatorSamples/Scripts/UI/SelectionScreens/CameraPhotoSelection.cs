using System;
using System.Linq;
using ReadyPlayerMe.AvatarCreator;
using UnityEngine;
using UnityEngine.UI;

namespace ReadyPlayerMe {
	public class CameraPhotoSelection : State {
		public static EventHandler<OnImageCapturedEventArgs> OnImageCaptured;
		public class OnImageCapturedEventArgs : EventArgs {
			public Texture texture;
		}

		[SerializeField] private RawImage rawImage;
		[SerializeField] private Button cameraButton;
		[SerializeField] private Text timerText;

		public override StateType StateType => StateType.CameraPhoto;
		public override StateType NextState => StateType.Editor;

		private WebCamTexture camTexture;
		private int videoOrientationAngle;
		private int updateTextureCount = 0;
		private int camTimer = 15;

		public override async void ActivateState() {
			cameraButton.onClick.AddListener(OnCameraButton);
			if (!AuthManager.IsSignedIn && !AuthManager.IsSignedInAnonymously) {
				await AuthManager.LoginAsAnonymous();
			}
			OpenCamera();
		}

		public override void DeactivateState() {
			cameraButton.onClick.RemoveListener(OnCameraButton);
			CloseCamera();
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

			//Vector2 size = rawImage.rectTransform.sizeDelta;
			//camTexture = new WebCamTexture(webCamDevice.name, (int)size.x, (int)size.y);
			//camTexture.Play();
			//rawImage.texture = camTexture;
			//rawImage.SizeToParent();

			Vector2 size = rawImage.rectTransform.sizeDelta;
			camTexture = new WebCamTexture(webCamDevice.name, (int)size.x, (int)size.y);
			camTexture.Play();

			videoOrientationAngle = -camTexture.videoRotationAngle;
			Debug.Log($"angle: {videoOrientationAngle}");

			rawImage.texture = camTexture;
			rawImage.SizeToParent();
			rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, videoOrientationAngle);

			updateTextureCount = 0;
			camTimer = 15;
			InvokeRepeating(nameof(UpdateCamTexture), 0f, 0.5f);
			InvokeRepeating(nameof(ShowTimer), 1f, 1f);
		}

		private void CloseCamera() {
			if (camTexture != null && camTexture.isPlaying) {
				camTexture.Stop();
			}
		}

		private void OnCameraButton() {
			if (camTexture == null || !camTexture.isPlaying) {
				LoadingManager.EnableLoading("Camera is not available.", LoadingManager.LoadingType.Popup, false);
				return;
			}

			var texture = new Texture2D(rawImage.texture.width, rawImage.texture.height, TextureFormat.ARGB32, false);
			texture.SetPixels(camTexture.GetPixels());

			texture = RotateTexture(texture, videoOrientationAngle);

			texture.Apply();

			var bytes = texture.EncodeToPNG();

			AvatarCreatorData.AvatarProperties.Id = string.Empty;
			AvatarCreatorData.AvatarProperties.Base64Image = Convert.ToBase64String(bytes);
			AvatarCreatorData.IsExistingAvatar = false;

			StateMachine.SetState(StateType.Editor);
		}

		private void UpdateCamTexture() {
			OnImageCaptured?.Invoke(this, new OnImageCapturedEventArgs { texture = rawImage.texture });
		}

		private void ShowTimer() {
			camTimer--;
			if (camTimer > 0) {
				timerText.text = $" {camTimer}";
			}
			else {
				CancelInvoke();
				timerText.text = "";
				OnCameraButton();
				CloseCamera();
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
	}
}
