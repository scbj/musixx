using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Musixx.Shared
{
    public static class ApplicationSettingsHelper
    {
        public static object ReadResetSettingsValue(string key)
        {
            object value = ReadSettingsValue(key);
            if (value != null)
                ApplicationData.Current.LocalSettings.Values.Remove(key);

            return value;
        }

        public static object ReadSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                return null;

            return ApplicationData.Current.LocalSettings.Values[key];
        }

        public static void SaveSettingsValue(string key, object value)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            else
                ApplicationData.Current.LocalSettings.Values[key] = value;
        }
    }
}
