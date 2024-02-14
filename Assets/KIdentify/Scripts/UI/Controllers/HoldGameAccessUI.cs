using KIdentify.Example;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KIdentify.UI {
	public class HoldGameAccessUI : BaseUI {





		#region BUTTON ONCLICK

		public void OnAuthorizeMeButtonClick() {
			KiDManager.Instance.AgeGateCheck();
		}

		#endregion

	}
}