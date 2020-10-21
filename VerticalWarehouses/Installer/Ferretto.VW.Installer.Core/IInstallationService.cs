using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable enable

namespace Ferretto.VW.Installer.Core
{
    public interface IInstallationService : INotifyPropertyChanged
    {
        #region Events

        event EventHandler<InstallationFinishedEventArgs> Finished;

        #endregion

        #region Properties

        Step? ActiveStep { get; }

        string? InstallerVersion { get; }

        bool IsRollbackInProgress { get; }

        Uri? MasUrl { get; }

        string? MasVersion { get; }

        string? PanelPcVersion { get; }

        IEnumerable<Step> Steps { get; }

        Uri? TsUrl { get; }

        #endregion

        #region Methods

        void Abort();

        bool CanStart();

        Task DeserializeAsync(string sourceFileName);

        void Run();

        void SetConfiguration(Uri masUrl);

        #endregion
    }
}
