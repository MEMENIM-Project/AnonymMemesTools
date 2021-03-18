using System;
using System.IO;
using MySqlConnector;
using RIS.Settings;
using RIS.Settings.Ini;
using Environment = RIS.Environment;

namespace AnonymDump.Settings
{
    public class AppSettings : IniSettings
    {
        private const string SettingsFileName = "AppSettings.config";

        [SettingCategory("Database")]
        public string DatabaseConnectionString { get; set; }
        public ushort DatabaseCommandTimeout { get; set; }
        public byte DatabaseConnectionsNumber { get; set; }
        [SettingCategory("Users")]
        public int UsersOffset { get; set; }
        [SettingCategory("Posts")]
        public int PostsOffset { get; set; }
        [SettingCategory("Comments")]
        public int CommentsCountPerTime { get; set; }
        [SettingCategory("Log")]
        public int LogRetentionDaysPeriod { get; set; }

        public AppSettings()
            : base(Path.Combine(Environment.ExecProcessDirectoryName, SettingsFileName))
        {
            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder(string.Empty)
            {
                ConnectionTimeout = 360,
                Server = "127.0.0.1",
                Database = "anonym_dump_db",
                UserID = "root",
                Password = string.Empty,
                CharacterSet = "utf8mb4"
            };

            DatabaseConnectionString = connectionStringBuilder.ConnectionString;
            DatabaseCommandTimeout = 120;
            DatabaseConnectionsNumber = 20;
            UsersOffset = 1;
            PostsOffset = 1;
            CommentsCountPerTime = 50;
            LogRetentionDaysPeriod = -1;

            Load();
        }
    }
}
