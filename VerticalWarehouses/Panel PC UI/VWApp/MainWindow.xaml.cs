﻿using System;
using System.Windows;

namespace Ferretto.VW.VWApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnClosed(EventArgs e)
        {
            Application.Current.Shutdown();
            base.OnClosed(e);
        }

        #endregion

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
