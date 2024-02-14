using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

internal static class KidLogger
{
	private const string _unknownClassFormat = "<b>[Unknown Class]</b>";

	[Conditional("UNITY_EDITOR")]
	internal static void Log(string message, Object obj = null)
	{
		LogMessage(LogType.Log, message, obj);
	}
	
	[Conditional("UNITY_EDITOR")]
	internal static void LogWarning(string message, Object obj = null)
	{
		LogMessage(LogType.Warning, message, obj);
	}
	
	internal static void LogError(string message, Object obj = null)
	{
		Debug.LogError(message, obj);
	}

	private static void LogMessage(LogType logType, string message, Object obj = null)
	{
		LogStackInfo stackInfo = GetLogCallerInfo();
		Debug.unityLogger.LogFormat(logType, obj,
			"{0} \nin {1}.{2} at {1}:{3} \n", message, stackInfo.CallingClass, stackInfo.CallingMethod, stackInfo.CallingLine);
	}

	private static LogStackInfo GetLogCallerInfo()
	{
		StackFrame stackFrame = new StackTrace(3, true).GetFrame(0);
		MethodBase callingMethod = stackFrame.GetMethod();
		Type callingClassType = callingMethod.ReflectedType;
	
		int callerLine = stackFrame.GetFileLineNumber();
		string callingMethodName = callingMethod.ToString();
		string callingClassName = callingClassType != null ? callingClassType.ToString(): _unknownClassFormat;
	
		return new LogStackInfo(callerLine, callingMethodName, callingClassName);
	}

	private struct LogStackInfo
	{
		public int CallingLine { get; }
		public string CallingMethod { get; }
		public string CallingClass { get; }

		public LogStackInfo(int callingLine, string callingMethod, string callingClass)
		{
			CallingLine = callingLine;
			CallingMethod = callingMethod;
			CallingClass = callingClass;
		}
	}
}
