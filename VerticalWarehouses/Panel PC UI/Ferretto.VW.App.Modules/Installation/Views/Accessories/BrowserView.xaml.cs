﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Ferretto.VW.App.Modules.Installation.Views
{
    public partial class BrowserView : UserControl
    {
        #region Fields

        public static readonly DependencyProperty IsOpenProperty =
           DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(BrowserView), new PropertyMetadata(OnIsOpenChanged));

        public static readonly DependencyProperty UrlProperty =
                   DependencyProperty.Register(nameof(Url), typeof(string), typeof(BrowserView), new PropertyMetadata(OnUrlChanged));

        #endregion

        #region Constructors

        public BrowserView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public bool IsOpen
        {
            get => (bool)this.GetValue(IsOpenProperty);
            set => this.SetValue(IsOpenProperty, value);
        }

        public string Url
        {
            get => (string)this.GetValue(UrlProperty);
            set => this.SetValue(UrlProperty, value);
        }

        #endregion

        #region Methods

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var istance = d as BrowserView;
            if ((bool)e.NewValue == false)
            {
                if (!string.IsNullOrEmpty(istance.Url))
                {
                    istance.MyWebBrowser.Navigate("http://" + istance.Url);
                }
            }
        }

        private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var istance = d as BrowserView;
            if (!string.IsNullOrEmpty(e.NewValue.ToString()))
            {
                istance.MyWebBrowser.Navigate("http://" + e.NewValue.ToString());
            }
        }

        private void MyWebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (!e.Uri.ToString().Contains("http://" + this.Url) &&
                e.Uri.AbsolutePath != "location.reload();" &&
                e.Uri.AbsolutePath != "clickRefresh()")
            {
                e.Cancel = true;
            }
        }

        private void PpcButton_Click(object sender, RoutedEventArgs e)
        {
            this.MyWebBrowser.Refresh();
        }

        private void PpcButton_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            this.MyWebBrowser.Refresh();
        }

        #endregion
    }
}
