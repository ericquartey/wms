using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Extensions;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class SetupStatusProvider : ISetupStatusProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly DataLayerContext dataContext;

        private readonly IVerticalOriginVolatileSetupStatusProvider verticalOriginSetupStatusProvider;

        #endregion

        #region Constructors

        public SetupStatusProvider(
            DataLayerContext dataContext,
            IVerticalOriginVolatileSetupStatusProvider verticalOriginSetupStatusProvider,
            IConfiguration configuration)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (verticalOriginSetupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(verticalOriginSetupStatusProvider));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            this.dataContext = dataContext;
            this.verticalOriginSetupStatusProvider = verticalOriginSetupStatusProvider;
            this.configuration = configuration;
        }

        #endregion

        #region Methods

        public void CompleteBeltBurnishing()
        {
            this.Update(s => s.BeltBurnishingCompleted = true);
        }

        public void CompleteVerticalOffset()
        {
            this.Update(s => s.VerticalOffsetCalibration = true);
        }

        public void CompleteVerticalOrigin()
        {
            this.verticalOriginSetupStatusProvider.Complete();
        }

        public void CompleteVerticalResolution()
        {
            this.Update(s => s.VerticalResolution = true);
        }

        public SetupStatusCapabilities Get()
        {
            var status = this.dataContext.SetupStatus.FirstOrDefault();

            var verticalOrigin = this.verticalOriginSetupStatusProvider.Get();

            var statusCapabilities = new SetupStatusCapabilities
            {
                Bay1 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Check,
                        CanBePerformed = status.VerticalOffsetCalibration && verticalOrigin.IsCompleted,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Laser,
                        CanBePerformed = true,
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Shape,
                        CanBePerformed = status.Bay1Check && verticalOrigin.IsCompleted,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Shutter,
                        CanBePerformed = true,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay1Check && verticalOrigin.IsCompleted,
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay1Check && verticalOrigin.IsCompleted,
                    },
                },

                Bay2 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Check,
                        CanBePerformed = status.VerticalOffsetCalibration && verticalOrigin.IsCompleted,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Laser,
                        CanBePerformed = true,
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Shape,
                        CanBePerformed = status.Bay2Check && verticalOrigin.IsCompleted,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Shutter,
                        CanBePerformed = true,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay2Check && verticalOrigin.IsCompleted,
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay2Check && verticalOrigin.IsCompleted,
                    },
                },

                Bay3 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Check,
                        CanBePerformed = status.VerticalOffsetCalibration && verticalOrigin.IsCompleted,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Laser,
                        CanBePerformed = true,
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Shape,
                        CanBePerformed = status.Bay3Check && verticalOrigin.IsCompleted,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Shutter,
                        CanBePerformed = true,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay3Check && verticalOrigin.IsCompleted,
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay3Check && verticalOrigin.IsCompleted,
                    },
                },

                BeltBurnishing = new BeltBurnishingSetupStepStatus
                {
                    CompletedCycles = status.BeltBurnishingCompletedCycles,
                    IsCompleted = status.BeltBurnishingCompleted,
                    CanBePerformed = verticalOrigin.IsCompleted,
                },
                CellsHeightCheck = new SetupStepStatus
                {
                    IsCompleted = status.CellsHeightCheck,
                    CanBePerformed = status.VerticalOffsetCalibration && verticalOrigin.IsCompleted,
                },
                AllLoadingUnits = new SetupStepStatus
                {
                    IsCompleted = status.AllLoadingUnits,
                    CanBePerformed = (status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit) && verticalOrigin.IsCompleted,
                },
                HorizontalHoming = new SetupStepStatus
                {
                    IsCompleted = status.HorizontalHoming,
                    CanBePerformed = status.WeightMeasurement && verticalOrigin.IsCompleted,
                },
                PanelsCheck = new SetupStepStatus
                {
                    IsCompleted = status.PanelsCheck,
                    CanBePerformed = status.VerticalOffsetCalibration && verticalOrigin.IsCompleted,
                },
                VerticalOriginCalibration = verticalOrigin,
                VerticalOffsetCalibration = new SetupStepStatus
                {
                    IsCompleted = status.VerticalOffsetCalibration,
                    CanBePerformed = status.VerticalResolution && verticalOrigin.IsCompleted,
                },
                VerticalResolution = new SetupStepStatus
                {
                    IsCompleted = status.VerticalResolution,
                    CanBePerformed = status.BeltBurnishingCompleted && verticalOrigin.IsCompleted,
                },
                WeightMeasurement = new SetupStepStatus
                {
                    IsCompleted = status.WeightMeasurement,
                    CanBePerformed = true, // TODO this should be probably conditioned by the bay checks
                },
            };

            if (this.configuration.IsSetupStatusOverridden())
            {
                var debugStatus = SetupStatusCapabilities.Complete;
                debugStatus.BeltBurnishing.CompletedCycles = status.BeltBurnishingCompletedCycles;
                return debugStatus;
            }

            return statusCapabilities;
        }

        public void IncreaseBeltBurnishingCycle()
        {
            this.Update(s => s.BeltBurnishingCompletedCycles++);
        }

        public void ResetBeltBurnishing()
        {
            this.Update(s =>
            {
                s.BeltBurnishingCompletedCycles = 0;
                s.BeltBurnishingCompleted = true;
            });
        }

        private void Update(Action<SetupStatus> updateAction)
        {
            var setupStatus = this.dataContext.SetupStatus.FirstOrDefault();

            updateAction(setupStatus);

            this.dataContext.SetupStatus.Update(setupStatus);

            this.dataContext.SaveChanges();
        }

        #endregion
    }
}
