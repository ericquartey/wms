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

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly DatabaseContext dataContext;

        private readonly IMachinesLiveDataContext liveMachinesDataContext;

        #endregion

        #region Constructors

        public MachineProvider(
            DatabaseContext dataContext,
            IMachinesLiveDataContext liveMachinesDataContext,
            IBayProvider bayProvider)
        {
            this.dataContext = dataContext;
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

            return this.MergeLiveData(machines);
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
            var machines = await this.dataContext.Machines
                .Select(m => new MachineServiceInfo
                {
                    Id = m.Id,
                    ServiceUrl = m.ServiceUrl,
                    Bays = m.Bays.Select(b => new Bay { Id = b.Id })
                })
                .ToArrayAsync();

            return new SuccessOperationResult<IEnumerable<MachineServiceInfo>>(machines);
        }

        public async Task<Machine> GetByBayIdAsync(int bayId)
        {
            var bay = await this.bayProvider.GetByIdAsync(bayId);
            var machine = await this.GetAllBase()
                       .SingleOrDefaultAsync(i => i.Id == bay.MachineId);

            return this.MergeLiveData(machine);
        }

        public async Task<MachineDetails> GetByIdAsync(int id)
        {
            var machine = await this.GetAllDetailsBase()
                     .SingleOrDefaultAsync(i => i.Id == id);

            return this.MergeLiveData(machine);
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.Machines,
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
                    && Equals(m.FillRate, searchAsInt));
        }

        private static Machine GetMaintenanceStatus(Machine machine)
        {
            if (machine != null)
            {
                if (machine.Id == 1)
                {
                    machine.MaintenanceStatus = MaintenanceStatus.Valid;
                }
                else if (machine.Id == 2)
                {
                    machine.MaintenanceStatus = MaintenanceStatus.Expiring;
                }
                else
                {
                    machine.MaintenanceStatus = MaintenanceStatus.Expired;
                }
            }

            return machine;
        }

        private IQueryable<Machine> GetAllBase()
        {
            return this.dataContext.Machines
                   .Join(
                         this.dataContext.Machines
                         .GroupJoin(
                                this.dataContext.LoadingUnits,
                                m => m.AisleId,
                                l => l.Cell.AisleId,
                                (m, l) => new
                                {
                                    MachineId = m.Id,
                                    LoadingUnit = l
                                })
                            .SelectMany(
                                ml => ml.LoadingUnit.DefaultIfEmpty(),
                                (m, l) => new
                                {
                                    MachineId = m.MachineId,
                                    LoadingUnit = l,
                                })
                            .GroupJoin(
                                this.dataContext.Compartments,
                                m => m.LoadingUnit.Id,
                                c => c.LoadingUnitId,
                                (m, c) => new
                                {
                                    MachineId = m.MachineId,
                                    LoadingUnit = m.LoadingUnit,
                                    Compartment = c
                                })
                            .SelectMany(
                                mc => mc.Compartment.DefaultIfEmpty(),
                                (m, c) => new
                                {
                                    MachineId = m.MachineId,
                                    LoadingUnitId = m.LoadingUnit.Id,
                                    LoadingUnitArea = m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Length * m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Width,
                                    CompartmentArea = (c != null ? c.CompartmentType.Width : 0) * (c != null ? c.CompartmentType.Height : 0),
                                })
                            .GroupBy(x => x.MachineId)
                            .Select(x => new
                            {
                                g = x.GroupBy(j => new { j.LoadingUnitId, j.LoadingUnitArea })
                                    .Select(a => new
                                    {
                                        MachineId = a.First().MachineId,
                                        LoadingUnitArea = a.First().LoadingUnitArea,
                                        CompartmentArea = a.Sum(y => y.CompartmentArea)
                                    })
                            })
                            .Select(x => new
                            {
                                MachineId = x.g.First().MachineId,
                                Occupation = x.g.Sum(y => y.CompartmentArea) / x.g.Sum(y => y.LoadingUnitArea) * 100
                            }),
                        m => m.Id,
                        agg => agg.MachineId,
                        (m, agg) => new
                        {
                            m,
                            agg,
                        })
                    .Select(x => new Machine
                    {
                        Id = x.m.Id,
                        ActualWeight = x.m.ActualWeight,
                        AisleId = x.m.AisleId,
                        AisleName = x.m.Aisle.Name,
                        AreaName = x.m.Aisle.Area.Name,
                        AutomaticTime = x.m.AutomaticTime,
                        BuildDate = x.m.BuildDate,
                        CradlesCount = x.m.CradlesCount,
                        CustomerAddress = x.m.CustomerAddress,
                        CustomerCity = x.m.CustomerCity,
                        CustomerCountry = x.m.CustomerCountry,
                        CustomerCode = x.m.CustomerCode,
                        CustomerName = x.m.CustomerName,
                        ErrorTime = x.m.ErrorTime,
                        FillRate = (int)x.agg.Occupation,
                        GrossMaxWeight = x.m.TotalMaxWeight,
                        GrossWeight = x.m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.Weight)),
                        Image = x.m.Image,
                        InputLoadingUnitsCount = x.m.InputLoadingUnitsCount,
                        InstallationDate = x.m.InstallationDate,
                        LastPowerOn = x.m.LastPowerOn,
                        LastServiceDate = x.m.LastServiceDate,
                        Latitude = x.m.Latitude,
                        Longitude = x.m.Longitude,
                        LoadingUnitsPerCradle = x.m.LoadingUnitsPerCradle,
                        MachineTypeId = x.m.MachineTypeId,
                        MachineTypeDescription = x.m.MachineType.Description,
                        ManualTime = x.m.ManualTime,
                        MissionTime = x.m.MissionTime,
                        Model = x.m.Model,
                        MovedLoadingUnitsCount = x.m.MovedLoadingUnitsCount,
                        NetMaxWeight = x.m.TotalMaxWeight - x.m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight)),
                        NetWeight = x.m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.Weight - l.LoadingUnitType.EmptyWeight)),
                        NextServiceDate = x.m.NextServiceDate,
                        Nickname = x.m.Nickname,
                        OutputLoadingUnitsCount = x.m.OutputLoadingUnitsCount,
                        PowerOnTime = x.m.PowerOnTime,
                        RegistrationNumber = x.m.RegistrationNumber,
                        TestDate = x.m.TestDate,
                        TotalMaxWeight = x.m.TotalMaxWeight
                    })
                    .Select(m => GetMaintenanceStatus(m));
        }

        private IQueryable<MachineDetails> GetAllDetailsBase(
                    Expression<Func<Common.DataModels.Machine, bool>> whereExpression = null,
            Expression<Func<Common.DataModels.Machine, bool>> searchExpression = null)
        {
            var actualWhereFunc = whereExpression ?? ((i) => true);
            var actualSearchFunc = searchExpression ?? ((i) => true);

            return this.dataContext.Machines
                .Where(actualWhereFunc)
                .Where(actualSearchFunc)
                    .Join(
                          this.dataContext.Machines
                           .Where(actualWhereFunc)
                            .Where(actualSearchFunc)
                          .GroupJoin(
                                 this.dataContext.LoadingUnits,
                                 m => m.AisleId,
                                 l => l.Cell.AisleId,
                                 (m, l) => new
                                 {
                                     MachineId = m.Id,
                                     LoadingUnit = l
                                 })
                             .SelectMany(
                                 ml => ml.LoadingUnit.DefaultIfEmpty(),
                                 (m, l) => new
                                 {
                                     MachineId = m.MachineId,
                                     LoadingUnit = l,
                                 })
                             .GroupJoin(
                                 this.dataContext.Compartments,
                                 m => m.LoadingUnit.Id,
                                 c => c.LoadingUnitId,
                                 (m, c) => new
                                 {
                                     MachineId = m.MachineId,
                                     LoadingUnit = m.LoadingUnit,
                                     Compartment = c
                                 })
                             .SelectMany(
                                 mc => mc.Compartment.DefaultIfEmpty(),
                                 (m, c) => new
                                 {
                                     MachineId = m.MachineId,
                                     LoadingUnitId = m.LoadingUnit.Id,
                                     LoadingUnitArea = m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Length * m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Width,
                                     CompartmentArea = (c != null ? c.CompartmentType.Width : 0) * (c != null ? c.CompartmentType.Height : 0),
                                 })
                             .GroupBy(x => x.MachineId)
                             .Select(x => new
                             {
                                 g = x.GroupBy(j => new { j.LoadingUnitId, j.LoadingUnitArea })
                                     .Select(a => new
                                     {
                                         MachineId = a.First().MachineId,
                                         LoadingUnitArea = a.First().LoadingUnitArea,
                                         CompartmentArea = a.Sum(y => y.CompartmentArea)
                                     })
                             })
                             .Select(x => new
                             {
                                 MachineId = x.g.First().MachineId,
                                 Occupation = x.g.Sum(y => y.CompartmentArea) / x.g.Sum(y => y.LoadingUnitArea) * 100
                             }),
                         m => m.Id,
                         agg => agg.MachineId,
                         (m, agg) => new
                         {
                             m,
                             agg,
                         })
                 .Select(x => new MachineDetails
                 {
                     Id = x.m.Id,
                     ActualWeight = x.m.ActualWeight,
                     AisleId = x.m.AisleId,
                     AisleName = x.m.Aisle.Name,
                     AreaName = x.m.Aisle.Area.Name,
                     AutomaticTime = x.m.AutomaticTime,
                     BuildDate = x.m.BuildDate,
                     CradlesCount = x.m.CradlesCount,
                     CustomerAddress = x.m.CustomerAddress,
                     CustomerCity = x.m.CustomerCity,
                     CustomerCountry = x.m.CustomerCountry,
                     CustomerCode = x.m.CustomerCode,
                     CustomerName = x.m.CustomerName,
                     ErrorTime = x.m.ErrorTime,
                     AreaFillRate = (int)x.agg.Occupation,
                     GrossMaxWeight = x.m.TotalMaxWeight,
                     GrossWeight = x.m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.Weight)),
                     Image = x.m.Image,
                     InputLoadingUnitsCount = x.m.InputLoadingUnitsCount,
                     InstallationDate = x.m.InstallationDate,
                     LastPowerOn = x.m.LastPowerOn,
                     LastServiceDate = x.m.LastServiceDate,
                     Latitude = x.m.Latitude,
                     Longitude = x.m.Longitude,
                     LoadingUnitsPerCradle = x.m.LoadingUnitsPerCradle,
                     MachineTypeId = x.m.MachineTypeId,
                     MachineTypeDescription = x.m.MachineType.Description,
                     MaintenanceStatus = GetMaintenanceStatus(x.m),
                     ManualTime = x.m.ManualTime,
                     MissionTime = x.m.MissionTime,
                     Model = x.m.Model,
                     MovedLoadingUnitsCount = x.m.MovedLoadingUnitsCount,
                     NetMaxWeight = x.m.TotalMaxWeight - x.m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight)),
                     NetWeight = x.m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight)),
                     NextServiceDate = x.m.NextServiceDate,
                     Nickname = x.m.Nickname,
                     OutputLoadingUnitsCount = x.m.OutputLoadingUnitsCount,
                     PowerOnTime = x.m.PowerOnTime,
                     RegistrationNumber = x.m.RegistrationNumber,
                     TestDate = x.m.TestDate,
                     TotalMaxWeight = x.m.TotalMaxWeight,
                     UrlService = new Uri(x.m.ServiceUrl),
                     CellCount = x.m.Aisle.Cells.Count(),

                     // TODO: to be calculated
                     // LoadingUnitCOunt
                     // CellCOunt
                     // ItemUnitCOunt
                     // MissionCOunt
                     // ListCOunt
                     // CompartmentCOunt
                 });
        }

        private TMachine MergeLiveData<TMachine>(TMachine machine)
            where TMachine : IMachineLiveData
        {
            var machineStatus = this.liveMachinesDataContext.GetMachineStatus(machine.Id);

            machine.Status = (Models.MachineStatus)machineStatus.Mode;

            return machine;
        }

        private IEnumerable<Machine> MergeLiveData(IEnumerable<Machine> machines)
        {
            foreach (var machine in machines)
            {
                this.MergeLiveData(machine);
            }

            return machines;
        }

        #endregion
    }
}
