using KIdentify.Example;
using UnityEngine;

namespace KIdentify.UI
{
	public class BaseUI : MonoBehaviour
	{
		[Header("- Container -")]
		[SerializeField] private GameObject uiContainer;

		public virtual void ShowUI()
		{
			uiContainer.SetActive(true);
		}

		public virtual void HideUI()
		{
			uiContainer.SetActive(false);
		}
	}
}

