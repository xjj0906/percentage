using System;
using System.Reflection;
using System.Windows.Forms;

namespace percentage
{
    [Serializable]
    public class SystemConfig
    {
        // Don't use Application.ExecutablePath
        // see https://stackoverflow.com/questions/12945805/odd-c-sharp-path-issue
        private static readonly string ExecutablePath = Assembly.GetEntryAssembly()?.Location;

        private static string _mainKey = "percentage_";
        private static string _instanceKey = _mainKey + Application.StartupPath.GetHashCode();

        #region 自动启动
        /// <summary>
        /// 设置开机启动项
        /// </summary>
        /// <param name="started">是否启动</param>
        public static void SetAutoStart(bool started)
        {
            Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey runKey = hkcu.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            if (runKey == null) return;

            if (started)
            {
                try
                {
                    var names = runKey.GetValueNames();
                    foreach (var name in names)
                    {
                        if (name.StartsWith(_mainKey))
                            runKey.DeleteValue(name);
                    }

                    runKey.SetValue(_instanceKey, ExecutablePath);
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    hkcu.Close();
                }
            }
            else
            {
                try
                {
                    runKey.DeleteValue(_instanceKey);
                    hkcu.Close();
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    hkcu.Close();
                }
            }
        }

        /// <summary>
        /// 检查开机启动项是否有效
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsAutoStart()
        {
            Microsoft.Win32.RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey runKey = hkcu.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            if (runKey == null) return false;

            try
            {
                string[] runList = runKey.GetValueNames();
                foreach (string item in runList)
                {
                    if (item.Equals(_instanceKey, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            finally
            {
                hkcu.Close();
            }
        }
        #endregion
    }
}