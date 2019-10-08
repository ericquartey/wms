using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IDrawerOperationMessageData : IMessageData
    {
        #region Properties

        LoadingUnitDestination Destination { get; set; }

        decimal DestinationHorizontalPosition { get; set; }

        decimal DestinationVerticalPosition { get; set; }

        bool IsDestinationPositive { get; set; }

        bool IsSourcePositive { get; set; }

        DrawerOperation Operation { get; set; }

        LoadingUnitDestination Source { get; set; }

        decimal SourceHorizontalPosition { get; set; }

        decimal SourceVerticalPosition { get; set; }

        #endregion
    }
}
