using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IDrawerOperationMessageData : IMessageData
    {
        #region Properties

        DrawerDestination Destination { get; set; }

        DrawerOperation Operation { get; set; }

        DrawerDestination Source { get; set; }

        DrawerOperationStep Step { get; set; }

        #endregion
    }
}
