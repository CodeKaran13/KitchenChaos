using System.Linq;
using KIdentify.Example;
using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.UI {
	public class AgeGateMiniGameUI : BaseUI {

		[Header("Success Screen")]
		[SerializeField] private GameApprovalSuccessUI gameSuccessUI;

		[Header("Mini-Game")]
		public CatchGame CatchGame;
		public GameObject instructionsUiContainer;
		public GameObject gameOverUiContainer;
		public Text scoreText;

		private int score;
		private WebCamTexture camTexture;

		public override void ShowUI() {
			base.ShowUI();
			ResetUI();
			OpenCamera();
		}

		public override void HideUI() {
			HideSuccessUI();
			base.HideUI();
		}

		public void OnGameOver() {
			gameOverUiContainer.SetActive(true);
			DisplayScore();
		}

		public void IncrementScore() {
			score++;
		}

		public void ShowSuccessUI() {
			gameSuccessUI.ShowUI();
		}

		private void HideSuccessUI() {
			gameSuccessUI.HideUI();
		}

		#region BUTTON ONCLICK

		public void OnReadyButtonClick() {
			instructionsUiContainer.SetActive(false);
			CatchGame.StartMiniGame();
		}

		public void OnNextButtonClick() {
			gameOverUiContainer.SetActive(false);
			KiDManager.Instance.ValidateAge();
		}

		#endregion

		private void OpenCamera() {
			WebCamDevice[] devices = WebCamTexture.devices;
			if (devices.Length == 0) {
				return;
			}

			WebCamDevice webCamDevice = devices.FirstOrDefault(device => device.isFrontFacing);
			if (webCamDevice.Equals(default(WebCamDevice))) {
				webCamDevice = devices[0];
			}

			camTexture = new WebCamTexture(webCamDevice.name, 600, 600);
			camTexture.Play();

			InvokeRepeating(nameof(UpdateCamTexture), 0f, 0.5f);
		}

		private void UpdateCamTexture() {
			uiManager.SendImageTexture(camTexture);
		}

		private void DisplayScore() {
			scoreText.text = $"Score: {score}";
		}

		private void ResetUI() {
			score = 0;
			gameOverUiContainer.SetActive(false);
			instructionsUiContainer.SetActive(true);
		}
	}
}
