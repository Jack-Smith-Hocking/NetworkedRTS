using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    [Tooltip("Will stop/allow errors to be printed from all DebugManager.ErrorMessage calls")] public bool ShowErrors = true;
    [Tooltip("Will stop/allow warnings to be printed from all DebugManager.WarningMessage calls")] public bool ShowWarnings = true;
    [Tooltip("Will stop/allow logs to be printed from all DebugManager.LogMessage calls")] public bool ShowLogs = true;

    private static DebugManager DebugInstance = null;

    private void Awake()
    {
        // Get singleton
        if (DebugInstance == null) DebugInstance = this;
        else if (DebugInstance != null)
        {
            if (!DebugInstance.Equals(this))
            {
                DebugManager.WarningMessage($"DebugManager already has a singleton instance on: {DebugInstance.gameObject.name}");
                Destroy(this);
            }
        }
    }

    /// <summary>
    /// Print a warning message to console, affected by singleton ShowWarnings
    /// </summary>
    /// <param name="message">Warning to print</param>
    public static void WarningMessage(string message)
    {
        if (DebugInstance && !DebugInstance.ShowWarnings) return;

        Debug.LogWarning(message);
    }

    /// <summary>
    /// Print an error message to the console, affected by singleton ShowErrors
    /// </summary>
    /// <param name="message">Error to print</param>
    public static void ErrorMessage(string message)
    {
        if (DebugInstance && !DebugInstance.ShowErrors) return;

        Debug.LogError(message);
    }

    /// <summary>
    /// Print a log message to the console, affected by singleton ShowLogs
    /// </summary>
    /// <param name="message">Log to print</param>
    public static void LogMessage(string message)
    {
        if (DebugInstance && !DebugInstance.ShowLogs) return;

        Debug.Log(message);
    }
}
