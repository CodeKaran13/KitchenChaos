using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApprovalSuccessUI : BaseUI {
	[SerializeField] private GameObject approvalSuccessContainer;
	[SerializeField] private GameObject approvalPendingContainer;
	[SerializeField] private GameObject permissionTemplatePrefab;
	[SerializeField] private Transform permissionContentTransform;

	private void OnEnable() {
		KiDManager.OnPermissionsChanged += KiDManager_OnPermissionsChanged;
	}

	private void OnDisable() {
		KiDManager.OnPermissionsChanged -= KiDManager_OnPermissionsChanged;
	}

	private void KiDManager_OnPermissionsChanged(List<Permission> permissions) {
		ShowPermissions(permissions);
		uiManager.ShowApprovalSuccessUI(true);
	}

	public override void ShowUI() {
		base.ShowUI();
		if (uiManager.ApprovalSuccess) {
			approvalPendingContainer.SetActive(false);
			approvalSuccessContainer.SetActive(true);
		}
		else {
			approvalSuccessContainer.SetActive(false);
			approvalPendingContainer.SetActive(true);
		}
	}

	public override void HideUI() {
		base.HideUI();
		approvalPendingContainer.SetActive(false);
		approvalSuccessContainer.SetActive(false);
	}

	#region BUTTON ONCLICK

	public void OnLetsPlayButtonClick() {
		uiManager.OnPlayButtonClick();
	}

	public void OnAuthorizeMeButtonClick() {
		uiManager.ShowQR();
	}

	#endregion

	private void ShowPermissions(List<Permission> permissions) {
		foreach (Permission permission in permissions) {
			PermissionTemplateUI permissionTemplateUI = GameObject.Instantiate(permissionTemplatePrefab, permissionContentTransform).GetComponent<PermissionTemplateUI>();
			permissionTemplateUI.ShowPermission(permission.name, permission.enabled);
		}
	}
}
