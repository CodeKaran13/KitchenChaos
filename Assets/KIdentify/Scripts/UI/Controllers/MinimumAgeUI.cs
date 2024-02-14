using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KIdentify.UI {
	public class MinimumAgeUI : BaseUI, IPointerClickHandler {
		[SerializeField] private TextMeshProUGUI minimumAgeText;

		private const string MINIMUM_AGE_LINK_ID = "minimumAge";

		public void OnPointerClick(PointerEventData eventData) {
			// First, get the index of the link clicked. Each of the links in the text has its own index.
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(minimumAgeText, Input.mousePosition, null);

			// As the order of the links can vary easily (e.g. because of multi-language support),
			// you need to get the ID assigned to the links instead of using the index as a base for our decisions.
			// you need the LinkInfo array from the textInfo member of the TextMesh Pro object for that.
			if (linkIndex != -1) {
				var linkId = minimumAgeText.textInfo.linkInfo[linkIndex].GetLinkID();

				// Now finally you have the ID in hand to decide what to do. Don't forget,
				// you don't need to make it act like an actual link, instead of opening a web page,
				// any kind of functions can be called.
				switch (linkId) {
					case MINIMUM_AGE_LINK_ID:
						Debug.Log($"MinimumAge link clicked");
						break;
					default:
						break;
				}
			}

			// Let's see that web page!
			//Application.OpenURL(url);
		}

		public override void ShowUI() {
			base.ShowUI();
		}

		public override void HideUI() {
			base.HideUI();
		}

		#region BUTTON ONCLICK

		public void OnBackButtonClick() {
			uiManager.ShowSignUp();
		}

		#endregion
	}
}