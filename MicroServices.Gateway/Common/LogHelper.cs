using System;
using System.Configuration;

namespace MicroServices.Gateway.Common
{
    public static class LogHelper
    {
        /// <summary>
        /// 运行日志
        /// </summary>
        /// <param name="title">日志消息标题，字数不超过300</param>
        /// <param name="message">日志详细内容，字数建议不超过3000</param>
        public static void Info(string title, string message)
        {
            //Log.LogText(title, message, string.Empty, isDurable);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="title">预警消息标题，字数不超过300</param>
        /// <param name="message">预警详细内容，字数建议不超过3000</param>
        /// <param name="ex">具体错误</param>
        public static void Error(string title, string message, Exception ex = null)
        {
            string alarmText = ex == null ? message : string.Join("------", message, ex.ToString());

           // EarlyWarn.AlarmLog(title, alarmText);
        }
    }
}