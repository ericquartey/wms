using System;
using System.Collections.Generic;
using System.ServiceModel;
using Ferretto.Common.BusinessModels;

namespace Ferretto.WMS.Scheduler.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class MachineService : IMachine
    {
        private readonly Machine[] machines = new Machine[]
       {
            new Machine
            {
                Id = 1,
                AisleName = "Vertimag 1",
                MachineTypeDescription = "Vertimag",
                LastPowerOn = System.DateTime.Now,
                Model = "2018/XS"
            },
            new Machine
            {
                Id = 2,
                AisleName = "Vertimag 2",
                MachineTypeDescription = "Vertimag",
                LastPowerOn = System.DateTime.Now.Subtract(System.TimeSpan.FromMinutes(15)),
                Model = "2018/XS"
            },
       };

        private IMachineCallback Callback => OperationContext.Current.GetCallbackChannel<IMachineCallback>();
        #region Methods

        public double CompleteMission(double n1, double n2)
        {
            var result = n1 + n2;
            Console.WriteLine("Received Add({0},{1})", n1, n2);
            // Code added to write output to the console window.
            Console.WriteLine("Return: {0}", result);
            return result;
        }

        public IEnumerable<Machine> GetAll()
        {
            this.Callback.WakeUpClients("Wake up!!");
            return this.machines;
        }


        #endregion Methods
    }
}
