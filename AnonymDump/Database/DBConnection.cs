using System;
using AnonymDump.Settings;
using RIS.Connection.MySQL;

namespace AnonymDump.Database
{
    public static class DBConnection
    {
        private static MySQLConnection Connection { get; set; }

        public static IRequestEngine RequestEngine { get; set; }

        static DBConnection()
        {
            Connection = new MySQLConnection();

            Connection.Open(
                SettingsManager.AppSettings.DatabaseConnectionsNumber,
                TimeSpan.FromSeconds(SettingsManager.AppSettings.DatabaseCommandTimeout),
                SettingsManager.AppSettings.DatabaseConnectionString);

            RequestEngine = Connection.RequestEngine;
        }

        public static bool IsOpened()
        {
            return Connection.ConnectionComplete;
        }

        public static void Close()
        {
            Connection.Close();
        }
    }
}
