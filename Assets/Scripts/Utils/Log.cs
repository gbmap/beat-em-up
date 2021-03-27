
using System;
using Catacumba.LevelGen;
using UnityEngine;

[Flags]
public enum ELogSystemBitmask
{
    None    = 0,
    Combat  = 1 << 1,
    AI      = 1 << 2,
    Skills  = 1 << 3,
    AssetProcessor = 1 << 4,
    Scripts = 1 << 5,
    
    All = Combat | AI | Skills | AssetProcessor | Scripts
}

public enum ELogPriorityBitmask
{
    None     = 0,
    Messages = 1 << 1,
    Warnings = 1 << 2,
    Errors   = 1 << 3,
    All = Messages | Warnings | Errors
}

public interface ILoggingMethod
{
    void Log(string msg);
    void LogWarning(string msg);
    void LogError(string msg);
}

public class UnityConsoleLoggingMethod : ILoggingMethod
{
    public void Log(string msg)
    {
        Debug.Log(msg);
    }

    public void LogWarning(string msg)
    {
        Debug.LogWarning(msg);
    }

    public void LogError(string msg)
    {
        Debug.LogError(msg);
    }
}

public static class Log
{
    public static ELogSystemBitmask Systems = ELogSystemBitmask.All;
    public static ELogPriorityBitmask Priority = ELogPriorityBitmask.All;
    public static ILoggingMethod Logger = new UnityConsoleLoggingMethod();

    public static void Message(ELogSystemBitmask system, string message)
    {
        AttemptLog(system, ELogPriorityBitmask.Messages, message, Logger.Log);
        // if (!ShouldLog(system, ELogPriorityBitmask.Messages)) return;
        // Logger.Log($"[{system.ToString()}] {message}");
    }

    public static void Warning(ELogSystemBitmask system, string message)
    {
        AttemptLog(system, ELogPriorityBitmask.Warnings, message, Logger.LogWarning);
        //if (!ShouldLog(system, ELogPriorityBitmask.Warnings)) return;
        //Logger.LogWarning($"[{system.ToString()}] {message}");
    }

    public static void Error(ELogSystemBitmask system, string message)
    {
        AttemptLog(system, ELogPriorityBitmask.Errors, message, Logger.LogError);
        // if (!ShouldLog(system, ELogPriorityBitmask.Errors)) return;
        // Logger.LogError($"[{system.ToString()}] {message}");
    }

    private static bool ShouldLog(ELogSystemBitmask system, ELogPriorityBitmask priority)
    {
        return BitmaskHelper.IsSet(Systems, system)
            && BitmaskHelper.IsSet(Priority, ELogPriorityBitmask.Messages);
    }

    private static void AttemptLog(ELogSystemBitmask system,
                                   ELogPriorityBitmask priority,
                                   string msg,
                                   System.Action<string> LoggingFunc
    )  {
        if (Logger == null) Logger = new UnityConsoleLoggingMethod();
        if (!ShouldLog(system, priority)) return;
        LoggingFunc(msg);
    }
}