using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode.Internal;
using TMPro;
using UnityEngine.EventSystems;

namespace KIdentify.UI {
	public class QRCodeUI : BaseUI, IPointerClickHandler {

		[SerializeField] private RawImage qrImage;
		[SerializeField] private TextMeshProUGUI kidParentPortalText;
		[SerializeField] private TextMeshProUGUI otpText;

		[Header("UI Screens")]
		[SerializeField] private GameObject qrCodeUIContainer;

		private const string OTP_MESSAGE = "";
		private const string KID_PORTAL_LINK_ID = "k-id-portal";

		private Texture2D qrCodeTexture;

		public void OnPointerClick(PointerEventData eventData) {
			// First, get the index of the link clicked. Each of the links in the text has its own index.
			var linkIndex = TMP_TextUtilities.FindIntersectingLink(kidParentPortalText, Input.mousePosition, null);

			// As the order of the links can vary easily (e.g. because of multi-language support),
			// you need to get the ID assigned to the links instead of using the index as a base for our decisions.
			// you need the LinkInfo array from the textInfo member of the TextMesh Pro object for that.
			if (linkIndex != -1) {
				var linkId = kidParentPortalText.textInfo.linkInfo[linkIndex].GetLinkID();

				// Now finally you have the ID in hand to decide what to do. Don't forget,
				// you don't need to make it act like an actual link, instead of opening a web page,
				// any kind of functions can be called.
				switch (linkId) {
					case KID_PORTAL_LINK_ID:
						Debug.Log($"Family portal link clicked");
						// Let's see that web page!
						Application.OpenURL("family.k-id.com");
						break;
					default:
						break;
				}
			}


		}

		public override void ShowUI() {
			GenerateQrCode(uiManager.QRCodeURL);
			SetOTP(uiManager.OTP);
			base.ShowUI();
			ShowQRCodeUI();
		}

		public override void HideUI() {
			HideQRCodeUI();
			base.HideUI();
		}

		private void ShowQRCodeUI() {
			qrCodeUIContainer.SetActive(true);
		}

		private void HideQRCodeUI() {
			qrCodeUIContainer.SetActive(false);
		}

		private void GenerateQrCode(string qrCode) {

			qrCodeTexture = new Texture2D(300, 300);
			Color32[] convertPixelsToTexture = Encode(qrCode, qrCodeTexture.width, qrCodeTexture.height);

			qrCodeTexture.SetPixels32(convertPixelsToTexture);
			qrCodeTexture.Apply();

			qrImage.texture = qrCodeTexture;
		}

		private void SetOTP(string otp) {
			otpText.text = OTP_MESSAGE + $"<color=#715DEC>{otp}</color>";
		}

		#region BUTTON ONCLICK

		public void OnUseAnotherMethodButtonClick() {
			uiManager.ShowMoreVerificationUI();
		}

		#endregion

		private Color32[] Encode(string qrCode, int width, int height) {

			var encodingOptions = new ZXing.Common.EncodingOptions {
				Height = height,
				Width = width,
				Margin = 0,
				PureBarcode = false
			};
			encodingOptions.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

			BarcodeWriter barcodeWriter = new BarcodeWriter();
			barcodeWriter.Format = BarcodeFormat.QR_CODE;
			barcodeWriter.Options = encodingOptions;
			return barcodeWriter.Write(qrCode);
		}
	}
}