using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IDrawerOperationMessageData : IMessageData
    {
        #region Properties

        DrawerDestination Destination { get; set; }

        double DestinationHorizontalPosition { get; set; }

        double DestinationVerticalPosition { get; set; }

        bool IsDestinationPositive { get; set; }

        bool IsSourcePositive { get; set; }

        DrawerOperation Operation { get; set; }

        DrawerDestination Source { get; set; }

        double SourceHorizontalPosition { get; set; }

        double SourceVerticalPosition { get; set; }

        DrawerOperationStep Step { get; set; }

        #endregion
    }
}
