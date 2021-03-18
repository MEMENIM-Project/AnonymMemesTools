using System;

namespace AnonymDump.Settings
{
    public static class SettingsManager
    {
        private static readonly object AppSettingsSyncRoot = new object();
        private static volatile AppSettings _appSettings;
        public static AppSettings AppSettings
        {
            get
            {
                if (_appSettings == null)
                {
                    lock (AppSettingsSyncRoot)
                    {
                        if (_appSettings == null)
                            _appSettings = new AppSettings();
                    }
                }

                return _appSettings;
            }
        }
    }
}
