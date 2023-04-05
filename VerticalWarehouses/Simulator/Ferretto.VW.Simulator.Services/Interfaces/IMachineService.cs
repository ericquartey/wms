using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.Simulator.Services.Models;

namespace Ferretto.VW.Simulator.Services.Interfaces
{
    public interface IMachineService
    {
        #region Properties

        ObservableCollection<InverterModel> Inverters { get; set; }

        InverterModel Inverters00 { get; set; }

        InverterModel Inverters01 { get; set; }

        InverterModel Inverters02 { get; set; }

        InverterModel Inverters03 { get; set; }

        InverterModel Inverters04 { get; set; }

        InverterModel Inverters05 { get; set; }

        InverterModel Inverters06 { get; set; }

        InverterModel Inverters07 { get; set; }

        bool IsErrorGeneratorInverter { get; set; }

        bool IsErrorGeneratorIODevice { get; set; }

        bool IsStartedSimulator { get; }

        Machine Machine { get; set; }

        IODeviceModel RemoteIOs01 { get; set; }

        IODeviceModel RemoteIOs02 { get; set; }

        IODeviceModel RemoteIOs03 { get; set; }

        #endregion

        #region Methods

        Task ProcessErrorGeneratorInverterAsync();

        Task ProcessErrorGeneratorIODeviceAsync();

        Task ProcessStartSimulatorAsync();

        Task ProcessStopSimulatorAsync();

        Task SimulateErrorAsync();

        Task SimulateSpeedAsync(int simulateSpeed);

        #endregion
    }
}
