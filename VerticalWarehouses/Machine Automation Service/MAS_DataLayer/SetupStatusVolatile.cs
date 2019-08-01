using Ferretto.VW.MAS.DataLayer.Interfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public class SetupStatusVolatile : ISetupStatusVolatileDataLayer
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
