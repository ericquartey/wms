using System.ServiceModel;

namespace Ferretto.WMS.Scheduler.WCF.Contracts
{
    [ServiceContract(Namespace = "http://www.ferrettogroup.com/wms/")]
    public interface ICalculator
    {
        #region Methods

        [OperationContract]
        double Add(double n1, double n2);

        [OperationContract]
        double Divide(double n1, double n2);

        [OperationContract]
        double Multiply(double n1, double n2);

        [OperationContract]
        double Subtract(double n1, double n2);

        #endregion Methods
    }
}
