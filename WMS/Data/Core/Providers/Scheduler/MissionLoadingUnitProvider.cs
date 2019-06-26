using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Interfaces.Policies;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionLoadingUnitProvider : BaseProvider, IMissionLoadingUnitProvider
    {
        #region Fields

        private readonly ILogger<MissionLoadingUnitProvider> logger;

        private readonly IMapper mapper;

        private readonly IMissionProvider missionProvider;

        private readonly IMissionOperationProvider operationProvider;

        private readonly ISchedulerRequestExecutionProvider requestProvider;

        #endregion

        #region Constructors

        public MissionLoadingUnitProvider(
            DatabaseContext dataContext,
            INotificationService notificationService,
            IMapper mapper,
            IMissionProvider missionProvider,
            IMissionOperationProvider operationProvider,
            ISchedulerRequestExecutionProvider requestProvider,
            ILogger<MissionLoadingUnitProvider> logger)
            : base(dataContext, notificationService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.missionProvider = missionProvider;
            this.operationProvider = operationProvider;
            this.requestProvider = requestProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<Mission>> AbortAsync(int id)
        {
            var mission = await this.GetByIdAsync(id);
            if (!mission.CanExecuteOperation(nameof(MissionPolicy.Abort)))
            {
                return new BadRequestOperationResult<Mission>(
                    mission.GetCanExecuteOperationReason(nameof(MissionPolicy.Complete)),
                    mission);
            }

            var operation = mission.Operations.FirstOrDefault();

            if (operation == null)
            {
                return new UnprocessableEntityOperationResult<Mission>(Resources.Mission.UnableToAbortTheMissionBecauseItHasNoAssociatedOperations);
            }

            operation.Status = MissionOperationStatus.Incomplete;

            await this.operationProvider.UpdateAsync(operation);
            var updatedMission = await this.GetByIdAsync(id);
            if (updatedMission != null)
            {
                return new SuccessOperationResult<Mission>(updatedMission);
            }

            return new UnprocessableEntityOperationResult<Mission>();
        }

        public async Task<IOperationResult<Mission>> CompleteAsync(int id)
        {
            var mission = await this.GetByIdAsync(id);
            if (!mission.CanExecuteOperation(nameof(MissionPolicy.Complete)))
            {
                return new BadRequestOperationResult<Mission>(
                    mission.GetCanExecuteOperationReason(nameof(MissionPolicy.Complete)),
                    mission);
            }

            var operation = mission.Operations.FirstOrDefault();
            if (operation == null)
            {
                return new UnprocessableEntityOperationResult<Mission>(Resources.Mission.UnableToAbortTheMissionBecauseItHasNoAssociatedOperations);
            }

            operation.Status = MissionOperationStatus.Completed;

            await this.operationProvider.UpdateAsync(operation);
            var updatedMission = await this.GetByIdAsync(id);
            if (updatedMission != null)
            {
                this.NotificationService.PushUpdate(new LoadingUnit { Id = updatedMission.LoadingUnitId });
                this.NotificationService.PushUpdate(mission);
                return new SuccessOperationResult<Mission>(updatedMission);
            }

            return new UnprocessableEntityOperationResult<Mission>();
        }

        public async Task<Mission> CreateWithdrawalOperationAsync(LoadingUnitSchedulerRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var mission = new Mission
            {
                BayId = request.BayId,
                LoadingUnitId = request.LoadingUnitId,
                Priority = request.Priority.Value,
                Operations = new[]
                {
                    new MissionOperation
                    {
                        Type = MissionOperationType.Pick,
                        Priority = request.Priority.Value
                    }
                }
            };

            this.logger.LogWarning(
                $"Scheduler Request (id={request.Id}): generating withdrawal mission for Loading Unit (Id={request.LoadingUnitId}), BayId={mission.BayId}. ");

            request.Status = SchedulerRequestStatus.Completed;

            await this.requestProvider.UpdateAsync(request);

            await this.missionProvider.CreateAsync(mission);

            return mission;
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            var mission = await this.DataContext.Missions
                 .ProjectTo<Mission>(this.mapper.ConfigurationProvider)
                 .SingleOrDefaultAsync(m => m.Id == id);

            if (mission != null)
            {
                SetPolicies(mission);
            }

            return mission;
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
