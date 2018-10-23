using System;
using System.ServiceModel;

namespace Ferretto.WMS.Scheduler.WCF
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            try
            {
                using (var serviceHost = new ServiceHost(typeof(CalculatorService)))
                {
                    serviceHost.Open();

                    Console.WriteLine("The service is ready.");
                    Console.WriteLine("Press <ENTER> to terminate service.");
                    Console.ReadLine();

                    Console.WriteLine("Terminating the service ...");
                    serviceHost.Close();
                }
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred:");
                Console.WriteLine(ce.Message);
                Console.ReadLine();
            }

            Console.WriteLine("Service terminated.");
        }

        #endregion Methods
    }
}
