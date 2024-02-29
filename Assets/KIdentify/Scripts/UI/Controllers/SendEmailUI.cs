using KIdentify.Example;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.UI
{
	public class SendEmailUI : BaseUI
	{

		[SerializeField] private GameObject popUpContainer;

		[SerializeField] private TMP_InputField emailInputField;
		[SerializeField] private Button emailSubmitButton;


		public override void ShowUI()
		{
			ResetUI();
			base.ShowUI();
		}

		public override void HideUI()
		{
			base.HideUI();
			popUpContainer.SetActive(false);
		}

		#region BUTTON ONCLICK

		public void OnBackButtonClick()
		{
			KiDManager.Instance.UIManager.ShowPreviousUI();
		}

		public void OnSubmitButtonClick()
		{
			popUpContainer.SetActive(true);
		}

		public void OnEmailInputEndEdit()
		{
			if (EmailValidator.IsEmail(emailInputField.text))
			{
				emailSubmitButton.interactable = true;
			}
			else
			{
				emailSubmitButton.interactable = false;
			}
		}

		public void OnPopupContinueButtonClick()
		{
			string email = emailInputField.text;
			KiDManager.Instance.SendEmail(email);
		}

		public void OnPopupBackButtonClick()
		{
			popUpContainer.SetActive(false);
		}

		#endregion

		private void ResetUI()
		{
			emailSubmitButton.interactable = false;
			emailInputField.contentType = TMP_InputField.ContentType.EmailAddress;
			emailInputField.characterValidation = TMP_InputField.CharacterValidation.EmailAddress;
		}
	}
}