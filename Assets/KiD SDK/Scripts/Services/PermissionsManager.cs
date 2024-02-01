using System;
using System.Collections.Generic;
using Kidentify.Scripts.Interfaces;
using Newtonsoft.Json;
using UnityEngine;

namespace Kidentify.Scripts.Services {
	[CreateAssetMenu(fileName = "PermissionsManager", menuName = "KiD/PermissionsManager", order = 1)]
	internal class PermissionsManager : ScriptableObject, IGameService {

		[SerializeField] private TextAsset en_permissionList;

		private Dictionary<string, PermissionInfo> permissions;

		internal IReadOnlyDictionary<string, PermissionInfo> Permissions => permissions;

		protected void OnEnable() {
			if (en_permissionList != null) {
				var permissionInfos = JsonConvert.DeserializeObject<PermissionCollection>(en_permissionList.text).list;

				permissions = new Dictionary<string, PermissionInfo>(permissionInfos.Count);

				foreach (var permission in permissionInfos) {
					permissions.Add(permission.Name, permission);
				}
			}
			else {
				permissions = null;
			}
		}

		internal string GetPermissionDisplayName(string permissionName) {
			if (permissions.ContainsKey(permissionName)) {
				return permissions[permissionName].DisplayName;
			}

			KidLogger.LogError($"Could not find a {permissionName} in {nameof(permissions)}");
			return null;
		}

		[Serializable]
		private class PermissionCollection {
			public string locale;
			public List<PermissionInfo> list = new();
		}

		[Serializable]
		public class PermissionInfo {
			public string Name;
			public string DisplayName;
			public string GroupName;
			public string GroupDisplayName;
		}
	}
}