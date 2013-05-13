namespace WPCloudApp.Phone.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO.IsolatedStorage;
    using System.Net;
    using Microsoft.Phone.Shell;
    using SL.Phone.Federation.Utilities;

    public static class PhoneHelpers
    {
        public static T GetApplicationState<T>(string key)
        {
            if (!PhoneApplicationService.Current.State.ContainsKey(key))
            {
                return default(T);
            }

            return (T)PhoneApplicationService.Current.State[key];
        }

        public static void SetApplicationState(string key, object value)
        {
            if (PhoneApplicationService.Current.State.ContainsKey(key))
            {
                PhoneApplicationService.Current.State.Remove(key);
            }

            PhoneApplicationService.Current.State.Add(key, value);
        }

        public static void RemoveApplicationState(string key)
        {
            if (PhoneApplicationService.Current.State.ContainsKey(key))
            {
                PhoneApplicationService.Current.State.Remove(key);
            }
        }

        public static bool ContainsApplicationState(string key)
        {
            return PhoneApplicationService.Current.State.ContainsKey(key);
        }

        public static T GetIsolatedStorageSetting<T>(string key)
        {
            IDictionary<string, object> isolatedStorage = IsolatedStorageSettings.ApplicationSettings;
            if (!isolatedStorage.ContainsKey(key))
            {
                return default(T);
            }

            return (T)isolatedStorage[key];
        }

        public static void SetIsolatedStorageSetting(string key, object value)
        {
            IDictionary<string, object> isolatedStorage = IsolatedStorageSettings.ApplicationSettings;
            if (isolatedStorage.ContainsKey(key))
            {
                isolatedStorage.Remove(key);
            }

            isolatedStorage.Add(key, value);
        }

        public static void RemoveIsolatedStorageSetting(string key)
        {
            IDictionary<string, object> isolatedStorage = IsolatedStorageSettings.ApplicationSettings;
            if (isolatedStorage.ContainsKey(key))
            {
                isolatedStorage.Remove(key);
            }
        }

        private const long ExpirationBuffer = 10;

        public static bool IsValid(this RequestSecurityTokenResponseStore rstrStore)
        {
            var epoch = (DateTime.UtcNow.Ticks - 621355968000000000) / 10000000;
            if ((rstrStore != null) && rstrStore.ContainsValidRequestSecurityTokenResponse())
            {
                return rstrStore.RequestSecurityTokenResponse.expires > epoch + ExpirationBuffer;
            }

            return false;
        }

        public static System.Net.WebHeaderCollection ParseQueryString(string queryString)
        {
            var res = new WebHeaderCollection();
            int num = (queryString != null) ? queryString.Length : 0;
            for (int i = 0; i < num; i++)
            {
                int startIndex = i;
                int num4 = -1;
                while (i < num)
                {
                    char ch = queryString[i];
                    if (ch == '=')
                    {
                        if (num4 < 0)
                        {
                            num4 = i;
                        }
                    }
                    else if (ch == '&')
                    {
                        break;
                    }

                    i++;
                }

                var str = string.Empty;
                var str2 = string.Empty;
                if (num4 >= 0)
                {
                    str = queryString.Substring(startIndex, num4 - startIndex);
                    str2 = queryString.Substring(num4 + 1, (i - num4) - 1);
                }
                else
                {
                    str2 = queryString.Substring(startIndex, i - startIndex);
                }

                res[str.Replace("?", string.Empty)] = System.Net.HttpUtility.UrlDecode(str2);
            }

            return res;
        }

    }
}