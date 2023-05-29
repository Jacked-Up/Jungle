using UnityEngine;

namespace Jungle.Editor
{
    /// <summary>
    /// Class for debugging using a custom log format.
    /// </summary>
    public static class JungleDebug
    {
        #region Variables

        private struct LogData
        {
            public string From;
            public string Message;
            public Object Context;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void Log(string from, string message, Object context = null)
        {
            if (string.IsNullOrEmpty(from))
            {
                from = "Jungle";
            }
            var data = new LogData
            {
                From = from,
                Message = message,
                Context = context
            };
            CreateLogWithFormat(data, LogType.Log);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void Warn(string from, string message, Object context = null)
        {
            if (string.IsNullOrEmpty(from))
            {
                from = "Jungle";
            }
            var data = new LogData
            {
                From = from,
                Message = message,
                Context = context
            };
            CreateLogWithFormat(data, LogType.Warning);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        public static void Error(string from, string message, Object context = null)
        {
            if (string.IsNullOrEmpty(from))
            {
                from = "Jungle";
            }
            var data = new LogData
            {
                From = from,
                Message = message,
                Context = context
            };
            CreateLogWithFormat(data, LogType.Error);
        }

        private static void CreateLogWithFormat(LogData data, LogType type)
        {
            Debug.LogFormat(type, LogOption.NoStacktrace, data.Context,
                $"[{data.From}] {data.Message}");
        }
    }
}
