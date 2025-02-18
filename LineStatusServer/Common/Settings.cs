using LineStatusServer.DTOs;
using LineStatusServer.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Runtime;

namespace LineStatusServer.Common
{
    internal class Settings
    {
        public static string connectionString = "";
        public static IPEndPoint TCPAddress = null;

        public static (string, string, string, string, string) ReadSQLConnectionString()
        {
            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            if (File.Exists(settingsFilePath))
            {
                string jsonString = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<UserSettings>(jsonString);
                if (settings != null)
                {
                    connectionString = $"Server={settings.ServerName};Database={settings.DatabaseName};User Id={settings.UserName};Password={settings.Password};{settings.Extra}";
                    return (settings.ServerName, settings.DatabaseName, settings.UserName, settings.Password, settings.Extra);
                }
                else
                {
                    throw new InvalidOperationException("Deserialized settings object is null");
                }
            }
            else
            {
                throw new FileNotFoundException("DB setting is not found");
            }
        }

        public static (string, string) ReadTCPAddress()
        {
            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TCP.json");
            if (File.Exists(settingsFilePath))
            {
                string jsonString = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<TCP_Address>(jsonString);
                if (settings != null)
                {
                    TCPAddress = new IPEndPoint(IPAddress.Parse(settings.IPAddress), settings.Port);
                    return (settings.IPAddress, Convert.ToString(settings.Port));
                }
                else
                {
                    throw new InvalidOperationException("Deserialized settings object is null");
                }
            }
            else
            {
                throw new FileNotFoundException("TCP setting is not found");
            }
        }

        public static void WriteSQLConnectionString(string ServerName, string DBName, string UserName, string Password, string Extra)
        {
            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            string json = File.ReadAllText(settingsFilePath);
            JObject jsonObj = JObject.Parse(json);
            jsonObj["ServerName"] = ServerName;
            jsonObj["DatabaseName"] = DBName;
            jsonObj["UserName"] = UserName;
            jsonObj["Password"] = Password;
            jsonObj["Extra"] = Extra;
            File.WriteAllText(settingsFilePath, jsonObj.ToString());
            connectionString = $"Server={ServerName};Database={DBName};User Id={UserName};Password={Password};{Extra}";
        }

        public static void WriteTCPAddress(string IP, string Port)
        {
            string settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TCP.json");
            string json = File.ReadAllText(settingsFilePath);
            JObject jsonObj = JObject.Parse(json);
            jsonObj["IPAddress"] = IP;
            jsonObj["Port"] = Port;
            File.WriteAllText(settingsFilePath, jsonObj.ToString());
            TCPAddress = new IPEndPoint(IPAddress.Parse(IP), Convert.ToInt32(Port));
        }
    }
}