using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Core.Providers
{
    public class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public MachineProvider(
            WMS.Data.WebAPI.Contracts.IMachinesDataService machinesDataService,
            IBayProvider bayProvider)
        {
            this.machinesDataService = machinesDataService;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Machine>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            try
            {
                var machines = await this.machinesDataService
                    .GetAllAsync(
                        skip,
                        take,
                        whereString,
                        orderBySortOptions.ToQueryString(),
                        searchString);

                return machines
                    .Select(m => new Machine
                    {
                        AisleName = m.AisleName,
                        AreaFillRate = m.AreaFillRate,
                        AreaName = m.AreaName,
                        AutomaticTime = m.AutomaticTime,
                        BuildDate = m.BuildDate,
                        CradlesCount = m.CradlesCount,
                        CustomerAddress = m.CustomerAddress,
                        CustomerCity = m.CustomerCity,
                        CustomerCode = m.CustomerCode,
                        CustomerCountry = m.CustomerCountry,
                        CustomerName = m.CustomerName,
                        ErrorTime = m.ErrorTime,
                        Id = m.Id,
                        Image = m.Image,
                        InputLoadingUnitsCount = m.InputLoadingUnitsCount,
                        InstallationDate = m.InstallationDate,
                        LastPowerOn = m.LastPowerOn,
                        LastServiceDate = m.LastServiceDate,
                        Latitude = m.Latitude,
                        LoadingUnitsPerCradle = m.LoadingUnitsPerCradle,
                        Longitude = m.Longitude,
                        MachineTypeDescription = m.MachineTypeDescription,
                        MaintenanceStatus = (MaintenanceStatus)m.MaintenanceStatus,
                        ManualTime = m.ManualTime,
                        MissionTime = m.MissionTime,
                        Model = m.Model,
                        MovedLoadingUnitsCount = m.MovedLoadingUnitsCount,
                        NextServiceDate = m.NextServiceDate,
                        Nickname = m.Nickname,
                        OutputLoadingUnitsCount = m.OutputLoadingUnitsCount,
                        PowerOnTime = m.PowerOnTime,
                        RegistrationNumber = m.RegistrationNumber,
                        Status = (MachineStatus)m.Status,
                        TestDate = m.TestDate,
                        TotalMaxWeight = m.TotalMaxWeight,
                        WeightFillRate = m.WeightFillRate,
                    });
            }
            catch
            {
                return new List<Machine>();
            }
        }

        public async Task<int> GetAllCountAsync(string whereString = null, string searchString = null)
        {
            try
            {
                return await this.machinesDataService
                    .GetAllCountAsync(whereString, searchString);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<MachineDetails> GetByIdAsync(int id)
        {
            try
            {
                var machine = await this.machinesDataService.GetByIdAsync(id);
                if (machine == null)
                {
                    return null;
                }

                var result = await this.bayProvider.GetByMachineIdAsync(id);
                IEnumerable<BayDetails> bays = null;
                if (result.Success)
                {
                    bays = result.Entity;
                }

                return new MachineDetails
                {
                    AisleName = machine.AisleName,
                    AreaFillRate = machine.AreaFillRate,
                    AreaName = machine.AreaName,
                    AutomaticTime = machine.AutomaticTime,
                    Bays = bays,
                    BuildDate = machine.BuildDate,
                    CellsCount = machine.CellsCount,
                    CompartmentsCount = machine.CompartmentsCount,
                    CradlesCount = machine.CradlesCount,
                    CustomerAddress = machine.CustomerAddress,
                    CustomerCity = machine.CustomerCity,
                    CustomerCode = machine.CustomerCode,
                    CustomerCountry = machine.CustomerCountry,
                    CustomerName = machine.CustomerName,
                    ErrorTime = machine.ErrorTime,
                    GrossMaxWeight = machine.GrossMaxWeight,
                    GrossWeight = machine.GrossWeight,
                    Id = machine.Id,
                    Image = machine.Image,
                    InputLoadingUnitsCount = machine.InputLoadingUnitsCount,
                    InstallationDate = machine.InstallationDate,
                    ItemsCount = machine.ItemsCount,
                    LastPowerOn = machine.LastPowerOn,
                    LastServiceDate = machine.LastServiceDate,
                    Latitude = machine.Latitude,
                    LoadingUnitsCount = machine.LoadingUnitsCount,
                    LoadingUnitsPerCradle = machine.LoadingUnitsPerCradle,
                    Longitude = machine.Longitude,
                    MachineTypeDescription = machine.MachineTypeDescription,
                    MaintenanceStatus = (MaintenanceStatus)machine.MaintenanceStatus,
                    ManualTime = machine.ManualTime,
                    MissionsCount = machine.MissionsCount,
                    MissionTime = machine.MissionTime,
                    Model = machine.Model,
                    MovedLoadingUnitsCount = machine.MovedLoadingUnitsCount,
                    NetMaxWeight = machine.NetMaxWeight,
                    NetWeight = machine.NetWeight,
                    NextServiceDate = machine.NextServiceDate,
                    Nickname = machine.Nickname,
                    OutputLoadingUnitsCount = machine.OutputLoadingUnitsCount,
                    PowerOnTime = machine.PowerOnTime,
                    RegistrationNumber = machine.RegistrationNumber,
                    ServiceUrl = machine.ServiceUrl,
                    Status = (MachineStatus)machine.Status,
                    TestDate = machine.TestDate,
                    TotalMaxWeight = machine.TotalMaxWeight,
                    WeightFillRate = machine.WeightFillRate,
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return await this.machinesDataService.GetUniqueValuesAsync(propertyName);
            }
            catch
            {
                return new List<object>();
            }
        }

        #endregion
    }
}
