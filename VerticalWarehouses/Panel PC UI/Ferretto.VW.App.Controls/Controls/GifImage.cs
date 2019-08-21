using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Ferretto.VW.App.Controls.Controls
{
    public class GifImage : Image
    {
        #region Fields

        public static readonly DependencyProperty AutoStartProperty = DependencyProperty.Register(
            nameof(AutoStart),
            typeof(bool),
            typeof(GifImage),
            new UIPropertyMetadata(false, AutoStartPropertyChanged));

        public static readonly DependencyProperty FrameIndexProperty = DependencyProperty.Register(
            nameof(FrameIndex),
            typeof(int),
            typeof(GifImage),
            new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        public static readonly DependencyProperty GifSourceProperty = DependencyProperty.Register(
            nameof(GifSource),
            typeof(string),
            typeof(GifImage),
            new UIPropertyMetadata(string.Empty, GifSourcePropertyChanged));

        private Int32Animation animation;

        private GifBitmapDecoder gifDecoder;

        private bool isInitialized;

        #endregion

        #region Constructors

        static GifImage()
        {
            VisibilityProperty.OverrideMetadata(typeof(GifImage),
                new FrameworkPropertyMetadata(VisibilityPropertyChanged));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Defines whether the animation starts on it's own.
        /// </summary>
        public bool AutoStart
        {
            get => (bool)this.GetValue(AutoStartProperty);
            set => this.SetValue(AutoStartProperty, value);
        }

        public int FrameIndex
        {
            get => (int)this.GetValue(FrameIndexProperty);
            set => this.SetValue(FrameIndexProperty, value);
        }

        public string GifSource
        {
            get => (string)this.GetValue(GifSourceProperty);
            set => this.SetValue(GifSourceProperty, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the animation.
        /// </summary>
        public void StartAnimation()
        {
            if (!this.isInitialized)
            {
                this.Initialize();
            }
            this.BeginAnimation(FrameIndexProperty, this.animation);
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void StopAnimation()
        {
            this.BeginAnimation(FrameIndexProperty, null);
        }

        private static void AutoStartPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (sender as GifImage).StartAnimation();
            }
        }

        private static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var gifImage = obj as GifImage;
            gifImage.Source = gifImage.gifDecoder.Frames[(int)ev.NewValue];
        }

        private static void GifSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as GifImage).Initialize();
        }

        private static void VisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((Visibility)e.NewValue == Visibility.Visible)
            {
                ((GifImage)sender).StartAnimation();
            }
            else
            {
                ((GifImage)sender).StopAnimation();
            }
        }

        private void Initialize()
        {
            this.gifDecoder = new GifBitmapDecoder(new Uri(this.GifSource), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            this.animation = new Int32Animation(0, this.gifDecoder.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, this.gifDecoder.Frames.Count / 10, (int)((this.gifDecoder.Frames.Count / 10.0 - this.gifDecoder.Frames.Count / 10) * 1000))));
            this.animation.RepeatBehavior = RepeatBehavior.Forever;
            this.Source = this.gifDecoder.Frames[0];

            this.isInitialized = true;
        }

        #endregion
    }
}
