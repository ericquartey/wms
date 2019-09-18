using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IDrawerOperationMessageData : IMessageData
    {
        #region Properties

        DrawerDestination Destination { get; set; }

        decimal DestinationHorizontalPosition { get; set; }

        decimal DestinationVerticalPosition { get; set; }

        bool IsDestinationPositive { get; set; }

        bool IsSourcePositive { get; set; }

        DrawerOperation Operation { get; set; }

        DrawerDestination Source { get; set; }

        decimal SourceHorizontalPosition { get; set; }

        decimal SourceVerticalPosition { get; set; }

        DrawerOperationStep Step { get; set; }

        #endregion
    }
}
