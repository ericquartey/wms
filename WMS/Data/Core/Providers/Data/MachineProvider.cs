using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        #endregion

        #region Constructors

        public MachineProvider(DatabaseContext dataContext, IBayProvider bayProvider)
        {
            this.dataContext = dataContext;
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
            return await this.GetAllBase()
                .ToArrayAsync<Machine, Common.DataModels.Machine>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));
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

        public async Task<Machine> GetByBayIdAsync(int bayId)
        {
            var bay = await this.bayProvider.GetByIdAsync(bayId);
            return await this.GetAllBase()
                       .SingleOrDefaultAsync(i => i.Id == bay.MachineId);
        }

        public async Task<Machine> GetByIdAsync(int id)
        {
            return await this.GetAllBase()
                     .SingleOrDefaultAsync(i => i.Id == id);
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

        private static MachineStatus GetMachineStatus(Common.DataModels.IDataModel machine)
        {
            if (machine != null)
            {
                if (machine.Id == 1)
                {
                    return MachineStatus.Automatic;
                }
                else if (machine.Id == 2)
                {
                    return MachineStatus.Error;
                }
                else if (machine.Id == 3)
                {
                    return MachineStatus.Manual;
                }
                else
                {
                    return MachineStatus.Offline;
                }
            }

            return MachineStatus.NotSpecified;
        }

        private static MaintenanceStatus GetMaintenanceStatus(Common.DataModels.IDataModel machine)
        {
            if (machine != null)
            {
                if (machine.Id == 1)
                {
                    return MaintenanceStatus.Valid;
                }
                else if (machine.Id == 2)
                {
                    return MaintenanceStatus.Expiring;
                }
                else
                {
                    return MaintenanceStatus.Expired;
                }
            }

            return MaintenanceStatus.NotSpecified;
        }

        private IQueryable<Machine> GetAllBase()
        {
            return this.dataContext.Machines
                .Select(m => new Machine
                {
                    Id = m.Id,
                    ActualWeight = m.ActualWeight,
                    AisleId = m.AisleId,
                    AisleName = m.Aisle.Name,
                    AreaName = m.Aisle.Area.Name,
                    AutomaticTime = m.AutomaticTime,
                    BuildDate = m.BuildDate,
                    CradlesCount = m.CradlesCount,
                    CustomerAddress = m.CustomerAddress,
                    CustomerCity = m.CustomerCity,
                    CustomerCountry = m.CustomerCountry,
                    CustomerCode = m.CustomerCode,
                    CustomerName = m.CustomerName,
                    ErrorTime = m.ErrorTime,
                    FillRate = new Random().Next(100),
                    GrossMaxWeight = m.TotalMaxWeight,
                    GrossWeight = m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.Weight)),
                    Image = m.Image,
                    InputLoadingUnitsCount = m.InputLoadingUnitsCount,
                    InstallationDate = m.InstallationDate,
                    LastPowerOn = m.LastPowerOn,
                    LastServiceDate = m.LastServiceDate,
                    Latitude = m.Latitude,
                    Longitude = m.Longitude,
                    LoadingUnitsPerCradle = m.LoadingUnitsPerCradle,
                    MachineTypeId = m.MachineTypeId,
                    MachineTypeDescription = m.MachineType.Description,
                    MaintenanceStatus = GetMaintenanceStatus(m),
                    ManualTime = m.ManualTime,
                    MissionTime = m.MissionTime,
                    Model = m.Model,
                    MovedLoadingUnitsCount = m.MovedLoadingUnitsCount,
                    NetMaxWeight = m.TotalMaxWeight - m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight)),
                    NetWeight = m.Aisle.Cells.Sum(c => c.LoadingUnits.Sum(l => l.LoadingUnitType.EmptyWeight)),
                    NextServiceDate = m.NextServiceDate,
                    Nickname = m.Nickname,
                    OutputLoadingUnitsCount = m.OutputLoadingUnitsCount,
                    PowerOnTime = m.PowerOnTime,
                    RegistrationNumber = m.RegistrationNumber,
                    Status = GetMachineStatus(m),
                    TestDate = m.TestDate,
                    TotalMaxWeight = m.TotalMaxWeight
                });
        }

        #endregion
    }
}
