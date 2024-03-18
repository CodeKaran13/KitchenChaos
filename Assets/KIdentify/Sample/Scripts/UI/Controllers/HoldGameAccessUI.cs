namespace KIdentify.Sample.UI {
	public class HoldGameAccessUI : BaseUI {

		#region BUTTON ONCLICK

		public void OnAuthorizeMeButtonClick() {
			KiDManager.Instance.AgeGateCheck();
		}

		#endregion

	}
}