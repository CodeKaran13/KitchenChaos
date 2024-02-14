using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KIdentify.UI {
	public class PermissionTemplateUI : MonoBehaviour {

		[SerializeField] private TextMeshProUGUI permissionNameText;
		[SerializeField] private Image permissionImage;

		[SerializeField] private Sprite enableSprite;
		[SerializeField] private Sprite disableSprite;

		public void ShowPermission(string name, bool isEnable) {
			permissionNameText.text = name;
			if (isEnable) {
				permissionImage.sprite = enableSprite;
			}
			else {
				permissionImage.sprite = disableSprite;
			}
		}
	}
}