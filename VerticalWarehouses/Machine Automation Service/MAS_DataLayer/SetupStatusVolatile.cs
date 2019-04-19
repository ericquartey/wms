using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public class SetupStatusVolatile : ISetupStatusVolatile
    {
        #region Fields

        private bool verticalHomingDone;

        #endregion

        #region Constructors

        public SetupStatusVolatile()
        {
            this.verticalHomingDone = false;
        }

        public SetupStatusVolatile(bool verticalHomingDone)
        {
            this.verticalHomingDone = verticalHomingDone;
        }

        #endregion

        #region Properties

        public bool VerticalHomingDone
        {
            get => this.verticalHomingDone;
            set => this.verticalHomingDone = value;
        }

        #endregion
    }
}
