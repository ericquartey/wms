using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MachineProvider : BaseProvider, IMachineProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly IMachinesLiveDataContext liveMachinesDataContext;

        #endregion

        #region Constructors

        public MachineProvider(
            DatabaseContext dataContext,
            IMachinesLiveDataContext liveMachinesDataContext,
            IBayProvider bayProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.bayProvider = bayProvider;
            this.liveMachinesDataContext = liveMachinesDataContext;
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
            var machines = await this.GetAllBase()
                .ToArrayAsync<Machine, Common.DataModels.Machine>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var machine in machines)
            {
                this.MergeLiveData(machine);
            }

            return machines;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Machine, Common.DataModels.Machine>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<IOperationResult<IEnumerable<MachineServiceInfo>>> GetAllMachinesServiceInfoAsync()
        {
            var machines = await this.DataContext.Machines
                .Select(m => new MachineServiceInfo
                {
                    Id = m.Id,
                    ServiceUrl = m.ServiceUrl,
                    Bays = m.Bays.Select(b => new Bay { Id = b.Id }),
                })
                .ToArrayAsync();

            return new SuccessOperationResult<IEnumerable<MachineServiceInfo>>(machines);
        }

        public async Task<Machine> GetByBayIdAsync(int bayId)
        {
            var bay = await this.bayProvider.GetByIdAsync(bayId);

            if (bay == null || bay.MachineId == null)
            {
                return null;
            }

            var machine = await this.GetAllBase()
                       .SingleOrDefaultAsync(i => i.Id == bay.MachineId);

            return this.MergeLiveData(machine);
        }

        public async Task<MachineDetails> GetByIdAsync(int id)
        {
            var machine = await this.GetAllDetailsBase()
                     .SingleOrDefaultAsync(i => i.Id == id);
            if (machine == null)
            {
                return null;
            }

            return this.MergeLiveData(machine);
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.DataContext.Machines,
                       this.GetAllBase());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<Machine, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (m) =>
                (m.AisleName != null && m.AisleName.Contains(search))
                || (m.AreaName != null && m.AreaName.Contains(search))
                || (m.MachineTypeDescription != null && m.MachineTypeDescription.Contains(search))
                || (m.Model != null && m.Model.Contains(search))
                || (m.Nickname != null && m.Nickname.Contains(search))
                || (m.RegistrationNumber != null && m.RegistrationNumber.Contains(search))
                || (successConversionAsInt
                    && Equals(m.AreaFillRate, searchAsInt));
        }

        private static Machine GetMaintenanceStatus(Machine machine)
        {
            if (machine != null)
            {
                if (machine.Id == 1)
                {
                    machine.MaintenanceStatus = Enums.MaintenanceStatus.Valid;
                }
                else if (machine.Id == 2)
                {
                    machine.MaintenanceStatus = Enums.MaintenanceStatus.Expiring;
                }
                else
                {
                    machine.MaintenanceStatus = Enums.MaintenanceStatus.Expired;
                }
            }

            return machine;
        }

        private static MachineDetails GetMaintenanceStatus(MachineDetails machine)
        {
            if (machine != null)
            {
                if (machine.Id == 1)
                {
                    machine.MaintenanceStatus = Enums.MaintenanceStatus.Valid;
                }
                else if (machine.Id == 2)
                {
                    machine.MaintenanceStatus = Enums.MaintenanceStatus.Expiring;
                }
                else
                {
                    machine.MaintenanceStatus = Enums.MaintenanceStatus.Expired;
                }
            }

            return machine;
        }

        private IQueryable<MachineItems> GetMachinesItems()
        {
            return this.DataContext.Machines
                .GroupJoin(
                    this.DataContext.Cells
                        .Join(
                            this.DataContext.LoadingUnits,
                            c => c.Id,
                            l => l.CellId,
                            (c, l) => new
                            {
                                c.AisleId,
                                LoadingUnitId = l.Id,
                            })
                        .Join(
                            this.DataContext.Compartments,
                            l => l.LoadingUnitId,
                            c => c.LoadingUnitId,
                            (l, c) => new
                            {
                                l.AisleId,
                                c.ItemId,
                            })
                        .Distinct()
                        .GroupBy(i => i.AisleId)
                        .Select(g => new
                        {
                            AisleId = g.Key,
                            ItemsCount = g.Count(),
                        }),
                    m => m.AisleId,
                    c => c.AisleId,
                    (m, c) => new
                    {
                        m.Id,
                        ItemsCount = c.Select(x => x.ItemsCount).DefaultIfEmpty(),
                    })
                .SelectMany(
                    mc => mc.ItemsCount.DefaultIfEmpty(),
                    (m, c) => new
                    {
                        m.Id,
                        ItemsCount = c,
                    })
                .Select(x => new MachineItems
                {
                    Id = x.Id,
                    ItemsCount = x.ItemsCount,
                });
        }

        private IQueryable<MachineOccupation> GetMachinesAreaOccupation()
        {
            return this.DataContext.Machines
                .GroupJoin(
                    this.DataContext.LoadingUnits,
                    m => m.AisleId,
                    l => l.Cell.AisleId,
                    (m, l) => new
                    {
                        MachineId = m.Id,
                        LoadingUnit = l,
                    })
                .SelectMany(
                    ml => ml.LoadingUnit.DefaultIfEmpty(),
                    (m, l) => new
                    {
                        m.MachineId,
                        LoadingUnit = l,
                    })
                .GroupJoin(
                    this.DataContext.Compartments,
                    m => m.LoadingUnit.Id,
                    c => c.LoadingUnitId,
                    (m, c) => new
                    {
                        m.MachineId,
                        m.LoadingUnit,
                        Compartment = c,
                    })
                .SelectMany(
                    mc => mc.Compartment.DefaultIfEmpty(),
                    (m, c) => new
                    {
                        m.MachineId,
                        LoadingUnitId = m.LoadingUnit != null ? m.LoadingUnit.Id : (int?)null,
                        LoadingUnitArea = m.LoadingUnit != null
                            ? m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Depth *
                            m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Width
                            : 0,
                        CompartmentArea = (c != null ? c.CompartmentType.Width : 0) *
                            (c != null ? c.CompartmentType.Depth : 0),
                    })
                .GroupBy(x => x.MachineId)
                .Select(x => new
                {
                    MachineId = x.Key,
                    g = x.GroupBy(j => new { j.LoadingUnitId, j.LoadingUnitArea })
                        .Select(a => new
                        {
                            a.First().LoadingUnitArea,
                            CompartmentArea = a.Sum(y => y.CompartmentArea),
                        }),
                })
                .Select(x => new MachineOccupation
                {
                    Id = x.MachineId,
                    Occupation = x.g.Sum(y => y.LoadingUnitArea) > 0
                        ? x.g.Sum(y => y.CompartmentArea) / x.g.Sum(y => y.LoadingUnitArea) * 100
                        : 0,
                });
        }

        private IQueryable<MachinesLoadingUnitsInfo> GetMachinesLoadingUnitsInfo()
        {
            return this.DataContext.Machines
                .GroupJoin(
                    this.DataContext.LoadingUnits
                        .Where(l => l.CellId.HasValue)
                        .Select(l => new
                        {
                            l.Id,
                            l.Cell.AisleId,
                            l.Weight,
                            l.LoadingUnitType.EmptyWeight,
                            CompartmentsCount = l.Compartments.Count(),
                            MissionsCount = l.Missions.Count(),
                        }),
                    m => m.AisleId,
                    l => l.AisleId,
                    (m, ll) => new
                    {
                        MachineId = m.Id,
                        TotalMaxWeight = m.TotalMaxWeight,
                        Weights = ll.Select(l => new { l.Weight, l.EmptyWeight }),
                        LoadingUnitsCount = ll.Count(),
                        CompartmentsCount = ll.Sum(l => l.CompartmentsCount),
                        MissionsCount = ll.Sum(l => l.MissionsCount),
                    })
                .Select(j => new
                {
                    Id = j.MachineId,
                    Weight = j.Weights.Sum(w => w.Weight),
                    EmptyWeight = j.Weights.Sum(w => w.EmptyWeight),
                    LoadingUnitsCount = j.LoadingUnitsCount,
                    CompartmentsCount = j.CompartmentsCount,
                    MissionsCount = j.MissionsCount,
                    TotalMaxWeight = j.TotalMaxWeight,
                })
                .Select(x => new MachinesLoadingUnitsInfo
                {
                    Id = x.Id,
                    Weight = x.Weight,
                    EmptyWeight = x.EmptyWeight,
                    LoadingUnitsCount = x.LoadingUnitsCount,
                    CompartmentsCount = x.CompartmentsCount,
                    MissionsCount = x.MissionsCount,
                    WeightFillRate = x.TotalMaxWeight != null ? (int)(100 * x.Weight / x.TotalMaxWeight) : 0,
                });
        }

        private IQueryable<Machine> GetAllBase()
        {
            var machinesAreaOccupation = this.GetMachinesAreaOccupation();
            var machinesLoadingUnitsInfo = this.GetMachinesLoadingUnitsInfo().AsEnumerable();

            return this.DataContext.Machines
                .Join(
                    machinesAreaOccupation,
                    m => m.Id,
                    agg => agg.Id,
                    (m, agg) => new
                    {
                        Machine = m,
                        AreaOccupation = agg,
                    })
                .Join(
                    machinesLoadingUnitsInfo,
                    m => m.Machine.Id,
                    agg => agg.Id,
                    (m, agg) => new
                    {
                        m.Machine,
                        m.AreaOccupation,
                        LoadingUnitsInfo = agg,
                    })
                .Select(x => new Machine
                {
                    Id = x.Machine.Id,
                    AisleId = x.Machine.AisleId,
                    AisleName = x.Machine.Aisle.Name,
                    AreaName = x.Machine.Aisle.Area.Name,
                    AutomaticTime = x.Machine.AutomaticTime,
                    BuildDate = x.Machine.BuildDate,
                    CradlesCount = x.Machine.CradlesCount,
                    CustomerAddress = x.Machine.CustomerAddress,
                    CustomerCity = x.Machine.CustomerCity,
                    CustomerCountry = x.Machine.CustomerCountry,
                    CustomerCode = x.Machine.CustomerCode,
                    CustomerName = x.Machine.CustomerName,
                    ErrorTime = x.Machine.ErrorTime,
                    AreaFillRate = (int)x.AreaOccupation.Occupation,
                    GrossMaxWeight = x.Machine.TotalMaxWeight,
                    GrossWeight = x.LoadingUnitsInfo.Weight,
                    Image = x.Machine.Image,
                    InputLoadingUnitsCount = x.Machine.InputLoadingUnitsCount,
                    InstallationDate = x.Machine.InstallationDate,
                    LastPowerOn = x.Machine.LastPowerOn,
                    LastServiceDate = x.Machine.LastServiceDate,
                    Latitude = x.Machine.Latitude,
                    Longitude = x.Machine.Longitude,
                    LoadingUnitsPerCradle = x.Machine.LoadingUnitsPerCradle,
                    MachineTypeId = x.Machine.MachineTypeId,
                    MachineTypeDescription = x.Machine.MachineType.Description,
                    ManualTime = x.Machine.ManualTime,
                    MissionTime = x.Machine.MissionTime,
                    Model = x.Machine.Model,
                    MovedLoadingUnitsCount = x.Machine.MovedLoadingUnitsCount,
                    NetMaxWeight = x.Machine.TotalMaxWeight - x.LoadingUnitsInfo.EmptyWeight,
                    NetWeight = x.LoadingUnitsInfo.Weight - x.LoadingUnitsInfo.EmptyWeight,
                    NextServiceDate = x.Machine.NextServiceDate,
                    Nickname = x.Machine.Nickname,
                    OutputLoadingUnitsCount = x.Machine.OutputLoadingUnitsCount,
                    PowerOnTime = x.Machine.PowerOnTime,
                    RegistrationNumber = x.Machine.RegistrationNumber,
                    TestDate = x.Machine.TestDate,
                    TotalMaxWeight = x.Machine.TotalMaxWeight,
                    WeightFillRate = x.LoadingUnitsInfo.WeightFillRate,
                })
                .Select(m => GetMaintenanceStatus(m));
        }

        private IQueryable<MachineDetails> GetAllDetailsBase()
        {
            var machinesAreaOccupation = this.GetMachinesAreaOccupation();
            var machinesLoadingUnitsInfo = this.GetMachinesLoadingUnitsInfo();
            var machinesItems = this.GetMachinesItems();

            return this.DataContext.Machines
                .Join(
                    machinesAreaOccupation,
                    m => m.Id,
                    agg => agg.Id,
                    (m, agg) => new
                    {
                        Machine = m,
                        AreaOccupation = agg,
                    })
                .Join(
                    machinesLoadingUnitsInfo,
                    m => m.Machine.Id,
                    agg => agg.Id,
                    (m, agg) => new
                    {
                        m.Machine,
                        m.AreaOccupation,
                        LoadingUnitsInfo = agg,
                    })
                .Join(
                    machinesItems,
                    m => m.Machine.Id,
                    agg => agg.Id,
                    (m, agg) => new
                    {
                        m.Machine,
                        m.AreaOccupation,
                        m.LoadingUnitsInfo,
                        agg.ItemsCount,
                    })
                .Select(x => new MachineDetails
                {
                    AisleId = x.Machine.AisleId,
                    AisleName = x.Machine.Aisle.Name,
                    AreaFillRate = (int)x.AreaOccupation.Occupation,
                    AreaName = x.Machine.Aisle.Area.Name,
                    AutomaticTime = x.Machine.AutomaticTime,
                    BuildDate = x.Machine.BuildDate,
                    CellsCount = x.Machine.Aisle.Cells.Count(),
                    CompartmentsCount = x.LoadingUnitsInfo.CompartmentsCount,
                    CradlesCount = x.Machine.CradlesCount,
                    CustomerAddress = x.Machine.CustomerAddress,
                    CustomerCity = x.Machine.CustomerCity,
                    CustomerCode = x.Machine.CustomerCode,
                    CustomerCountry = x.Machine.CustomerCountry,
                    CustomerName = x.Machine.CustomerName,
                    ErrorTime = x.Machine.ErrorTime,
                    GrossMaxWeight = x.Machine.TotalMaxWeight,
                    GrossWeight = x.LoadingUnitsInfo.Weight,
                    Id = x.Machine.Id,
                    Image = x.Machine.Image,
                    InputLoadingUnitsCount = x.Machine.InputLoadingUnitsCount,
                    InstallationDate = x.Machine.InstallationDate,
                    ItemsCount = x.ItemsCount,
                    LastPowerOn = x.Machine.LastPowerOn,
                    LastServiceDate = x.Machine.LastServiceDate,
                    Latitude = x.Machine.Latitude,
                    LoadingUnitsCount = x.LoadingUnitsInfo.LoadingUnitsCount,
                    LoadingUnitsPerCradle = x.Machine.LoadingUnitsPerCradle,
                    Longitude = x.Machine.Longitude,
                    MachineTypeDescription = x.Machine.MachineType.Description,
                    MachineTypeId = x.Machine.MachineTypeId,
                    ManualTime = x.Machine.ManualTime,
                    MissionsCount = x.LoadingUnitsInfo.MissionsCount,
                    MissionTime = x.Machine.MissionTime,
                    Model = x.Machine.Model,
                    MovedLoadingUnitsCount = x.Machine.MovedLoadingUnitsCount,
                    NetMaxWeight = x.Machine.TotalMaxWeight - x.LoadingUnitsInfo.EmptyWeight,
                    NetWeight = x.LoadingUnitsInfo.Weight - x.LoadingUnitsInfo.EmptyWeight,
                    NextServiceDate = x.Machine.NextServiceDate,
                    Nickname = x.Machine.Nickname,
                    OutputLoadingUnitsCount = x.Machine.OutputLoadingUnitsCount,
                    PowerOnTime = x.Machine.PowerOnTime,
                    RegistrationNumber = x.Machine.RegistrationNumber,
                    ServiceUrl = x.Machine.ServiceUrl,
                    TestDate = x.Machine.TestDate,
                    TotalMaxWeight = x.Machine.TotalMaxWeight,
                    WeightFillRate = x.LoadingUnitsInfo.WeightFillRate,
                })
                .Select(m => GetMaintenanceStatus(m));
        }

        private TMachine MergeLiveData<TMachine>(TMachine machine)
            where TMachine : IMachineLiveData
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(machine.Id);

            machine.Status = machineStatus.Mode;

            return machine;
        }

        #endregion
    }
}
