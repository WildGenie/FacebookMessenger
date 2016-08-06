﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CefSharp;
using Newtonsoft.Json;
using Application = System.Windows.Application;

namespace FacebookMessenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NotifyIcon TrayIcon { get; set; }
        ISettings Settings = new AppSettings();
        public static string AppDataFolderName { get; set; } = "dp-facebook-messenger";
        public static string AppDataPath { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppDataFolderName);


        protected override void OnStartup(StartupEventArgs e)
        {
            

            Settings = CreateConfigIfNotExists(new AppSettings());

            CefInit();

            MainWindow = new MainWindow(Settings);

            if (Settings.EnableTrayIcon) EnableTrayIcon();

            MainWindow.MinWidth = 500;

            if (!Settings.StartMinimized) MainWindow.Show();
            
            base.OnStartup(e);

        }



        void EnableTrayIcon()
        {
            TrayIcon = Tray.CreateNotifyIcon(this, new[] { new MenuItem("Exit", delegate { Shutdown(); }) }, Settings);
            TrayIcon.Visible = true;
        }

        ISettings CreateConfigIfNotExists(ISettings config)
        {
            

            Directory.CreateDirectory(AppDataPath);
            var file = new FileInfo(Path.Combine(AppDataPath, "config.json"));

            if (file.Exists) return DeserializeSettings();

            var stream = File.Create(file.FullName);
            stream.Close();
            stream.Dispose();

            File.WriteAllText(file.FullName, JsonConvert.SerializeObject(config, Formatting.Indented));
            var settings = DeserializeSettings();
            return settings;
        }

        private static ISettings DeserializeSettings()
        {
            
            var json = File.ReadAllText(Path.Combine(AppDataPath, "config.json"));
            var settings = JsonConvert.DeserializeObject<AppSettings>(json);
            return settings;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TrayIcon?.Icon?.Dispose();
            TrayIcon?.Dispose();
            base.OnExit(e);
        }

        protected static void CefInit()
        {
            var settings = new CefSettings
            {
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dp-facebook-messenger", "CEF_Cache"),
                PersistSessionCookies = true,
                PersistUserPreferences = true,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.82 Safari/537.36"

            };

            settings.SetOffScreenRenderingBestPerformanceArgs();

            //BUG: disable frme scheduling to fix blank frame bugs in cefsharp 
            settings.CefCommandLineArgs.Remove("enable-begin-frame-scheduling");
            Cef.Initialize(settings);
        }


    }
}
