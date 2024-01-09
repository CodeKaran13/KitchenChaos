using System;
using System.Threading;
using UnityEngine;

public static class Slug {
	public delegate void Logger(string message);

	public static Logger Log = m => LogMessage(LogType.Log, m);
	public static Logger LogWarning = m => LogMessage(LogType.Warning, m);
	public static Logger LogError = m => LogMessage(LogType.Error, m);

    public delegate void LoggerT(UnityEngine.Object obj, string message);
    public static LoggerT LogT = (UnityEngine.Object obj, string m) => LogMessageT(obj, LogType.Log, m);

    public delegate void LoggerT2(object obj, string message);
    public static LoggerT2 LogT2 = (obj, m) => LogMessageT2(obj, LogType.Log, m);

    private static void LogMessageT2(object obj, LogType logType, string message)
    {
		string msg = String.Format("{0} {1} {2} {3} {4}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss.fff"), obj.GetType().Name, GetClassName(), message);

		Debug.unityLogger.Log(logType, msg);
    }

    private static void LogMessageT(UnityEngine.Object obj, LogType logType, string message)
    {
		string msg = String.Format("{0} {1} {2} {3} {4}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss"), obj.GetType().Name, GetClassName(), message);

		Debug.unityLogger.Log(logType, msg);
    }

    private static void LogMessage(LogType logType, string message) {
		string msg = String.Format("{0} {1} {2} {3}", Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToString("HH:mm:ss"), GetClassName(), message);
		Debug.unityLogger.Log(logType, msg);
	}

	private static string GetClassName()
	{
		Type m_ReflectedType = new System.Diagnostics.StackTrace(true).GetFrame(3).GetMethod().ReflectedType;
		return m_ReflectedType != null ? "[" + m_ReflectedType.Name + "]": "<b>[Unknown Class]</b>";
	}
}