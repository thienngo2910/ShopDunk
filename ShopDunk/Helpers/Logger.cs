using System;
using System.IO;
using System.Web;

namespace ShopDunk.Helpers
{
    public static class Logger
    {
        private static readonly object _lock = new object();

        public static void Log(string message)
        {
            try
            {
                string appData = HttpContext.Current?.Server.MapPath("~/App_Data");
                if (string.IsNullOrEmpty(appData))
                {
                    appData = Path.GetTempPath();
                }

                if (!Directory.Exists(appData))
                    Directory.CreateDirectory(appData);

                string file = Path.Combine(appData, "logs.txt");
                string text = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}";
                lock (_lock)
                {
                    File.AppendAllText(file, text);
                }
            }
            catch
            {
                // swallow logging errors
            }
        }
    }
}