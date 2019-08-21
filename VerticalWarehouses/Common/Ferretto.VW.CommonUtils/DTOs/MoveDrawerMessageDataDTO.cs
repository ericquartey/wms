using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.DTOs
{
    public class MoveDrawerMessageDataDTO
    {
        #region Constructors

        public MoveDrawerMessageDataDTO(DrawerOperation operation)
        {
            this.DrawerOperation = operation;
        }

        #endregion

        #region Properties

        public DrawerOperation DrawerOperation { get; set; }

        #endregion
    }
}
