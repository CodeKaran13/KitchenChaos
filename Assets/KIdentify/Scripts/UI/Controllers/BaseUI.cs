using KIdentify.Example;
using UnityEngine;

namespace KIdentify.UI {
	public class BaseUI : MonoBehaviour {
		[Header("- Container -")]
		[SerializeField] private GameObject uiContainer;

		protected KidUIManager uiManager;

		protected virtual void Start() {
			uiManager = KiDManager.Instance.uiManager;
		}

		public virtual void ShowUI() {
			uiContainer.SetActive(true);
		}

		public virtual void HideUI() {
			uiContainer.SetActive(false);
		}
	}
}

