using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionProvider : BaseProvider, IMissionProvider
    {
        #region Fields

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public MissionProvider(
            DatabaseContext dataContext,
            INotificationService notificationService,
            IMapper mapper)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<Mission>> CreateAsync(Mission model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var validationError = model.ValidateBusinessModel(this.DataContext.Missions);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new BadRequestOperationResult<Mission>(
                    validationError,
                    model);
            }

            var entry = await this.DataContext.Missions.AddAsync(
                this.mapper.Map<Common.DataModels.Mission>(model));

            this.NotificationService.PushCreate(model);

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<Mission>();
            }

            var createdModel = await this.GetByIdAsync(entry.Entity.Id);
            return new SuccessOperationResult<Mission>(createdModel);
        }

        public async Task<IOperationResult<IEnumerable<Mission>>> CreateRangeAsync(IEnumerable<Mission> models)
        {
            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            await this.DataContext.Missions.AddRangeAsync(
                this.mapper.Map<IEnumerable<Common.DataModels.Mission>>(models));

            foreach (var model in models)
            {
                this.NotificationService.PushCreate(model);
            }

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<IEnumerable<Mission>>();
            }

            return new SuccessOperationResult<IEnumerable<Mission>>(models);
        }

        public async Task<IEnumerable<MissionInfo>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var missions = await this.DataContext.Missions
                .ProjectTo<MissionInfo>(this.mapper.ConfigurationProvider)
                .ToArrayAsync<MissionInfo, Common.DataModels.Mission>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpressionInfo(searchString));

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
            return await this.DataContext.Missions
                .ProjectTo<MissionWithLoadingUnitDetails>(this.mapper.ConfigurationProvider)
                .CountAsync<MissionWithLoadingUnitDetails, Common.DataModels.Mission>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            var mission = await this.DataContext.Missions
                .Where(m => m.Id == id)
                .ProjectTo<Mission>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            SetPolicies(mission);

            return mission;
        }

        public async Task<IOperationResult<IEnumerable<MissionInfo>>> GetByMachineIdAsync(int id)
        {
            if (await this.DataContext.Machines.AnyAsync(m => m.Id == id) == false)
            {
                return new NotFoundOperationResult<IEnumerable<MissionInfo>>();
            }

            var missions = await this.DataContext.Missions
                .Join(
                    this.DataContext.Machines,
                    mission => mission.LoadingUnit.Cell.Aisle.Id,
                    machine => machine.Aisle.Id,
                    (mission, machine) => new { Mission = mission, Machine = machine })
                .Where(j => j.Machine.Id == id)
                .Select(j => j.Mission)
                .ProjectTo<MissionInfo>(this.mapper.ConfigurationProvider)
                .ToArrayAsync();

            foreach (var mission in missions)
            {
                SetPolicies(mission);
            }

            return new SuccessOperationResult<IEnumerable<MissionInfo>>(missions);
        }

        public async Task<IOperationResult<MissionWithLoadingUnitDetails>> GetDetailsByIdAsync(int id)
        {
            var missionDetails = await this.DataContext.Missions
                .Where(m => m.Id == id)
                .ProjectTo<MissionWithLoadingUnitDetails>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            if (missionDetails == null)
            {
                return new NotFoundOperationResult<MissionWithLoadingUnitDetails>();
            }

            SetPolicies(missionDetails);

            return new SuccessOperationResult<MissionWithLoadingUnitDetails>(missionDetails);
        }

        public async Task<MissionInfo> GetInfoByIdAsync(int id)
        {
            var mission = await this.DataContext.Missions
             .Where(m => m.Id == id)
             .ProjectTo<MissionInfo>(this.mapper.ConfigurationProvider)
             .SingleOrDefaultAsync();

            SetPolicies(mission);

            return mission;
        }

        public async Task<Mission> GetNewByLoadingUnitIdAsync(int loadingUnitId)
        {
            return await this.DataContext.Missions
                .Where(m =>
                    m.LoadingUnitId == loadingUnitId
                    &&
                    m.Operations.All(o => o.Status == Common.DataModels.MissionOperationStatus.New)
                    &&

                    // exclude loading unit missions (they do not have operations)
                    m.Operations.Any())
                .ProjectTo<Mission>(this.mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.DataContext.Missions,
                this.DataContext.Missions.ProjectTo<MissionWithLoadingUnitDetails>(this.mapper.ConfigurationProvider));
        }

        public async Task<IOperationResult<Mission>> UpdateAsync(Mission model)
        {
            var result = await this.UpdateAsync(
               model,
               this.DataContext.Missions,
               this.DataContext,
               false);

            this.NotificationService.PushUpdate(model);

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
                    "Major Code Smell",
                    "S4058:Overloads with a \"StringComparison\" parameter should be used",
                    Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<MissionWithLoadingUnitDetails, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (m) =>
                (m.BayDescription != null && m.BayDescription.Contains(search))
                || m.Status.ToString().Contains(search)
                || (successConversionAsInt && Equals(m.Priority, searchAsInt));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
          "Major Code Smell",
          "S4058:Overloads with a \"StringComparison\" parameter should be used",
          Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<MissionInfo, bool>> BuildSearchExpressionInfo(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (m) =>
                (m.BayDescription != null && m.BayDescription.Contains(search))
                || (m.LoadingUnitCode != null && m.LoadingUnitCode.Contains(search))
                || m.Status.ToString().Contains(search)
                || (successConversionAsInt && Equals(m.Priority, searchAsInt));
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

        #endregion
    }
}
