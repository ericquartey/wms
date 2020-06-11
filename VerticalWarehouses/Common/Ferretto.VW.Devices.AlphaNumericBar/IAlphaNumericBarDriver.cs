﻿using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using static Ferretto.VW.Devices.AlphaNumericBar.AlphaNumericBarCommands;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public interface IAlphaNumericBarDriver
    {
        #region Properties

        int NumberOfLeds { get; }

        bool TestEnabled { get; }

        #endregion

        #region Methods

        int CalculateArrowPosition(double loadUnitlengthInMM, double itemPositionXInMM);

        int CalculateOffset(int offset, string message);

        int CalculateOffsetArrowMiddlePosition(int delta = 1);

        Task<bool> ClearAsync();

        void Configure(IPAddress ipAddress, int port, AlphaNumericBarSize size);

        Task<bool> CustomAsync(string hexval);

        Task<bool> DimAsync(int dimension);

        Task<bool> EnabledAsync(bool value, bool force = true);

        Task<bool> HelpAsync();

        Task<bool> LuminosityAsync(int luminosity);

        Task<bool> ScrollDirAsync(ScrollDirection direction);

        Task<bool> ScrollSpeedAsync(int speed);

        Task<bool> SetAndWriteArrowAsync(int arrowPosition = 0, bool forceClear = true);

        Task<bool> SetAndWriteMessageAsync(string message, int offset = 0, bool forceClear = true);

        Task<bool> SetAndWriteMessageScrollAsync(string message, int offset, int scrollEnd, bool forceClear = true);

        Task<bool> SetCustomCharacterAsync(int index, int offset, bool forceClear = true);

        Task<bool> TestAsync(bool value);

        Task<bool> TestScrollAsync(bool value);

        #endregion
    }
}
