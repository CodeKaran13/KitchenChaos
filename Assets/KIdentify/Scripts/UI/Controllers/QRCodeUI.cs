using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode.Internal;
using TMPro;

namespace KIdentify.UI {
	public class QRCodeUI : BaseUI {

		[SerializeField] private RawImage qrImage;
		[SerializeField] private TextMeshProUGUI otpText;

		[Header("UI Screens")]
		[SerializeField] private GameObject qrCodeUIContainer;

		private const string OTP_MESSAGE = "Or go to <color=#715DEC><link=\"parent.k-id.com\">parent.k-id.com</link></color> and enter code ";

		private Texture2D qrCodeTexture;

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