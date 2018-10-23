using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Ferretto.WMS.Scheduler.WCF.Contracts;

namespace Ferretto.WMS.Scheduler.WCF
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            // Step 1 Create a URI to serve as the base address.
            var baseAddress = new Uri("http://localhost:8000/wms/");

            // Step 2 Create a ServiceHost instance
            var selfHost = new ServiceHost(typeof(CalculatorService), baseAddress);

            try
            {
                // Step 3 Add a service endpoint.
                selfHost.AddServiceEndpoint(typeof(ICalculator), new WSHttpBinding(), "CalculatorService");

                // Step 4 Enable metadata exchange.
                var smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                selfHost.Description.Behaviors.Add(smb);

                // Step 5 Start the service.
                selfHost.Open();
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();

                // Close the ServiceHostBase to shutdown the service.
                selfHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                selfHost.Abort();
            }
        }

        #endregion Methods
    }
}
