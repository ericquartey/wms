using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using static Ferretto.VW.Devices.AlphaNumericBar.AlphaNumericBarCommands;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public interface IAlphaNumericBarDriver
    {
        #region Properties

        int NumberOfLeds { get; }

        string SelectedMessage { get; set; }

        double? SelectedPosition { get; set; }

        bool TestEnabled { get; }

        #endregion

        #region Methods

        int CalculateArrowPosition(double loadUnitlengthInMM, double itemPositionXInMM);

        int CalculateOffset(int offset, string message);

        int CalculateOffsetArrowMiddlePosition(int delta = 1);

        Task<bool> ClearAsync();

        void ClearCommands();

        void Configure(IPAddress ipAddress, int port, AlphaNumericBarSize size, bool bayIsExternal = false, int maxMessageLength = 125);

        Task ConnectAsync();

        Task<bool> CustomAsync(string hexval);

        Task<bool> DimAsync(int dimension);

        void Disconnect();

        Task<bool> EnabledAsync(bool value, bool force = true);

        Task<bool> ExecuteCommandsAsync(SemaphoreSlim syncObject);

        bool GetOffsetArrowAndMessage(double x, string message, out int offsetArrow, out int offsetMessage, out int scrollEnd);

        bool GetOffsetArrowAndMessageFromCompartment(double compartmentWidth, double itemXPosition, string message, double loadingUnitWidth, WarehouseSide side, out int offsetArrow, out int offsetMessage, out int scrollEnd);

        bool GetOffsetMessage(double x, string message, out int offsetMessage);

        Task<bool> HelpAsync();

        Task<bool> LuminosityAsync(int luminosity);

        string NormalizeMessageCharacters(string str);

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
