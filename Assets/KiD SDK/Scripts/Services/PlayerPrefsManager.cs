using System.Collections;
using System.Collections.Generic;
using Kidentify.PlayerInfo;
using UnityEngine;

public class PlayerPrefsManager {
	private static PlayerPrefsManager instance;
	public static PlayerPrefsManager Instance {
		get {
			return instance;
		}
	}

	private static readonly string SESSION_KEY = "Session-Id";

	private KiDPlayer currentPlayer;

	public PlayerPrefsManager(KiDPlayer player) {
		instance = this;
		currentPlayer = player;
	}

	public void SaveSession() {
		PlayerPrefs.SetString(SESSION_KEY, currentPlayer.SessionId);
	}

	public string GetSession() {
		return PlayerPrefs.GetString(SESSION_KEY, "");
	}

	public void ClearData() {
		PlayerPrefs.DeleteAll();
	}
}
