using System.Collections;
using Kidentify.Example;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionUI : MonoBehaviour {

	[SerializeField] private GameObject uiContainer;

	[SerializeField] private GameObject loading;
	[SerializeField] private GameObject selectPlayer;
	[SerializeField] private RenderPanel[] renderPanels;
	[SerializeField] private Button nextButton;

	[SerializeField] private UIManager uiManager;

	public void ShowUI() {
		ResetUI();
		uiContainer.SetActive(true);

		StartCoroutine(LoadPlayersCo());
	}

	public void HideUI() {
		uiContainer.SetActive(false);
	}

	public void SetPlayerSelection(int index) {
		nextButton.interactable = true;
		for (int i = 0; i < renderPanels.Length; i++) {
			if (i == index) {
				renderPanels[i].SetSelected();
				KiDManager.Instance.SelectedRPMUrl = renderPanels[i].Url;
			}
			else {
				renderPanels[i].SetDeselected();
			}
		}
	}

	#region BUTTON ONCLICK

	public void OnNextButtonClick() {
		uiManager.ShowMainMenu();
	}

	#endregion

	private IEnumerator LoadPlayersCo() {
		yield return new WaitUntil(HasFinishedLoading);
		loading.SetActive(false);
		selectPlayer.SetActive(true);
		nextButton.gameObject.SetActive(true);
		foreach (var renderPanel in renderPanels) {
			renderPanel.ShowPanel();
		}
	}

	private bool HasFinishedLoading() {
		foreach (var renderPanel in renderPanels) {
			if (!renderPanel.IsReady) {
				return false;
			}
			else {
				continue;
			}
		}
		return true;
	}

	private void ResetUI() {
		loading.SetActive(true);
		selectPlayer.SetActive(false);
		nextButton.gameObject.SetActive(false);
		nextButton.interactable = false;
	}
}
