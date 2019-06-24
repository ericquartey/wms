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

        private readonly ISchedulerRequestExecutionProvider requestProvider;

        #endregion

        #region Constructors

        public MissionLoadingUnitProvider(
            DatabaseContext dataContext,
            INotificationService notificationService,
            IMapper mapper,
            IMissionProvider missionProvider,
            ISchedulerRequestExecutionProvider requestProvider,
            ILogger<MissionLoadingUnitProvider> logger)
            : base(dataContext, notificationService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.missionProvider = missionProvider;
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

            mission.Status = MissionStatus.Incomplete;

            var updatedMission = await this.UpdateAsync(mission);
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

            mission.Status = MissionStatus.Completed;

            var updatedMission = await this.UpdateAsync(mission);
            if (updatedMission != null)
            {
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
                Priority = request.Priority.Value
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

        private async Task<Mission> UpdateAsync(Mission mission)
        {
            var missionDataModel = await this.DataContext.Missions
                .SingleOrDefaultAsync(m => m.Id == mission.Id);

            missionDataModel.Status = (Common.DataModels.MissionStatus)mission.Status;

            await this.DataContext.SaveChangesAsync();

            this.NotificationService.PushUpdate(mission);
            this.NotificationService.PushUpdate(new LoadingUnit { Id = mission.LoadingUnitId });

            return await this.GetByIdAsync(mission.Id);
        }

        #endregion
    }
}
