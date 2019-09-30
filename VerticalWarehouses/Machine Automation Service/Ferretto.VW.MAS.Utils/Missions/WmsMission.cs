// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public class WmsMission : Mission
    {

        #region Fields

        private bool disposed;

        #endregion



        #region Methods

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                // Managed Resources
            }

            // Unmanaged Resources
            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
