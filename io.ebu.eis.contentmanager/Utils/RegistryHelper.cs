/* 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2014 SwissMediaPartners AG, Mathieu Habegger
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * https://gist.github.com/6708638f6dd48b271ddd.git
 */

using System;
using Microsoft.Win32;

namespace io.ebu.eis.contentmanager.Utils
{
    public static class RegistryHelper
    {

        public static string RegPath = @"Software\";

        #region ApplicationGenerics

        public static void SaveValue(string company, string applicationName, string WindowName, string keyString, object value)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(company))
            {
                reg += company + "\\";
            }
            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += applicationName + "\\";

                RegistryKey key = Registry.CurrentUser.CreateSubKey(reg + WindowName);
                key.SetValue(keyString, value.ToString());
            }
        }

        public static string GetString(string company, string applicationName, string windowName, string keyString)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(company))
            {
                reg += company + "\\";
            }
            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += applicationName + "\\";

                var key = Registry.CurrentUser.OpenSubKey(reg + windowName);
                if (key != null)
                {
                    return key.GetValue(keyString).ToString();
                }
            }
            return "";
        }

        public static int GetInt(string company, string applicationName, string windowName, string keyString)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(company))
            {
                reg += company + "\\";
            }
            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += applicationName + "\\";

                var key = Registry.CurrentUser.OpenSubKey(reg + windowName);
                if (key != null)
                {
                    var top = int.Parse(key.GetValue(keyString).ToString());
                    return top;
                }
            }
            return 0;
        }

        public static double GetDouble(string company, string applicationName, string windowName, string keyString)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(company))
            {
                reg += company + "\\";
            }
            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += applicationName + "\\";

                var key = Registry.CurrentUser.OpenSubKey(reg + windowName);
                if (key != null)
                {
                    Double top = Double.Parse(key.GetValue(keyString).ToString());
                    return top;
                }
            }
            return 0.0;
        }

        public static bool GetBoolean(string company, string applicationName, string windowName, string keyString)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(company))
            {
                reg += company + "\\";
            }
            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += applicationName + "\\";

                var key = Registry.CurrentUser.OpenSubKey(reg + windowName);
                if (key != null)
                {
                    bool top = bool.Parse(key.GetValue(keyString).ToString());
                    return top;
                }
            }
            return false;
        }

        #endregion ApplicationGenerics

        #region ApplicationWindowManagement

        public static void SaveStatus(string company, string applicationName, string windowName, double top, double left, double width, double height, bool maximized)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(company))
            {
                reg += company + "\\";
            }
            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += applicationName + "\\";

                RegistryKey key = Registry.CurrentUser.CreateSubKey(reg + windowName);
                key.SetValue("Position_Top", top.ToString());
                key.SetValue("Position_Left", left.ToString());
                key.SetValue("Position_Width", width.ToString());
                key.SetValue("Position_Height", height.ToString());
                key.SetValue("Position_IsMaximized", maximized.ToString());
            }
        }

        public static double GetTop(string Company, string ApplicationName, string WindowName)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(Company))
            {
                reg += Company + "\\";
            }
            if (String.IsNullOrEmpty(ApplicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += ApplicationName + "\\";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(reg + WindowName);
                if (key != null)
                {
                    Double top = Double.Parse(key.GetValue("Position_Top").ToString());

                    return top;
                }
            }
            return 0.0;
        }
        public static double GetLeft(string Company, string ApplicationName, string WindowName)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(Company))
            {
                reg += Company + "\\";
            }
            if (String.IsNullOrEmpty(ApplicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += ApplicationName + "\\";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(reg + WindowName);
                if (key != null)
                {
                    Double top = Double.Parse(key.GetValue("Position_Left").ToString());

                    return top;
                }
            }
            return 0.0;
        }
        public static double GetWidth(string Company, string ApplicationName, string WindowName)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(Company))
            {
                reg += Company + "\\";
            }
            if (String.IsNullOrEmpty(ApplicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += ApplicationName + "\\";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(reg + WindowName);
                if (key != null)
                {
                    Double top = Double.Parse(key.GetValue("Position_Width").ToString());

                    return top;
                }
            }
            return 500.0;
        }
        public static double GetHeight(string Company, string ApplicationName, string WindowName)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(Company))
            {
                reg += Company + "\\";
            }
            if (String.IsNullOrEmpty(ApplicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += ApplicationName + "\\";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(reg + WindowName);
                if (key != null)
                {
                    Double top = Double.Parse(key.GetValue("Position_Height").ToString());

                    return top;
                }
            }
            return 300.0;
        }
        public static bool GetIsMaximized(string Company, string ApplicationName, string WindowName)
        {
            var reg = RegPath;
            if (!String.IsNullOrEmpty(Company))
            {
                reg += Company + "\\";
            }
            if (String.IsNullOrEmpty(ApplicationName))
            {
                throw new ArgumentNullException("ApplicationName cannot be null or left empty.");
            }
            else
            {
                reg += ApplicationName + "\\";

                RegistryKey key = Registry.CurrentUser.OpenSubKey(reg + WindowName);
                if (key != null)
                {
                    bool top = bool.Parse(key.GetValue("Position_IsMaximized").ToString());

                    return top;
                }
            }
            return false;
        }

        #endregion ApplicationWindowManagement
    }
}
