using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public interface IAlphaNumericBarDriver
    {
        #region Methods

        int CalculateArrowPosition(double loadUnitlengthInMM, double itemPositionXInMM);

        int CalculateOffset(int offset, string message);

        Task<bool> ClearAsync();

        bool Configure(IPAddress ipAddress, int port, AlphaNumericBarSize size);

        Task<bool> CustomAsync(string hexval);

        Task<bool> HelpAsync();

        Task<bool> SetAndWriteArrowAsync(int arrowPosition = 0, bool forceClear = true);

        Task<bool> SetAndWriteMessageAsync(string message, int offset = 0, bool forceClear = true);

        Task<bool> SetAndWriteMessageScrollAsync(string message, int offset, int scrollEnd, bool forceClear = true);

        Task<bool> SetDimAsync(int dimension);

        Task<bool> SetEnabledAsync(bool value);

        Task<bool> SetLuminosityAsync(int luminosity);

        Task<bool> SetTestAsync(bool value);

        #endregion
    }
}
