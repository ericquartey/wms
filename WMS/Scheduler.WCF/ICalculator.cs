using System.Collections.Generic;
using System.ServiceModel;
using Ferretto.Common.BusinessModels;

namespace Ferretto.WMS.Scheduler.WCF
{
    [ServiceContract(
        Namespace = "http://www.ferrettogroup.com/wms/",
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(ICalculatorCallback))]
    public interface ICalculator
    {
        #region Methods

        [OperationContract]
        double CompleteMission(double n1, double n2);

        [OperationContract]
        IEnumerable<Machine> GetAll();


        #endregion Methods
    }

    public interface ICalculatorCallback
    {
        [OperationContract(IsOneWay = true)]
        void Equals(double result);
        [OperationContract(IsOneWay = true)]
        void Equation(string eqn);
    }
}
