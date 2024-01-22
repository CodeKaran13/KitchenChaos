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
		private int updateTextureCount = 0;
		private int camTimer = 10;

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

			Vector2 size = rawImage.rectTransform.sizeDelta;
			camTexture = new WebCamTexture(webCamDevice.name, (int)size.x, (int)size.y);
			camTexture.Play();
			rawImage.texture = camTexture;
			rawImage.SizeToParent();

			updateTextureCount = 0;
			camTimer = 10;
			InvokeRepeating(nameof(UpdateCamTexture), 0f, 0.5f);
			InvokeRepeating(nameof(ShowTimer), 1f, 1f);
		}

		private void CloseCamera() {
			if (camTexture != null && camTexture.isPlaying) {
				camTexture.Stop();
				CancelInvoke(nameof(UpdateCamTexture));
			}
		}

		private void OnCameraButton() {
			if (camTexture == null || !camTexture.isPlaying) {
				LoadingManager.EnableLoading("Camera is not available.", LoadingManager.LoadingType.Popup, false);
				return;
			}

			var texture = new Texture2D(rawImage.texture.width, rawImage.texture.height, TextureFormat.ARGB32, false);
			texture.SetPixels(camTexture.GetPixels());
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
	}
}
