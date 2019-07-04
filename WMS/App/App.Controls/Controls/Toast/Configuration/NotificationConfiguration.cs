using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Controls
{
    /// <summary>
    /// notification configuration
    /// </summary>
    public class NotificationConfiguration : INotificationConfiguration
    {
        #region Fields

        /// <summary>
        /// The default notifications window Height
        /// </summary>
        private const int DefaultHeight = 150;

        /// <summary>
        /// The default template of notification window
        /// </summary>
        private const string DefaultTemplateName = "notificationTemplate";

        /// <summary>
        /// The default notifications window Width
        /// </summary>
        private const int DefaultWidth = 300;

        /// <summary>
        /// The default display duration for a notification window.
        /// </summary>
        private static readonly TimeSpan DefaultDisplayDuration = TimeSpan.FromSeconds(2);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationConfiguration"/> class.
        /// Initialises the configuration object.
        /// </summary>
        /// <param name="displayDuration">The notification display duration. set it TimeSpan.Zero to use default value </param>
        /// <param name="width">The notification width. set it to null to use default value</param>
        /// <param name="height">The notification height. set it to null to use default value</param>
        /// <param name="templateName">The notification template name. set it to null to use default value</param>
        /// <param name="notificationFlowDirection">The notification flow direction. set it to null to use default value (RightBottom)</param>
        public NotificationConfiguration(TimeSpan displayDuration, int? width, int? height, string templateName, NotificationFlowDirection? notificationFlowDirection)
        {
            this.DisplayDuration = displayDuration > TimeSpan.Zero ? displayDuration : DefaultDisplayDuration;
            this.Width = width.HasValue ? width : DefaultWidth;
            this.Height = height.HasValue ? height : DefaultHeight;
            this.TemplateName = !string.IsNullOrEmpty(templateName) ? templateName : DefaultTemplateName;
            this.NotificationFlowDirection = notificationFlowDirection ?? NotificationFlowDirection.RightBottom;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default configuration object
        /// </summary>
        public static NotificationConfiguration DefaultConfiguration
        {
            get
            {
                return new NotificationConfiguration(DefaultDisplayDuration, DefaultWidth, DefaultHeight, DefaultTemplateName, NotificationFlowDirection.RightBottom);
            }
        }

        /// <summary>
        /// Gets the display duration for a notification window.
        /// </summary>
        public TimeSpan DisplayDuration { get; private set; }

        /// <summary>
        /// Gets notifications window Height
        /// </summary>
        public int? Height { get; private set; }

        /// <summary>
        /// Gets or sets the notification window flow direction
        /// </summary>
        public NotificationFlowDirection NotificationFlowDirection { get; set; }

        /// <summary>
        /// Gets the template of notification window
        /// </summary>
        public string TemplateName { get; private set; }

        /// <summary>
        /// Gets notifications window Width
        /// </summary>
        public int? Width { get; private set; }

        #endregion
    }
}
