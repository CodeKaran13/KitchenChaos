using System.Collections;
using KIdentify.Sample;
using KIdentify.Services;
using KIdentify.Sample.Tools;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionUI : MonoBehaviour
{
	[SerializeField] private GameObject uiContainer;

	[SerializeField] private GameObject loading;
	[SerializeField] private GameObject selectPlayer;
	[SerializeField] private RenderPanel[] renderPanels;
	[SerializeField] private Button nextButton;
	[SerializeField] private Image loadingBarImage;

	[SerializeField] private UIManager uiManager;

	private PlayerPrefsManager playerPrefsManager;

	public int PlayersLoaded { get; set; } = 0;

	private void Start()
	{
		playerPrefsManager = ServiceLocator.Current.Get<PlayerPrefsManager>();
	}

	public void ShowUI()
	{
		ResetUI();
		uiContainer.SetActive(true);

		StartCoroutine(LoadPlayersCo());
	}

	public void HideUI()
	{
		uiContainer.SetActive(false);
	}

	public void SaveAvatarRender(RenderPanel renderPanel, Texture2D texture)
	{
		playerPrefsManager.SaveAvatarRender(renderPanel.Url, texture);
	}

	public Texture2D GetRenderForAvatar(string url)
	{
		return playerPrefsManager.GetAvatarRender(url);
	}

	public void SelectPlayer(int index)
	{
		nextButton.interactable = true;
		for (int i = 0; i < renderPanels.Length; i++)
		{
			if (i == index)
			{
				renderPanels[i].SetSelected();
				KiDManager.Instance.SelectedRPMUrl = renderPanels[i].Url;
			}
			else
			{
				renderPanels[i].SetDeselected();
			}
		}
	}

	#region BUTTON ONCLICK

	public void OnNextButtonClick()
	{
		uiManager.ShowMainMenu();
	}

	#endregion

	private IEnumerator LoadPlayersCo()
	{
		yield return new WaitUntil(HasFinishedLoading);
		loading.SetActive(false);
		selectPlayer.SetActive(true);
		nextButton.gameObject.SetActive(true);
		foreach (var renderPanel in renderPanels)
		{
			renderPanel.ShowPanel();
		}
	}

	private bool HasFinishedLoading()
	{
		for (int renderPanelIndex = 0; renderPanelIndex < renderPanels.Length; renderPanelIndex++)
		{
			if (!renderPanels[renderPanelIndex].IsReady)
			{
				return false;
			}
		}
		return true;
	}

	private void ResetUI()
	{
		PlayersLoaded = 0;
		loading.SetActive(true);
		selectPlayer.SetActive(false);
		nextButton.gameObject.SetActive(false);
		nextButton.interactable = false;
	}

	private void Update()
	{
		if (loadingBarImage != null)
		{
			UpdateLoadingBar();
		}
	}

	private void UpdateLoadingBar()
	{
		loadingBarImage.fillAmount = ((float)PlayersLoaded / renderPanels.Length);
	}
}
