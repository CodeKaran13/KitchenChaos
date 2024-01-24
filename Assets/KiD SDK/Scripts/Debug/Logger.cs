using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour {
	//#if !UNITY_EDITOR
	private static string myLog = "";

	private string output;
	private string stack;

	//private float power = 0.22f;
	//private float groupXFactor = 0.01f;
	//private float groupYFactor = 0.7f;
	//private float groupWidth = 0.6f;
	//private float groupHeight = 0.3f;


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
		//power -= 20f;
	}

	void OnGUI() {

		//GUI.BeginGroup(new Rect(Screen.width * groupXFactor, Screen.height * groupYFactor, Screen.width * groupWidth, Screen.height * groupHeight));
		//GUI.Label(new Rect(Screen.width * 0f, Screen.height * 0.2f - power, Screen.width * 0.208f, Screen.height * 0.24f), myLog, logStyle);
		//GUI.EndGroup();

		GUI.Label(new Rect(10f, 10f, Screen.width - 10, Screen.height - 10), myLog, logStyle);
	}
	//#endif
}
