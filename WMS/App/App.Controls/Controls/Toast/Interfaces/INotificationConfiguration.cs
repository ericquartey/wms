using System;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface INotificationConfiguration
    {
        #region Properties

        TimeSpan DisplayDuration { get; }

        int? Height { get; }

        string TemplateName { get; }

        int? Width { get; }

        #endregion
    }
}
