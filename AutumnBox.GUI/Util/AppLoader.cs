﻿/*************************************************
** auth： zsh2401@163.com
** date:  2018/8/19 20:39:02 (UTC +8:00)
** desc： ...
*************************************************/
using AutumnBox.Basic.ManagedAdb;
using AutumnBox.GUI.Properties;
using AutumnBox.GUI.Util.Bus;
using AutumnBox.GUI.Util.Debugging;
using AutumnBox.GUI.Util.Net;
using AutumnBox.GUI.Util.OpenFxManagement;
using AutumnBox.GUI.Util.OS;
using AutumnBox.GUI.View.DialogContent;
using AutumnBox.GUI.View.Windows;
using AutumnBox.OpenFramework;
using MaterialDesignThemes.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutumnBox.GUI.Util
{
    class AppLoader
    {
        public interface ILoadingUI
        {
            string LoadingTip { set; }
            double Progress { set; }
            void Finish();
        }
        private readonly ILoadingUI ui;
        public AppLoader(ILoadingUI ui) : this()
        {
            this.ui = ui;
        }
        public void LoadAsync(Action callback)
        {
            Task.Run(() =>
            {
                Load();
                callback?.Invoke();
            });
        }

#if PREVIEW || DEBUG
        private const bool isPreviewOrDebug = true;
#else
        private const bool isPreviewOrDebug = false;
#endif

        private readonly ILogger logger;
        public AppLoader()
        {
            logger = new Logger<AppLoader>();
        }
        #region LOADING_FLOW
        private void Load()
        {
            LoggingStation.Instance.Work();
            ui.Progress = 0;
            if (isPreviewOrDebug || Settings.Default.IsFirstLaunch)
            {
                Task langDialogTask = null;
                App.Current.Dispatcher.Invoke(() =>
                {
                    langDialogTask = (App.Current.MainWindow as MainWindow).DialogHost.ShowDialog(new LanguageChoice());
                });
                langDialogTask.Wait();
            }
            if (isPreviewOrDebug || !Settings.Default.LicenseAccepted)
            {
                Task<object> dialogTask = null;
                App.Current.Dispatcher.Invoke(() =>
                {
                    dialogTask = (App.Current.MainWindow as MainWindow).DialogHost.ShowDialog(new ContentLicense());
                });
                dialogTask.Wait();
                bool accepted = dialogTask.Result != null;
                if (!accepted)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        App.Current.Shutdown(0);
                    });
                    return;
                }
                else
                {
                    Settings.Default.LicenseAccepted = true;
                }
            }
            //如果设置在启动时打开调试窗口
            if (Settings.Default.ShowDebuggingWindowNextLaunch)
            {
                //打开调试窗口
                App.Current.Dispatcher.Invoke(() =>
                {
                    new LogWindow().Show();
                });
            }
            logger.Info("");
            logger.Info("======================");
            logger.Info($"Run as " + (Self.HaveAdminPermission ? "Admin" : "Normal user"));
            logger.Info($"AutumnBox version: {Self.Version}");
            logger.Info($"SDK version: {BuildInfo.SDK_VERSION}");
            logger.Info($"Windows version {Environment.OSVersion.Version}");
            logger.Info("======================");
            Basic.Util.Debugging.LoggingStation.Logging += (s, e) =>
            {
#if !DEBUG
                if (e.Tag.ToLower() == "debug") return;
#endif
                LoggingStation.Instance.Log(e.Tag, e.Level.ToString(), e.Text);
            };
            Basic.Util.Settings.CreateNewWindow = Settings.Default.DisplayCmdWindow;
            ui.Progress = 30;
            ui.LoadingTip = App.Current.Resources["ldmsgStartAdb"].ToString();
            try
            {
                TaskKill.Kill("adb.exe");
                logger.Info("trying starts adb server");
                Adb.Load(new AdbManager());
                Adb.Server.Start();
                logger.Info($"adb server started at {Adb.Server.IP}:{Adb.Server.Port}");
            }
            catch (Exception e)
            {
                logger.Warn("can not start adb server!", e);
                App.Current.Dispatcher.Invoke(() =>
                {
                    new AdbFailedWindow(e.Message)
                    {
                        Owner = App.Current.MainWindow
                    }.ShowDialog();
                });
                App.Current.Shutdown(1);
            }

            ui.Progress = 60;
            ui.LoadingTip = App.Current.Resources["ldmsgLoadingExtensions"].ToString();
            OpenFrameworkManager.Init();
            OpenFxObserver.Instance.OnLoaded();
            ConnectedDevicesListener.Instance.Work();

            ui.Progress = 90;
            ui.LoadingTip = "How can a man die better?";
            Updater.RefreshAsync(() =>
            {
                Updater.ShowUI(false);
            });
            Statistics.Do();
            ToastMotd.Do();

            ui.Progress = 100;
            ui.LoadingTip = "Enjoy!";
            Thread.Sleep(1 * 1000);
            ui.Finish();
        }
        #endregion
    }
}
