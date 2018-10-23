using System.Collections.Generic;
using System.ServiceModel;
using Ferretto.Common.BusinessModels;

namespace Ferretto.WMS.Scheduler.WCF
{
    [ServiceContract(
        Namespace = "http://www.ferrettogroup.com/wms/",
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IMachineCallback))]
    public interface IMachine
    {
        #region Methods

        [OperationContract]
        double CompleteMission(double n1, double n2);

        [OperationContract]
        IEnumerable<Machine> GetAll();


        #endregion Methods
    }
}
