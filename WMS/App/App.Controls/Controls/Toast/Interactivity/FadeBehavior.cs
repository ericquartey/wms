using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace Ferretto.WMS.App.Controls
{
    /// <summary>
    /// The fade behaviour.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Minor Code Smell",
        "S3220:Method calls should not resolve ambiguously to overloads with \"params\"",
        Justification = "Yes")]
    public sealed class FadeBehavior : Behavior<FrameworkElement>
    {
        #region Fields

        public static readonly DependencyProperty BeginTimeProperty = DependencyProperty.Register(
            "BeginTime",
            typeof(TimeSpan),
            typeof(FadeBehavior),
            new PropertyMetadata(default(TimeSpan)));

        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
            "Duration",
            typeof(TimeSpan),
            typeof(FadeBehavior),
            new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 300)));

        public static readonly DependencyProperty IsAnimatingOnIsVisibleChangedProperty = DependencyProperty.Register(
            "IsAnimatingOnIsVisibleChanged",
            typeof(bool),
            typeof(FadeBehavior),
            new PropertyMetadata(true));

        public static readonly DependencyProperty IsAnimatingOnLoadedProperty = DependencyProperty.Register(
            "IsAnimatingOnLoaded",
            typeof(bool),
            typeof(FadeBehavior),
            new PropertyMetadata(true));

        #endregion

        #region Events

        /// <summary>
        /// The fade in completed.
        /// </summary>
        public event EventHandler FadeInCompleted;

        /// <summary>
        /// The fade out completed.
        /// </summary>
        public event EventHandler FadeOutCompleted;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the begin time.
        /// </summary>
        public TimeSpan BeginTime
        {
            get { return (TimeSpan)this.GetValue(BeginTimeProperty); }
            set { this.SetValue(BeginTimeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public TimeSpan Duration
        {
            get { return (TimeSpan)this.GetValue(DurationProperty); }
            set { this.SetValue(DurationProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is animating on is visible changed.
        /// </summary>
        public bool IsAnimatingOnIsVisibleChanged
        {
            get { return (bool)this.GetValue(IsAnimatingOnIsVisibleChangedProperty); }
            set { this.SetValue(IsAnimatingOnIsVisibleChangedProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is animating on loaded.
        /// </summary>
        public bool IsAnimatingOnLoaded
        {
            get { return (bool)this.GetValue(IsAnimatingOnLoadedProperty); }
            set { this.SetValue(IsAnimatingOnLoadedProperty, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The fade in.
        /// </summary>
        public void FadeIn()
        {
            var storyboard = GetFadeInStoryboard(this.BeginTime, this.Duration);
            EventHandler eventHandler = null;
            eventHandler = (sender, e) =>
            {
                storyboard.Completed -= eventHandler;
                this.OnFadeInCompleted(this, EventArgs.Empty);
            };
            storyboard.Completed += eventHandler;
            storyboard.Begin(this.AssociatedObject);
        }

        /// <summary>
        /// The fade out.
        /// </summary>
        public void FadeOut()
        {
            var storyboard = GetFadeOutStoryboard(this.BeginTime, this.Duration);
            EventHandler eventHandler = null;
            eventHandler = (sender, e) =>
            {
                storyboard.Completed -= eventHandler;
                this.OnFadeOutCompleted(this, EventArgs.Empty);
            };
            storyboard.Completed += eventHandler;
            storyboard.Begin(this.AssociatedObject);
        }

        /// <summary>
        /// The on attached.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.IsVisibleChanged += this.OnIsVisibleChanged;
            this.AssociatedObject.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// The on detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.IsVisibleChanged -= this.OnIsVisibleChanged;
            this.AssociatedObject.Loaded -= this.OnLoaded;
        }

        /// <summary>
        /// The get fade in storyboard.
        /// </summary>
        /// <param name="beginTime"> The begin time. </param>
        /// <param name="duration"> The duration. </param>
        /// <returns> The <see cref="Storyboard"/>. </returns>
        private static Storyboard GetFadeInStoryboard(TimeSpan beginTime, TimeSpan duration)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(default(TimeSpan)),
                    Value = 0
                });
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(beginTime),
                    Value = 0
                });
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(beginTime + duration),
                    Value = 1
                });

            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(animation);

            return storyboard;
        }

        /// <summary>
        /// The get fade out storyboard.
        /// </summary>
        /// <param name="beginTime"> The begin time. </param>
        /// <param name="duration"> The duration. </param>
        /// <returns> The <see cref="Storyboard"/>. </returns>
        private static Storyboard GetFadeOutStoryboard(TimeSpan beginTime, TimeSpan duration)
        {
            var animation = new DoubleAnimationUsingKeyFrames();
            animation.FillBehavior = FillBehavior.HoldEnd;
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame
                {
                    KeyTime = KeyTime.FromTimeSpan(beginTime),
                    Value = 1
                });
            animation.KeyFrames.Add(
                new SplineDoubleKeyFrame
                {
                    KeySpline = new KeySpline(0.5, 0, 1, 0.75),
                    KeyTime = KeyTime.FromTimeSpan(beginTime + duration),
                    Value = 0
                });

            var storyboard = new Storyboard();
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(animation);

            return storyboard;
        }

        /// <summary>
        /// The on fade in completed.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void OnFadeInCompleted(object sender, EventArgs e)
        {
            var eventHandler = this.FadeInCompleted;

            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
        }

        /// <summary>
        /// The on fade out completed.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void OnFadeOutCompleted(object sender, EventArgs e)
        {
            var eventHandler = this.FadeOutCompleted;

            if (eventHandler != null)
            {
                eventHandler(sender, e);
            }
        }

        /// <summary>
        /// The on is visible changed.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsAnimatingOnIsVisibleChanged)
            {
                if (this.AssociatedObject.Visibility == Visibility.Visible)
                {
                    this.FadeIn();
                }
                else
                {
                    this.FadeOut();
                }
            }
        }

        /// <summary>
        /// The on loaded.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.IsAnimatingOnLoaded)
            {
                this.FadeIn();
            }
        }

        #endregion
    }
}
