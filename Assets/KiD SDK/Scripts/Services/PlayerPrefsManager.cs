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

	private readonly KiDPlayer currentPlayer;

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

	public void SaveAvatarRender(string tag, Texture2D texture) {
		WriteTextureToPlayerPrefs(tag, texture);
	}

	public Texture2D GetAvatarRender(string tag) {
		return ReadTextureFromPlayerPrefs(tag);
	}

	private void WriteTextureToPlayerPrefs(string tag, Texture2D tex) {
		// if texture is png otherwise you can use tex.EncodeToJPG().
		byte[] texByte = tex.EncodeToPNG();

		// convert byte array to base64 string
		string base64Tex = System.Convert.ToBase64String(texByte);

		// write string to playerpref
		PlayerPrefs.SetString(tag, base64Tex);
		PlayerPrefs.Save();
	}

	private Texture2D ReadTextureFromPlayerPrefs(string tag) {
		// load string from playerpref
		string base64Tex = PlayerPrefs.GetString(tag, null);

		if (!string.IsNullOrEmpty(base64Tex)) {
			// convert it to byte array
			byte[] texByte = System.Convert.FromBase64String(base64Tex);
			Texture2D tex = new(300, 300);

			//load texture from byte array
			if (tex.LoadImage(texByte)) {
				return tex;
			}
		}

		return null;
	}
}
