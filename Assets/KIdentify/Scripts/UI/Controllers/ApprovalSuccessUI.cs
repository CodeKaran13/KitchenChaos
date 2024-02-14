using KIdentify.Example;
using KIdentify.Services;
using Kidentify.Scripts.Tools;
using System.Collections.Generic;
using UnityEngine;
using KIdentify.Models;

namespace KIdentify.UI {
	public class ApprovalSuccessUI : BaseUI {
		[SerializeField] private GameObject permissionTemplatePrefab;
		[SerializeField] private Transform permissionContentTransform;
		[SerializeField] private RectTransform contentRectTransform;

		private PermissionsManager permissionsManager;

		private void OnEnable() {
			KiDManager.OnPermissionsChanged += KiDManager_OnPermissionsChanged;
		}

		protected override void Start() {
			base.Start();
			permissionsManager = ServiceLocator.Current.Get<PermissionsManager>();
		}

		private void OnDisable() {
			KiDManager.OnPermissionsChanged -= KiDManager_OnPermissionsChanged;
		}

		private void KiDManager_OnPermissionsChanged(List<Permission> permissions) {
			ShowPermissions(permissions);
			ShowUI();
		}

		#region BUTTON ONCLICK

		public void OnLetsPlayButtonClick() {
			uiManager.OnPlayButtonClick();
		}

		public void OnAuthorizeMeButtonClick() {
			KiDManager.Instance.AgeGateCheck();
		}

		#endregion

		private void ShowPermissions(List<Permission> permissions) {
			foreach (Permission permission in permissions) {
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