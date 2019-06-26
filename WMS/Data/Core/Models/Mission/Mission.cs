using System.Collections.Generic;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces.Policies;
using Newtonsoft.Json;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class Mission : BaseModel<int>, IMissionPolicy
    {
        #region Properties

        public int? BayId { get; set; }

        [JsonIgnore]
        public int CompletedOperationsCount { get; set; }

        [JsonIgnore]
        public int ErrorOperationsCount { get; set; }

        [JsonIgnore]
        public int ExecutingOperationsCount { get; set; }

        [JsonIgnore]
        public int IncompleteOperationsCount { get; set; }

        public int LoadingUnitId { get; set; }

        [JsonIgnore]
        public int NewOperationsCount { get; set; }

        public IEnumerable<MissionOperation> Operations { get; set; }

        [JsonIgnore]
        public int OperationsCount { get; private set; }

        [Positive]
        public int Priority { get; set; }

        public MissionStatus Status => GetStatus(
            this.OperationsCount,
            this.NewOperationsCount,
            this.ExecutingOperationsCount,
            this.CompletedOperationsCount,
            this.IncompleteOperationsCount,
            this.ErrorOperationsCount);

        #endregion

        #region Methods

        internal static MissionStatus GetStatus(
           int operationsCount,
           int newOperationsCount,
           int executingOperationsCount,
           int completedOperationsCount,
           int incompleteOperationsCount,
           int errorOperationsCount)
        {
            if (operationsCount == 0 || operationsCount == newOperationsCount)
            {
                return MissionStatus.New;
            }

            if (operationsCount == completedOperationsCount)
            {
                return MissionStatus.Completed;
            }

            if (errorOperationsCount > 0)
            {
                return MissionStatus.Error;
            }

            if (executingOperationsCount > 0)
            {
                return MissionStatus.Executing;
            }

            if (incompleteOperationsCount > 0)
            {
                return MissionStatus.Incomplete;
            }

            return MissionStatus.Executing;
        }

        #endregion
    }
}
