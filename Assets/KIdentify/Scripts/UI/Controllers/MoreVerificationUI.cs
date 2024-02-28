using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.UI {
	public class MoreVerificationUI : BaseUI {

		[SerializeField] private Image[] methodImages;
		[SerializeField] private Sprite unselectedSprite;
		[SerializeField] private Sprite selectedSprite;
		[SerializeField] private Button nextButton;

		private int currentSelectedMethodIndex = -1;


		public override void ShowUI() {
			ResetMethodSelection();
			base.ShowUI();
		}

		public override void HideUI() {
			base.HideUI();
		}

		#region BUTTON CLICK

		public void OnVerificationMethodButtonClick(int index) {
			SelectVerificationMethodUI(index);
			currentSelectedMethodIndex = index;
			EnableNextButton();
		}

		public void OnContinueButtonClick() {
			switch (currentSelectedMethodIndex) {
				case 0:
					//uiManager.ShowEmail();
					break;
				case 1:
					uiManager.ShowQR();
					break;
				case 2:
					Debug.Log($"Parent portal");
					break;
				default:
					break;
			}
		}

		public void OnBackButtonClick() {
			uiManager.ShowPreviousUI();
		}

		#endregion


		private void SelectVerificationMethodUI(int index) {
			for (int i = 0; i < methodImages.Length; i++) {
				if (i == index) {
					methodImages[i].sprite = selectedSprite;
				}
				else {
					methodImages[i].sprite = unselectedSprite;
				}
			}
		}

		private void ResetMethodSelection() {
			currentSelectedMethodIndex = -1;
			DisableNextButton();
			foreach (Image image in methodImages) {
				image.sprite = unselectedSprite;
			}
		}

		private void EnableNextButton() {
			if (!nextButton.interactable)
				nextButton.interactable = true;
		}

		private void DisableNextButton() {
			if (nextButton.interactable)
				nextButton.interactable = false;
		}
	}
}