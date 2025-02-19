﻿using System;
using System.IO;

namespace LineStatusServer.Common
{
    public class ErrorLogger
    {
        public static void Write(Exception exception)
        {
            DateTime now = DateTime.Now;

            string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            string monthDirectory = Path.Combine(logsDirectory, now.ToString("yyyy-MM"));

            Directory.CreateDirectory(monthDirectory);

            string logFilePath = Path.Combine(monthDirectory, now.ToString("dd") + ".txt");

            string logMessage = $"[{now}] Error: {exception.Message}\n{exception.StackTrace}\n\n";

            File.AppendAllText(logFilePath, logMessage);
        }

        public static void SaveLog(string title ,string t_error)
        {
            try
            {
                DateTime now = DateTime.Now;

                string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveLogs");
                string monthDirectory = Path.Combine(logsDirectory, now.ToString("yyyy-MM"));

                Directory.CreateDirectory(monthDirectory);

                string logFilePath = Path.Combine(monthDirectory, now.ToString("dd") + ".txt");

                string logMessage = $"[{now}] {title}: {t_error}\n\n";

                File.AppendAllText(logFilePath, logMessage);
            }
            catch (Exception)
            {
            }
        }
    }
}