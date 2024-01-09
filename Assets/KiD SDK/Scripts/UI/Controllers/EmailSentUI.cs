using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmailSentUI : BaseUI {

	public override void ShowUI() {
		base.ShowUI();
	}

	public override void HideUI() {
		base.HideUI();
	}

	#region BUTTON ONCLICK

	public void OnLetsPlayButtonClick() {
		//TODO:- Show wait for approval message or redirect to game
		uiManager.OnPlayButtonClick();
	}

	#endregion
}
