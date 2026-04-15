using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;


namespace R2InventoryArtifact
{
    internal static class Log
    {
        private static ManualLogSource _logSource;

        internal static void Initialize(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void Debug(object data)     => _logSource.LogDebug(FormatLogMessage(data.ToString()));
        internal static void Error(object data)     => _logSource.LogError(FormatLogMessage(data.ToString()));
        internal static void Fatal(object data)     => _logSource.LogFatal(FormatLogMessage(data.ToString()));
        internal static void Info(object data)      => _logSource.LogInfo(FormatLogMessage(data.ToString()));
        internal static void Message(object data)   => _logSource.LogMessage(FormatLogMessage(data.ToString()));
        internal static void Warning(object data)   => _logSource.LogWarning(FormatLogMessage(data.ToString()));

        private static string FormatLogMessage(string message)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, false);
            System.Diagnostics.StackFrame prevFrame = st.GetFrame(1); 

            List<string> tokens = [$"{System.DateTime.Now:yy-MM-dd HH::mm::ss}"]; 
            
            if(prevFrame != null)
            {
                tokens.Add(prevFrame.GetMethod().DeclaringType.Name); 
                tokens.Add(prevFrame.GetMethod().Name); 
            }

            tokens.Add(message); 

            return string.Join(" | ", tokens); 
        }
    }
}