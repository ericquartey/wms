using System;
using System.Threading.Tasks;

#nullable enable

namespace Ferretto.VW.Installer.Core
{
    public interface IMachineConfigurationService
    {
        #region Properties

        MAS.DataModels.VertimagConfiguration? Configuration { get; }

        #endregion

        #region Methods

        void ClearConfiguration();

        Task LoadFromFileAsync(string fileName);

        Task LoadFromWebServiceAsync(Uri uri);

        Task SaveToFileAsync(string configurationFilePath);

        #endregion
    }
}
