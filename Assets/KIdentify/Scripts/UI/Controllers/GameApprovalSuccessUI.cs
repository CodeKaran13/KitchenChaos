using System.Collections;
using System.Collections.Generic;
using Kidentify.Scripts.Tools;
using KIdentify.Example;
using KIdentify.Models;
using KIdentify.PlayerInfo;
using KIdentify.Services;
using UnityEngine;

namespace KIdentify.UI {
	public class GameApprovalSuccessUI : BaseUI {
		[SerializeField] protected GameObject permissionTemplatePrefab;
		[SerializeField] protected Transform permissionContentTransform;
		[SerializeField] protected RectTransform contentRectTransform;

		protected PermissionsManager permissionsManager;
		protected KiDPlayer currentPlayer;

		protected override void Start() {
			base.Start();
			permissionsManager = ServiceLocator.Current.Get<PermissionsManager>();
			currentPlayer = KiDManager.Instance.CurrentPlayer;
		}

		public override void ShowUI() {
			ShowPermissions();
			base.ShowUI();
		}

		#region BUTTON ONCLICK

		public void OnLetsPlayButtonClick() {
			uiManager.OnPlayButtonClick();
		}

		#endregion

		private void ShowPermissions() {
			foreach (Permission permission in currentPlayer.Permissions) {
				PermissionTemplateUI permissionTemplateUI = GameObject.Instantiate(permissionTemplatePrefab, permissionContentTransform).GetComponent<PermissionTemplateUI>();
				string permissionDisplayName = permissionsManager.GetPermissionDisplayName(permission.name);
				if (!string.IsNullOrEmpty(permissionDisplayName)) {
					permissionTemplateUI.ShowPermission(permissionDisplayName, permission.enabled);
				}
				else {
					permissionTemplateUI.ShowPermission(permission.name, permission.enabled);
				}
			}

		}
	}
}