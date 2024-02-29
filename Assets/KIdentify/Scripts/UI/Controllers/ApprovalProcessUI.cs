using KIdentify.Example;

namespace KIdentify.UI {
	public class ApprovalProcessUI : BaseUI {

		public override void ShowUI() {
			base.ShowUI();
		}

		public override void HideUI() {
			base.HideUI();
		}

		#region BUTTON ONCLICK

		public void OnLetsPlayButtonClick() {
			KiDManager.Instance.UIManager.OnPlayButtonClick();
		}

		#endregion
	}
}