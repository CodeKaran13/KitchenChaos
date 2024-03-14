using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour
{

	[SerializeField] private GameObject uiContainer;
	[SerializeField] private Button agreeButton;
	[SerializeField] private Button disagreeButton;
	[SerializeField] private Button privatelyButton;
	[SerializeField] private Button contactUsButton;

	private Action<bool> caller;

	private void OnEnable()
	{
		agreeButton.onClick.AddListener(delegate { OnAgreeButtonClick(); });
		disagreeButton.onClick.AddListener(delegate { OnDisagreeButtonClick(); });
		privatelyButton.onClick.AddListener(delegate
		{
			OnPrivatelyButtonClick();
		});
		contactUsButton.onClick.AddListener(delegate
		{
			OnContactUsButtonClick();
		});
	}

	private void OnDisable()
	{
		agreeButton.onClick.RemoveListener(delegate { OnAgreeButtonClick(); });
		disagreeButton.onClick.RemoveListener(delegate { OnDisagreeButtonClick(); });
		privatelyButton.onClick.RemoveListener(delegate
		{
			OnPrivatelyButtonClick();
		});
		contactUsButton.onClick.RemoveListener(delegate
		{
			OnContactUsButtonClick();
		});
	}

	public void ShowPopup(Action<bool> callback)
	{
		caller = callback;
		uiContainer.SetActive(true);
	}

	public void HidePopup()
	{
		uiContainer.SetActive(false);
	}

	private void OnAgreeButtonClick()
	{
		HidePopup();
		caller?.Invoke(true);
	}

	private void OnDisagreeButtonClick()
	{
		HidePopup();
		caller?.Invoke(false);
	}

	private void OnPrivatelyButtonClick()
	{
		Application.OpenURL("https://www.privately.eu/");
	}

	private void OnContactUsButtonClick()
	{
		Application.OpenURL("https://www.k-id.com/privacy-policy");
	}
}
