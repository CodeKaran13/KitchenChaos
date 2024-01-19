using Kidentify.Example;
using Kidentify.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour {
	[SerializeField] private GameObject uiContainer;

	protected KidUIManager uiManager;

	protected virtual void Start () {
		uiManager = KiDManager.Instance.uiManager;
	}

	public virtual void ShowUI() {
		uiContainer.SetActive(true);
	}

	public virtual void HideUI() {
		uiContainer.SetActive(false);
	}
}
