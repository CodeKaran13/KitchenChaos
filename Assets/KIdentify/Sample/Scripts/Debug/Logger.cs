using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour {
#if !UNITY_EDITOR
	private static string myLog = "";

	private string output;
	private string stack;

	GUIStyle logStyle;

	void OnEnable() {
		Application.logMessageReceived += Log;
	}

	private void Start() {
		logStyle = new GUIStyle {
			fontSize = 20,
			alignment = TextAnchor.UpperLeft
		};
	}

	void OnDisable() {
		Application.logMessageReceived -= Log;
	}

	public void Log(string logString, string stackTrace, LogType type) {
		output = logString;
		stack = stackTrace;
		myLog = output + "\n" + myLog;
		if (myLog.Length > 5000) {
			myLog = myLog[..4000];
		}
	}

	void OnGUI() {
		GUI.Label(new Rect(10f, 10f, Screen.width - 10, Screen.height - 10), myLog, logStyle);
	}
#endif
}
