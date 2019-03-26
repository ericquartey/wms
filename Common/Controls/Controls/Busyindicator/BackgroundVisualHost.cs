﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ferretto.Common.Controls
{
    public class BackgroundVisualHost : FrameworkElement, IDisposable
    {
        #region Fields

        public static readonly DependencyProperty CreateContentProperty = DependencyProperty.Register(
            nameof(CreateContent),
            typeof(CreateVisualContent),
            typeof(BackgroundVisualHost),
            new FrameworkPropertyMetadata(OnCreateContentChanged));

        public static readonly DependencyProperty IsContentShowingProperty = DependencyProperty.Register(
            nameof(IsContentShowing),
            typeof(bool),
            typeof(BackgroundVisualHost),
            new FrameworkPropertyMetadata(false, OnIsContentShowingChanged));

        private bool disposedValue;

        private HostVisual hostVisual;

        private ThreadedVisualHelper threadedHelper;

        #endregion

        #region Delegates

        public delegate Visual CreateVisualContent();

        #endregion

        #region Properties

        public CreateVisualContent CreateContent
        {
            get => (CreateVisualContent)this.GetValue(CreateContentProperty);
            set => this.SetValue(CreateContentProperty, value);
        }

        public bool IsContentShowing
        {
            get => (bool)this.GetValue(IsContentShowingProperty);
            set => this.SetValue(IsContentShowingProperty, value);
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (this.hostVisual != null)
                {
                    yield return this.hostVisual;
                }
            }
        }

        protected override int VisualChildrenCount => this.hostVisual != null ? 1 : 0;

        #endregion

        #region Methods

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.threadedHelper?.Dispose();
                }

                this.disposedValue = true;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (this.hostVisual != null && index == 0)
            {
                return this.hostVisual;
            }

            throw new InvalidOperationException("Invalid index");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return this.threadedHelper != null ? this.threadedHelper.DesiredSize : base.MeasureOverride(availableSize);
        }

        private static void OnCreateContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bvh = (BackgroundVisualHost)d;

            if (bvh.IsContentShowing)
            {
                bvh.HideContentHelper();
                if (e.NewValue != null)
                {
                    bvh.CreateContentHelper();
                }
            }
        }

        private static void OnIsContentShowingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var backgroundVisHost = (BackgroundVisualHost)d;

            if (backgroundVisHost.CreateContent != null)
            {
                if ((bool)e.NewValue)
                {
                    backgroundVisHost.CreateContentHelper();
                }
                else
                {
                    backgroundVisHost.HideContentHelper();
                }
            }
        }

        private void CreateContentHelper()
        {
            this.threadedHelper = new ThreadedVisualHelper(this.CreateContent, this.SafeInvalidateMeasure);
            this.hostVisual = this.threadedHelper.HostVisual;
        }

        private void HideContentHelper()
        {
            if (this.threadedHelper != null)
            {
                this.threadedHelper.Exit();
                this.threadedHelper = null;
                this.InvalidateMeasure();
            }
        }

        private void SafeInvalidateMeasure()
        {
            this.Dispatcher.BeginInvoke(new Action(this.InvalidateMeasure), DispatcherPriority.Loaded);
        }

        #endregion
    }
}
