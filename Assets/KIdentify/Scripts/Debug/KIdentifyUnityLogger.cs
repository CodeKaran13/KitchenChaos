using System;
using KIdentify.Logger.Interface;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
using UnityEngine;

namespace KIdentify.Logger {
	public class KIdentifyUnityLogger : IKIdentifyLogger {
		public void Log(string message) {
			Debug.Log(message);
		}

		public void LogError(string message) {
			Debug.LogError(message);
		}

		public void LogWarning(string message) {
			Debug.LogWarning(message);
		}
	}
}
#endif
