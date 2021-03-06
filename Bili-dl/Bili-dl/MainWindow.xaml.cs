﻿using Bili;
using BiliDownload;
using BiliLogin;
using Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bili_dl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window

        private void This_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;
            WindowLong.SetWindowLong(windowHandle, WindowLong.GWL_STYLE, (WindowLong.GetWindowLong(windowHandle, WindowLong.GWL_STYLE) | WindowLong.WS_CAPTION));

            LoadConfig();
        }

        private async void LoadConfig()
        {
            ConfigManager.ConfigManager.Init();

            if (!ConfigManager.ConfigManager.GetStatementConfirmed())
                StatementGrid.Visibility = Visibility.Visible;

            BiliApi.CookieCollection = ConfigManager.ConfigManager.GetCookieCollection();
            SettingsBox.SetSettings(ConfigManager.ConfigManager.GetSettings());

            List<DownloadInfo> infos = ConfigManager.ConfigManager.GetDownloadInfos();
            foreach (DownloadInfo info in infos)
                DownloadQueuePanel.Append(new DownloadTask(info));

            if (BiliApi.CookieCollection != null)
            {
                UserInfo userInfo = await UserInfo.GetUserInfoAsync(BiliApi.CookieCollection);
                if (userInfo != null)
                {
                    ShowUserInfo(userInfo);
                    LoginBtn.Content = "登出";
                }
            }    
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.WndProc));
            }
        }

        protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case HitTest.WM_NCHITTEST:
                    handled = true;
                    return HitTest.Hit(lParam, this.Top, this.Left, this.ActualHeight, this.ActualWidth);
            }
            return IntPtr.Zero;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.ResizeMode = ResizeMode.NoResize;
            this.DragMove();
            this.ResizeMode = ResizeMode.CanResize;
        }

        #endregion

        #region Login

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if(LoginBtn.Content.ToString() == "登录")
            {
                MoblieLoginWindow moblieLoginWindow = new MoblieLoginWindow(this);
                moblieLoginWindow.LoggedIn += MoblieLoginWindow_LoggedIn;
                moblieLoginWindow.Canceled += MoblieLoginWindow_Canceled;
                moblieLoginWindow.Show();
                LoginBtn.Content = "登录中...";
            }
            else if(LoginBtn.Content.ToString() == "登出")
            {
                BiliApi.CookieCollection = null;
                ConfigManager.ConfigManager.SetCookieCollection(null);
                UserInfoBox.Text = string.Empty;
                UserFaceImage.Source = null;
                LoginBtn.Content = "登录";
            }
            
        }

        private void MoblieLoginWindow_Canceled(MoblieLoginWindow sender)
        {
            LoginBtn.Content = "登录";
        }

        private void MoblieLoginWindow_LoggedIn(MoblieLoginWindow sender, System.Net.CookieCollection cookies, uint uid)
        {
            Dispatcher.Invoke(new Action(async () =>
            {
                sender.Topmost = false;
                sender.Hide();

                BiliApi.CookieCollection = cookies;
                ConfigManager.ConfigManager.SetCookieCollection(cookies);

                UserInfo userInfo = await UserInfo.GetUserInfoAsync(BiliApi.CookieCollection);

                if(userInfo != null)
                {
                    ShowUserInfo(userInfo);
                    LoginBtn.Content = "登出";
                }
                sender.Close();
                
            }));
        }

        private async void ShowUserInfo(UserInfo userInfo)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(userInfo.Uname);
            stringBuilder.Append(string.Format(" [Lv.{0}]", userInfo.CurrentLevel));
            if (userInfo.VipStatus > 0)
                stringBuilder.Append(" [大会员]");
            UserInfoBox.Text = stringBuilder.ToString();

            UserFaceImage.Source = BiliApi.BitmapToImageSource(await userInfo.GetFaceBitmapAsync());
        }

        #endregion

        #region Search

        private async void SearchBox_Search(BiliSearch.SearchBox sender, string text)
        {
            await ResultBox.SearchAsync(text);
        }

        #endregion

        #region DownloadOption

        private void ResultBox_VideoSelected(string title, long id)
        {
            DownloadOptionPanel.ShowParts(title, (uint)id, false);
            DownloadOptionPanel.Visibility = Visibility.Visible;
        }

        private void ResultBox_SeasonSelected(string title, long id)
        {
            DownloadOptionPanel.ShowParts(title, (uint)id, true);
            DownloadOptionPanel.Visibility = Visibility.Visible;
        }

        private void DownloadOptionGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DownloadOptionPanel.Visibility = Visibility.Hidden;
        }

        private void DownloadOptionPanel_TaskCreated(BiliDownload.DownloadTask downloadTask)
        {
            if (DownloadQueuePanel.Append(downloadTask))
            {
                Prompt.Text = "已添加到下载队列";
                ((System.Windows.Media.Animation.Storyboard)Resources["ShowPrompt"]).Begin();
            }
            else
            {
                Prompt.Text = "已存在于下载队列";
                ((System.Windows.Media.Animation.Storyboard)Resources["ShowPrompt"]).Begin();
            }
        }

        #endregion

        #region DownloadQueue

        private void ShowQueueBtn_Click(object sender, RoutedEventArgs e)
        {
            DownloadQueuePanel.Visibility = Visibility.Visible;
        }

        private void DownloadQueueGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DownloadQueuePanel.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Settings

        private void ShowSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsBox.Visibility = Visibility.Visible;
        }

        private void SettingsGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SettingsBox.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Closing

        private void This_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DownloadQueuePanel.StopAll();
            SettingPanel.Settings settings = ConfigManager.ConfigManager.GetSettings();
            if (settings.MovedTempPath != null && settings.MovedTempPath != settings.TempPath)
            {
                CopyDirectory(settings.TempPath, settings.MovedTempPath);
                settings.TempPath = settings.MovedTempPath;
                settings.MovedTempPath = null;
                ConfigManager.ConfigManager.SetSettings(settings);
            }
        }

        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(sourcePath);
            System.IO.Directory.CreateDirectory(destinationPath);
            foreach (System.IO.FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = System.IO.Path.Combine(destinationPath, fsi.Name);

                if (fsi is System.IO.FileInfo)
                {
                    System.IO.File.Copy(fsi.FullName, destName, true);
                    System.IO.File.Delete(fsi.FullName);
                } 
                else
                {
                    System.IO.Directory.CreateDirectory(destName);
                    CopyDirectory(fsi.FullName, destName);
                }
            }
            System.IO.Directory.Delete(sourcePath);
        }

        #endregion

        #region Statement

        private void StatementConfirmChk_Checked(object sender, RoutedEventArgs e)
        {
            ConfigManager.ConfigManager.ConfirmStatement();
            StatementGrid.Visibility = Visibility.Hidden;
        }

        #endregion

    }
}
