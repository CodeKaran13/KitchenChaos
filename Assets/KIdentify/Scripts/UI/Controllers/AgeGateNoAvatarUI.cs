using System.Linq;
using KIdentify.Example;
using ReadyPlayerMe;
using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.UI
{
	public class AgeGateNoAvatarUI : BaseUI
	{
		[Header("Selfie Screen UI")]
		[SerializeField] private GameObject selfieScreenUiContainer;
		[SerializeField] private Button photoButton;
		[Header("Camera Screen UI")]
		[SerializeField] private GameObject cameraScreenUiContainer;
		[SerializeField] private RawImage rawImage;
		[SerializeField] private Image progressBarFillImage;

		private WebCamTexture camTexture;
		private bool isCameraOn = false;
		private int videoOrientationAngle;
		private readonly float camMaxTimer = 5f;
		private float camTimer = 0;

		private void Update()
		{
			if (isCameraOn)
			{
				camTimer += Time.deltaTime;
				if (camTimer > camMaxTimer)
				{
					isCameraOn = false;
					UpdateProgressBar(1f);
					HideCameraScreenUI();
				}
				else
				{
					UpdateProgressBar(camTimer / camMaxTimer);
				}
			}
		}

		public override void ShowUI()
		{
			base.ShowUI();
			ShowSelfieScreenUI();
		}

		public override void HideUI()
		{
			base.HideUI();
		}

		private void ShowSelfieScreenUI()
		{
			photoButton.onClick.AddListener(OnPhotoButton);

			selfieScreenUiContainer.SetActive(true);
			cameraScreenUiContainer.SetActive(false);
		}

		private void HideSelfieScreenUI()
		{
			photoButton.onClick.RemoveListener(OnPhotoButton);

			selfieScreenUiContainer.SetActive(false);
			cameraScreenUiContainer.SetActive(true);
		}

		private void ShowCameraScreenUI()
		{
			OpenCamera();
		}

		private void HideCameraScreenUI()
		{
			CancelInvoke(nameof(UpdateCamTexture));
			CloseCamera();
			cameraScreenUiContainer.SetActive(false);
			KiDManager.Instance.ValidateAge();
		}

		private void OpenCamera()
		{
			WebCamDevice[] devices = WebCamTexture.devices;
			if (devices.Length == 0)
			{
				return;
			}

			rawImage.color = Color.white;

			WebCamDevice webCamDevice = devices.FirstOrDefault(device => device.isFrontFacing);
			if (webCamDevice.Equals(default(WebCamDevice)))
			{
				webCamDevice = devices[0];
			}

			Vector2 size = rawImage.rectTransform.sizeDelta;
			Debug.Log($"sizeX: {size.x}, sizeY: {size.y}");
			camTexture = new WebCamTexture(webCamDevice.name, (int)size.x, (int)size.y);
			camTexture.Play();

			videoOrientationAngle = -camTexture.videoRotationAngle;
			Debug.Log($"angle: {videoOrientationAngle}");

			rawImage.texture = camTexture;
			rawImage.SizeToParent();
			rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, videoOrientationAngle);

			camTimer = 0;
			isCameraOn = true;
			InvokeRepeating(nameof(UpdateCamTexture), 0f, 0.3f);
		}

		private void CloseCamera()
		{
			if (camTexture != null && camTexture.isPlaying)
			{
				camTexture.Stop();
			}
		}

		private void UpdateCamTexture()
		{
			KiDManager.Instance.UIManager.SendImageTexture(camTexture);
		}

		private void UpdateProgressBar(float value)
		{
			progressBarFillImage.fillAmount = value;
		}

		private Texture2D RotateTexture(Texture2D texture, float eulerAngles)
		{
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

			for (j = 0; j < H; j++)
			{
				for (i = 0; i < W; i++)
				{
					arr2[j * W + i] = new Color32(0, 0, 0, 0);

					x = (int)(cs * (i - xc) + sn * (j - yc) + xc);
					y = (int)(-sn * (i - xc) + cs * (j - yc) + yc);

					if ((x > -1) && (x < W) && (y > -1) && (y < H))
					{
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

		private void OnPhotoButton()
		{
			if (KiDManager.Instance.ShowCameraAccess)
			{
				KiDManager.Instance.UIManager.popup.ShowPopup((access) =>
				{
					if (access)
					{
						HideSelfieScreenUI();
						ShowCameraScreenUI();
					}
					else
					{
						KiDManager.Instance.UIManager.SkipMagicAgeGate();
					}
				});
			}
			else
			{
				HideSelfieScreenUI();
				ShowCameraScreenUI();
			}
		}

		#endregion
	}
}