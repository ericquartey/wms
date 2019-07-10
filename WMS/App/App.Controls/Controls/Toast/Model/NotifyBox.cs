using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Controls
{
    public class NotifyBox : INotifyBox
    {
        #region Fields

        /// <summary>
        /// The margin between notification windows.
        /// </summary>
        private const double Margin = 5;

        /// <summary>
        /// Max number of notifications window
        /// </summary>
        private const int MAX_NOTIFICATIONS = 1;

        private readonly DispatcherTimer timer;

        /// <summary>
        /// buffer list of notifications window.
        /// </summary>
        private readonly List<WindowInfo> notificationsBuffer;

        /// <summary>
        /// list of notifications window.
        /// </summary>
        private readonly List<WindowInfo> notificationWindows;

        /// <summary>
        /// Number of notification windows
        /// </summary>
        private int notificationWindowsCount;

        private WindowInfo currentWindowInfo;

        #endregion

        #region Constructors

        public NotifyBox()
        {
            this.notificationWindows = new List<WindowInfo>();
            this.notificationsBuffer = new List<WindowInfo>();
            this.notificationWindowsCount = 0;
            this.timer = new DispatcherTimer();
            this.timer.Tick += this.DispatcherTimer_Tick;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Remove all notifications from notification list and buffer list.
        /// </summary>
        public void ClearNotifications()
        {
            this.notificationWindows.Clear();
            this.notificationsBuffer.Clear();

            this.notificationWindowsCount = 0;
        }

        /// <summary>
        /// Shows the specified notification.
        /// </summary>
        /// <param name="content">The notification content.</param>
        /// <param name="configuration">The notification configuration object.</param>
        public void Show(object content, INotificationConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            if (configuration is NotificationConfiguration configNotification)
            {
                var notificationTemplate = (DataTemplate)Application.Current.Resources[configNotification.TemplateName];
                var window = new Window
                {
                    Title = string.Empty,
                    Width = configuration.Width.Value,
                    Height = configuration.Height.Value,

                    Content = content,
                    ShowActivated = false,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Background = Brushes.Transparent,
                    UseLayoutRounding = true,
                    ContentTemplate = notificationTemplate,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                };
                this.Show(window, configuration.DisplayDuration, configNotification.NotificationFlowDirection);
            }
        }

        /// <summary>
        /// Shows the specified notification.
        /// </summary>
        /// <param name="content">The notification content.</param>
        public void Show(object content)
        {
            this.Show(content, NotificationConfiguration.DefaultConfiguration);
        }

        /// <summary>
        /// Shows the specified window as a notification.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="displayDuration">The display duration.</param>
        /// <param name="notificationFlowDirection"></param>
        public void Show(Window window, TimeSpan displayDuration, NotificationFlowDirection notificationFlowDirection)
        {
            var behaviors = Interaction.GetBehaviors(window);
            behaviors.Add(new FadeBehavior());
            behaviors.Add(new SlideBehavior());
            SetWindowDirection(window, notificationFlowDirection);
            this.notificationWindowsCount += 1;
            var windowInfo = new WindowInfo
            {
                Id = this.notificationWindowsCount,
                DisplayDuration = displayDuration,
                Window = window
            };
            windowInfo.Window.Closed += this.Window_Closed;
            if (this.notificationWindows.Count + 1 > MAX_NOTIFICATIONS)
            {
                this.notificationsBuffer.Add(windowInfo);
            }
            else
            {
                this.StartWindowCloseTimer(windowInfo);
                if (window == null)
                {
                    return;
                }

                window.Show();
                this.notificationWindows.Add(windowInfo);
            }
        }

        /// <summary>
        /// Display the notification window in specified direction of the screen
        /// </summary>
        /// <param name="window"> The window object</param>
        /// <param name="notificationFlowDirection"> Direction in which new notifications will appear.</param>
        private static void SetWindowDirection(Window window, NotificationFlowDirection notificationFlowDirection)
        {
            var matrix = PresentationSource.FromVisual(Application.Current.MainWindow)
                .CompositionTarget
                .TransformFromDevice;
            var scaleFactor = matrix.M11;
            var offsetSize = FormControl.GetMainApplicationOffsetSize();

            var isMaximized = window.Owner.WindowState == WindowState.Maximized;
            var top = (int)((isMaximized ? offsetSize.screenTop : window.Owner.Top) / scaleFactor);
            var left = (int)((isMaximized ? offsetSize.screenLeft : window.Owner.Left) / scaleFactor);
            var width = (int)((isMaximized ? offsetSize.screenWidth : window.Owner.Width) / scaleFactor);
            var height = (int)((isMaximized ? offsetSize.screenHeight : window.Owner.Height) / scaleFactor);

            var workingArea = new System.Drawing.Rectangle(
                new System.Drawing.Point(left, top),
                new System.Drawing.Size(width, height));

            var corner = matrix.Transform(new Point(workingArea.Right, workingArea.Bottom));

            switch (notificationFlowDirection)
            {
                case NotificationFlowDirection.LeftBottom:
                    window.Left = 0;
                    window.Top = corner.Y - window.Height - window.Margin.Top;
                    break;

                case NotificationFlowDirection.LeftUp:
                    window.Left = 0;
                    window.Top = 0;
                    break;

                case NotificationFlowDirection.RightUp:
                    window.Left = corner.X - window.Width - window.Margin.Right - Margin;
                    window.Top = 0;
                    break;

                default:
                    window.Left = corner.X - window.Width - window.Margin.Right - Margin;
                    window.Top = corner.Y - window.Height - window.Margin.Top;
                    break;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.timer.Stop();
            if (this.currentWindowInfo != null)
            {
                this.OnTimerElapsed(this.currentWindowInfo);
            }
        }

        /// <summary>
        /// Called when the timer has elapsed. Removes any stale notifications.
        /// </summary>
        private void OnTimerElapsed(WindowInfo windowInfo)
        {
            if (this.notificationWindows.Count > 0 && this.notificationWindows.All(i => i.Id != windowInfo.Id))
            {
                return;
            }

            if (windowInfo.Window.IsMouseOver)
            {
                this.StartWindowCloseTimer(windowInfo);
            }
            else
            {
                var behaviors = Interaction.GetBehaviors(windowInfo.Window);
                var fadeBehavior = behaviors.OfType<FadeBehavior>().First();
                var slideBehavior = behaviors.OfType<SlideBehavior>().First();

                fadeBehavior.FadeOut();
                slideBehavior.SlideOut();

                void OnFadeOutCompleted(object sender2, EventArgs e2)
                {
                    fadeBehavior.FadeOutCompleted -= OnFadeOutCompleted;
                    this.notificationWindows.Remove(windowInfo);
                    windowInfo.Window.Close();

                    if (this.notificationsBuffer == null || this.notificationsBuffer.Count <= 0)
                    {
                        return;
                    }

                    var bufferWindowInfo = this.notificationsBuffer.First();
                    this.StartWindowCloseTimer(bufferWindowInfo);

                    this.notificationWindows.Add(bufferWindowInfo);
                    bufferWindowInfo.Window.Show();
                    this.notificationsBuffer.Remove(bufferWindowInfo);
                }

                fadeBehavior.FadeOutCompleted += OnFadeOutCompleted;
            }
        }

        private void StartWindowCloseTimer(WindowInfo windowInfo)
        {
            this.timer.Stop();
            this.timer.Interval = windowInfo.DisplayDuration;
            this.currentWindowInfo = windowInfo;
            this.timer.Start();
        }

        /// <summary>
        /// Called when the window is about to close.
        /// Remove the notification window from notification windows list and add one from the buffer list.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            var window = (Window)sender;
            if (this.notificationWindows.Count <= 0 || this.notificationWindows.First().Window != window)
            {
                return;
            }

            var windowInfo = this.notificationWindows.First();
            this.notificationWindows.Remove(windowInfo);
            if (this.notificationsBuffer == null || this.notificationsBuffer.Count <= 0)
            {
                return;
            }

            var bufferWindowInfo = this.notificationsBuffer.First();
            this.StartWindowCloseTimer(bufferWindowInfo);

            this.notificationWindows.Add(bufferWindowInfo);
            bufferWindowInfo.Window.Show();
            this.notificationsBuffer.Remove(bufferWindowInfo);
        }

        #endregion

        #region Classes

        /// <summary>
        /// Window metadata.
        /// </summary>
        private sealed class WindowInfo
        {
            #region Properties

            /// <summary>
            /// Gets or sets the display duration.
            /// </summary>
            /// <value>
            /// The display duration.
            /// </value>
            public TimeSpan DisplayDuration { get; set; }

            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the window.
            /// </summary>
            /// <value>
            /// The window.
            /// </value>
            public Window Window { get; set; }

            #endregion
        }

        #endregion
    }
}
