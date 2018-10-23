using System.ServiceModel;

namespace Ferretto.WMS.Scheduler.WCF
{
    public interface IMachineCallback
    {
        [OperationContract(IsOneWay = true)]
        void WakeUpClients(string eqn);
    }
}
