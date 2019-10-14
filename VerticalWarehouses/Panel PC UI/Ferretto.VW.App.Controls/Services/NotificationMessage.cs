using System;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Controls.Services
{
    public sealed class NotificationMessage
    {
        #region Constructors

        public NotificationMessage(string text, NotificationSeverity severity = NotificationSeverity.Info)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            this.Severity = severity;
            this.Text = text;
        }

        #endregion

        #region Properties

        public NotificationSeverity Severity { get; }

        public string Text { get; }

        #endregion
    }
}
