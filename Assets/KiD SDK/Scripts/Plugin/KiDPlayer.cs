using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kidentify.PlayerInfo {

	public enum PlayerStatus {
		Unknown,
		Pending,
		Verified,
		Failed
	}

	public enum Status {
		Active,
		Challenge,
		Prohibited
	}

	[Serializable]
	public class KiDPlayer {
		public string SessionId { get; set; }
		public string Etag { get; set; }
		public string Name { get; set; }
		public DateTime DateOfBirth { get; set; }
		public string CountryCode { get; set; }
		public string RegionCode { get; set; }
		public string Token { get; set; }
		public string ChallengeId { get; set; }
		public bool ChildLiteAccessEnabled { get; set; }

		public PlayerStatus Status { get; set; }

		public ChallengeType ChallengeType { get; set; }
		public List<Permission> Permissions { get; set; }

		public string RefreshToken { get; set; }
		public string AuthToken { get; set; }

		public string KidId { get; set; }
		public string Nickname { get; set; }
		public int AvatarIndex { get; set; }
		public bool IsAdult { get; internal set; }

		public KiDPlayer(string id, string name, DateTime dateOfBirth, string country, string region, List<Permission> permissions) {
			SessionId = id;
			Name = name;
			DateOfBirth = dateOfBirth;
			CountryCode = country;
			RegionCode = region;
			Permissions = permissions;
		}

		public KiDPlayer() {
		}

		public string ToJson() {
			try {
				return JsonUtility.ToJson(this);
			}
			catch (InvalidOperationException e) {
				Debug.LogError(e.Message);
				return null;
			}
			catch (ArgumentException e) {
				Debug.LogError(e.Message);
				return null;
			}
		}

		public static KiDPlayer FromJson(string json) {
			try {
				return JsonUtility.FromJson<KiDPlayer>(json);
			}
			catch (InvalidOperationException e) {
				Debug.LogError(e.Message);
				return null;
			}
			catch (ArgumentException e) {
				Debug.LogError(e.Message);
				return null;
			}
		}

		public override string ToString() {
			return $"Id: {SessionId}, Name: {Name}, DateOfBirth: {DateOfBirth}, Location: {CountryCode} ";
		}

		public bool IsAgeUnder(int years) {
			return DateOfBirth.AddYears(years) > DateTime.Now;
		}

		public bool IsAgeOver(int years) {
			return DateOfBirth.AddYears(years) < DateTime.Now;
		}
	}
}
