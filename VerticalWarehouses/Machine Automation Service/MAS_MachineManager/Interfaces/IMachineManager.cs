using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MachineManager
{
    public interface IMachineManager
    {
        #region Methods

        Task DoHoming(BroadcastDelegate _delegate);

        #endregion Methods
    }
}
