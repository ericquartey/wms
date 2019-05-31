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
using Ferretto.WMS.Data.Core.Interfaces.Policies;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionProvider : IMissionProvider
    {
        #region Fields

        private static readonly Expression<Func<Common.DataModels.Mission, Mission>> SelectMission = (m) => new Mission
        {
            BayDescription = m.Bay.Description,
            BayId = m.BayId,
            CellAisleName = m.Cell.Aisle.Name,
            CellId = m.CellId,
            CompartmentId = m.CompartmentId,
            CompartmentTypeWidth = m.Compartment.CompartmentType.Width,
            CompartmentTypeHeight = m.Compartment.CompartmentType.Height,
            CreationDate = m.CreationDate,
            Id = m.Id,
            ItemDescription = m.Item.Description,
            ItemId = m.ItemId,
            ItemListDescription = m.ItemList.Description,
            ItemListId = m.ItemListId,
            ItemListRowCode = m.ItemListRow.Code,
            ItemListRowId = m.ItemListRowId,
            ItemMeasureUnitDescription = m.Item.MeasureUnit.Description,
            LastModificationDate = m.LastModificationDate,
            LoadingUnitCode = m.LoadingUnit.Code,
            LoadingUnitId = m.LoadingUnitId,
            Lot = m.Lot,
            MaterialStatusDescription = m.MaterialStatus.Description,
            MaterialStatusId = m.MaterialStatusId,
            PackageTypeDescription = m.PackageType.Description,
            PackageTypeId = m.PackageTypeId,
            Priority = m.Priority,
            DispatchedQuantity = m.DispatchedQuantity,
            RegistrationNumber = m.RegistrationNumber,
            RequestedQuantity = m.RequestedQuantity,
            Status = (MissionStatus)m.Status,
            Sub1 = m.Sub1,
            Sub2 = m.Sub2,
            Type = (MissionType)m.Type,
        };

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public MissionProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var missions = await this.GetAllBase()
                .ToArrayAsync<Mission, Common.DataModels.Mission>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var mission in missions)
            {
                SetPolicies(mission);
            }

            return missions;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Mission, Common.DataModels.Mission>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            var mission = await this.GetAllBase()
                       .SingleOrDefaultAsync(m => m.Id == id);

            SetPolicies(mission);

            return mission;
        }

        public async Task<IOperationResult<IEnumerable<Mission>>> GetByMachineIdAsync(int id)
        {
            if (await this.dataContext.Machines.AnyAsync(m => m.Id == id) == false)
            {
                return new NotFoundOperationResult<IEnumerable<Mission>>();
            }

            var missions = await this.dataContext.Missions
                .Join(
                    this.dataContext.Machines,
                    mission => mission.Compartment.LoadingUnit.Cell.Aisle.Id,
                    machine => machine.Aisle.Id,
                    (mission, machine) => new { Mission = mission, Machine = machine })
                .Where(j => j.Machine.Id == id)
                .Select(j => j.Mission)
                .Select(SelectMission)
                .ToArrayAsync();

            foreach (var mission in missions)
            {
                SetPolicies(mission);
            }

            return new SuccessOperationResult<IEnumerable<Mission>>(missions);
        }

        public async Task<IOperationResult<MissionDetails>> GetDetailsByIdAsync(int id)
        {
            var missionDetails = await this.dataContext.Missions
                .Where(m => m.Id == id)
                .Select(m => new MissionDetails
                {
                    BayId = m.BayId,
                    CompartmentId = m.CompartmentId,
                    Id = m.Id,
                    Lot = m.Lot,
                    MaterialStatusDescription = m.MaterialStatus.Description,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeDescription = m.PackageType.Description,
                    PackageTypeId = m.PackageTypeId,
                    Priority = m.Priority,
                    RegistrationNumber = m.RegistrationNumber,
                    RequestedQuantity = m.RequestedQuantity,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type,
                    Item = new ItemMissionInfo
                    {
                        Id = m.Item.Id,
                        Code = m.Item.Code,
                        Description = m.Item.Description,
                        Image = m.Item.Image
                    },
                    LoadingUnit = new LoadingUnitMissionInfo
                    {
                        Id = m.LoadingUnit.Id,
                        Width = m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Width,
                        Length = m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Length,
                        Compartments = m.LoadingUnit.Compartments.Select(c => new CompartmentMissionInfo
                        {
                            Id = c.Id,
                            Width = c.HasRotation ? c.CompartmentType.Height : c.CompartmentType.Width,
                            Height = c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Height,
                            Stock = c.Stock,
                            MaxCapacity = c.ItemId.HasValue ? c.CompartmentType.ItemsCompartmentTypes.SingleOrDefault(ict => ict.ItemId == c.ItemId).MaxCapacity : null,
                        })
                    },
                    ItemList = new ItemListMissionInfo
                    {
                        Id = m.ItemList.Id,
                        Code = m.ItemList.Code,
                        Description = m.ItemList.Description
                    },
                    ItemListRow = new ItemListRowMissionInfo
                    {
                        Id = m.ItemListRow.Id,
                        Code = m.ItemListRow.Code
                    },
                })
                .SingleOrDefaultAsync();

            if (missionDetails == null)
            {
                return new NotFoundOperationResult<MissionDetails>();
            }

            SetPolicies(missionDetails);

            return new SuccessOperationResult<MissionDetails>(missionDetails);
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.Missions,
                       this.GetAllBase());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<Mission, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);
            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (m) =>
                (m.BayDescription != null && m.BayDescription.Contains(search))
                || (m.ItemDescription != null && m.ItemDescription.Contains(search))
                || (m.ItemListDescription != null && m.ItemListDescription.Contains(search))
                || (m.ItemListRowCode != null && m.ItemListRowCode.Contains(search))
                || (m.LoadingUnitCode != null && m.LoadingUnitCode.Contains(search))
                || m.Type.ToString().Contains(search)
                || m.Status.ToString().Contains(search)
                || (successConversionAsInt
                    && Equals(m.Priority, searchAsInt))
                || (successConversionAsDouble
                    && Equals(m.RequestedQuantity, searchAsDouble));
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            if (model is IMissionPolicy mission)
            {
                model.AddPolicy(mission.ComputeAbortPolicy());
                model.AddPolicy(mission.ComputeCompletePolicy());
                model.AddPolicy(mission.ComputeExecutePolicy());
            }
        }

        private IQueryable<Mission> GetAllBase()
        {
            return this.dataContext.Missions.Select(SelectMission);
        }

        #endregion
    }
}
