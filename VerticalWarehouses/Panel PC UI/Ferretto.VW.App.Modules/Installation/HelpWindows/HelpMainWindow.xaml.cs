﻿using System.Windows;
using System.Windows.Controls;
using Prism.Events;

namespace Ferretto.VW.App.Installation.HelpWindows
{
    /// <summary>
    /// Interaction logic for HelpMainWindow.xaml
    /// </summary>
    public partial class HelpMainWindow : Window
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public HelpMainWindow(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public ContentPresenter HelpMainWindowContentRegion => this.HelpContentRegion;

        #endregion

        #region Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        #endregion
    }
}
