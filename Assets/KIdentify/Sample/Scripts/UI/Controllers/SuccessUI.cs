using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.Sample.UI
{
	public class SuccessUI : BaseUI
	{
		[SerializeField] private Button playButton;

		private void OnEnable()
		{
			playButton.onClick.AddListener(delegate { OnPlayButtonClick(); });
		}

		private void OnDisable()
		{
			playButton.onClick.RemoveListener(delegate { OnPlayButtonClick(); });
		}

		private void OnPlayButtonClick()
		{
			KiDManager.Instance.UIManager.OnPlayButtonClick();
		}
	}
}