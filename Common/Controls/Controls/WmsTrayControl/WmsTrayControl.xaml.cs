﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ferretto.Common.Controls
{
    public partial class WmsTrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CommandDoubleClickProperty = DependencyProperty.Register(
            nameof(CommandDoubleClick), typeof(ICommand), typeof(WmsTrayControl), new UIPropertyMetadata(OnCommandDoubleClickChanged));

        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(
            nameof(Compartments), typeof(IEnumerable<IDrawableCompartment>), typeof(WmsTrayControl));

        public static readonly DependencyProperty DimensionHeightProperty = DependencyProperty.Register(
            nameof(DimensionHeight), typeof(double), typeof(WmsTrayControl));

        public static readonly DependencyProperty DimensionWidthProperty = DependencyProperty.Register(
            nameof(DimensionWidth), typeof(double), typeof(WmsTrayControl));

        public static readonly DependencyProperty GridLinesColorProperty = DependencyProperty.Register(
            nameof(GridLinesColor), typeof(Brush), typeof(WmsTrayControl), new UIPropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(
            nameof(IsCompartmentSelectable), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(WmsTrayControl));

        public static readonly DependencyProperty OriginHorizontalProperty = DependencyProperty.Register(
            nameof(OriginHorizontal), typeof(OriginHorizontal), typeof(WmsTrayControl), new FrameworkPropertyMetadata(OriginHorizontal.Left));

        public static readonly DependencyProperty OriginVerticalProperty = DependencyProperty.Register(
            nameof(OriginVertical), typeof(OriginVertical), typeof(WmsTrayControl), new FrameworkPropertyMetadata(OriginVertical.Bottom));

        public static readonly DependencyProperty RulerFontSizeProperty = DependencyProperty.Register(
            nameof(RulerFontSize), typeof(int), typeof(WmsTrayControl), new UIPropertyMetadata(8));

        public static readonly DependencyProperty RulerForegroundProperty = DependencyProperty.Register(
            nameof(RulerForeground), typeof(Brush), typeof(WmsTrayControl), new UIPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty RulerInfoProperty = DependencyProperty.Register(
            nameof(RulerInfo), typeof(string), typeof(WmsTrayControl));

        public static readonly DependencyProperty RulerStepProperty = DependencyProperty.Register(
            nameof(RulerStep), typeof(int), typeof(WmsTrayControl), new UIPropertyMetadata(100));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
            nameof(SelectedColorFilterFunc), typeof(Func<IDrawableCompartment, IDrawableCompartment, string>), typeof(WmsTrayControl));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(IDrawableCompartment), typeof(WmsTrayControl));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
            nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ShowInfoProperty = DependencyProperty.Register(
            nameof(ShowInfo), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowLittleMarkProperty = DependencyProperty.Register(
           nameof(ShowLittleMark), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMarkProperty = DependencyProperty.Register(
          nameof(ShowMark), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMiddleMarkProperty = DependencyProperty.Register(
           nameof(ShowMiddleMark), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(
            nameof(ShowRuler), typeof(bool), typeof(WmsTrayControl));

        public static readonly DependencyProperty TrayWidthProperty =
            DependencyProperty.Register(nameof(TrayWidth), typeof(double), typeof(WmsTrayControl));

        #endregion

        #region Constructors

        public WmsTrayControl()
        {
            this.InitializeComponent();
            var wmsTrayControl = this;
            this.RootTrayGrid.DataContext = wmsTrayControl;
            this.SizeChanged += this.WmsTrayControl_SizeChanged;
        }

        #endregion

        #region Properties

        public ICommand CommandDoubleClick
        {
            get => (ICommand)this.GetValue(CommandDoubleClickProperty);
            set => this.SetValue(CommandDoubleClickProperty, value);
        }

        public IEnumerable<IDrawableCompartment> Compartments
        {
            get => (IEnumerable<IDrawableCompartment>)this.GetValue(CompartmentsProperty);
            set => this.SetValue(CompartmentsProperty, value);
        }

        public double DimensionHeight
        {
            get => (double)this.GetValue(DimensionHeightProperty);
            set => this.SetValue(DimensionHeightProperty, value);
        }

        public double DimensionWidth
        {
            get => (double)this.GetValue(DimensionWidthProperty);
            set => this.SetValue(DimensionWidthProperty, value);
        }

        public Brush GridLinesColor
        {
            get => (Brush)this.GetValue(GridLinesColorProperty);
            set => this.SetValue(GridLinesColorProperty, value);
        }

        public bool IsCompartmentSelectable
        {
            get => (bool)this.GetValue(IsCompartmentSelectableProperty);
            set => this.SetValue(IsCompartmentSelectableProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        public OriginHorizontal OriginHorizontal
        {
            get => (OriginHorizontal)this.GetValue(OriginHorizontalProperty);
            set => this.SetValue(OriginHorizontalProperty, value);
        }

        public OriginVertical OriginVertical
        {
            get => (OriginVertical)this.GetValue(OriginVerticalProperty);
            set => this.SetValue(OriginVerticalProperty, value);
        }

        public int RulerFontSize
        {
            get => (int)this.GetValue(RulerFontSizeProperty);
            set => this.SetValue(RulerFontSizeProperty, value);
        }

        public Brush RulerForeground
        {
            get => (Brush)this.GetValue(RulerForegroundProperty);
            set => this.SetValue(RulerForegroundProperty, value);
        }

        public string RulerInfo
        {
            get => (string)this.GetValue(RulerInfoProperty);
            set => this.SetValue(RulerInfoProperty, value);
        }

        public int RulerStep
        {
            get => (int)this.GetValue(RulerStepProperty);
            set => this.SetValue(RulerStepProperty, value);
        }

        public Func<IDrawableCompartment, IDrawableCompartment, string> SelectedColorFilterFunc
        {
            get => (Func<IDrawableCompartment, IDrawableCompartment, string>)this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public IDrawableCompartment SelectedItem
        {
            get => (IDrawableCompartment)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public bool ShowInfo
        {
            get => (bool)this.GetValue(ShowInfoProperty);
            set => this.SetValue(ShowInfoProperty, value);
        }

        public bool ShowLittleMark
        {
            get => (bool)this.GetValue(ShowLittleMarkProperty);
            set => this.SetValue(ShowLittleMarkProperty, value);
        }

        public bool ShowMark
        {
            get => (bool)this.GetValue(ShowMarkProperty);
            set => this.SetValue(ShowMarkProperty, value);
        }

        public bool ShowMiddleMark
        {
            get => (bool)this.GetValue(ShowMiddleMarkProperty);
            set => this.SetValue(ShowMiddleMarkProperty, value);
        }

        public bool ShowRuler
        {
            get => (bool)this.GetValue(ShowRulerProperty);
            set => this.SetValue(ShowRulerProperty, value);
        }

        public double TrayWidth
        {
            get => (double)this.GetValue(TrayWidthProperty);
            set => this.SetValue(TrayWidthProperty, value);
        }

        #endregion

        #region Methods

        public static void OnCommandDoubleClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                if (e.NewValue != null && e.OldValue == null)
                {
                    control.MouseDoubleClick += OnMouseDoubleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.MouseDoubleClick -= OnMouseDoubleClick;
                }
            }
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            var command = (ICommand)control.GetValue(CommandDoubleClickProperty);
            var commandParameter = control.GetValue(CommandDoubleClickProperty);
            command.Execute(commandParameter);
        }

        private void WmsTrayControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.CanvasListBoxControl.SetSize(e.NewSize.Height - 1, e.NewSize.Width - 1);
            this.TrayWidth = this.CanvasListBoxControl.TrayWidth;
        }

        #endregion
    }
}
