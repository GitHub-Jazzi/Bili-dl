﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bili_dl
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPanel : UserControl
    {
        [Serializable]
        public class Settings
        {
            public string DownloadPath;
            public string TempPath;
            public string MovedTempPath;
            public int RetryInterval;
            public int DownloadThreads;

            public Settings()
            {
                DownloadPath = AppDomain.CurrentDomain.BaseDirectory + "Download";
                TempPath = AppDomain.CurrentDomain.BaseDirectory + "Temp";
                RetryInterval = 5;
                DownloadThreads = 5;
            }
        }
        
        public static Settings settings;

        public SettingPanel()
        {
            InitializeComponent();
        }

        public void SetSettings(Settings settings)
        {
            SettingPanel.settings = settings;

            DownloadPathBox.Text = settings.DownloadPath;
            if (settings.MovedTempPath != null)
                TempPathBox.Text = settings.MovedTempPath;
            else
                TempPathBox.Text = settings.TempPath;
            RetryIntervalBox.Text = settings.RetryInterval.ToString();
            DownloadThreadsBox.Text = settings.DownloadThreads.ToString();
        }

        private void SelectDownloadPathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = settings.DownloadPath;
            folderBrowserDialog.Description = "请选择下载目录";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.DownloadPath = folderBrowserDialog.SelectedPath.ToString();
                DownloadPathBox.Text = folderBrowserDialog.SelectedPath.ToString();
                ConfigManager.ConfigManager.SetSettings(settings);
            }
        }

        private void SelectTempPathBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = settings.TempPath;
            folderBrowserDialog.Description = "请选择一个空白文件夹作为临时文件目录";
            while (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (settings.MovedTempPath != null && folderBrowserDialog.SelectedPath == settings.MovedTempPath)
                    break;
                if (System.IO.Directory.Exists(folderBrowserDialog.SelectedPath) && folderBrowserDialog.SelectedPath != settings.TempPath)
                {
                    System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(folderBrowserDialog.SelectedPath);
                    if (directoryInfo.GetDirectories().Length == 0 && directoryInfo.GetFiles().Length == 0)
                    {
                        settings.MovedTempPath = folderBrowserDialog.SelectedPath.ToString();
                        TempPathBox.Text = folderBrowserDialog.SelectedPath.ToString();
                        ConfigManager.ConfigManager.SetSettings(settings);
                        break;
                    }
                    MessageBox.Show("请选择一个空目录");
                }
                else
                {
                    settings.MovedTempPath = folderBrowserDialog.SelectedPath.ToString();
                    TempPathBox.Text = folderBrowserDialog.SelectedPath.ToString();
                    ConfigManager.ConfigManager.SetSettings(settings);
                    break;
                }
            }
        }

        private void ReduceRetryIntervalBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settings.RetryInterval == 3)
                return;
            settings.RetryInterval--;
            RetryIntervalBox.Text = settings.RetryInterval.ToString();
            ConfigManager.ConfigManager.SetSettings(settings);
        }

        private void IncreaseRetryIntervalBtn_Click(object sender, RoutedEventArgs e)
        {
            settings.RetryInterval++;
            RetryIntervalBox.Text = settings.RetryInterval.ToString();
            ConfigManager.ConfigManager.SetSettings(settings);
        }

        private void ReduceDownloadThreadsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settings.DownloadThreads == 1)
                return;
            settings.DownloadThreads--;
            DownloadThreadsBox.Text = settings.DownloadThreads.ToString();
            ConfigManager.ConfigManager.SetSettings(settings);
        }

        private void IncreaseDownloadThreadsBtn_Click(object sender, RoutedEventArgs e)
        {
            settings.DownloadThreads++;
            DownloadThreadsBox.Text = settings.DownloadThreads.ToString();
            ConfigManager.ConfigManager.SetSettings(settings);
        }
    }
}
