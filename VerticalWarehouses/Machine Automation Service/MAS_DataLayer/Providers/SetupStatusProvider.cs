using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class SetupStatusProvider : ISetupStatusProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly IVerticalOriginSetupStatusProvider verticalOriginSetupStatusProvider;

        #endregion

        #region Constructors

        public SetupStatusProvider(
            DataLayerContext dataContext,
            IVerticalOriginSetupStatusProvider verticalOriginSetupStatusProvider)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (verticalOriginSetupStatusProvider == null)
            {
                throw new ArgumentNullException(nameof(verticalOriginSetupStatusProvider));
            }

            this.dataContext = dataContext;
            this.verticalOriginSetupStatusProvider = verticalOriginSetupStatusProvider;
        }

        #endregion

        #region Methods

        public void CompleteBeltBurnishing()
        {
            this.Update(s => s.BeltBurnishing = true);
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

            var statusCapabilities = new SetupStatusCapabilities
            {
                Bay1 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Check,
                        CanBePerformed = status.VerticalOffsetCalibration
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Laser,
                        CanBePerformed = true
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Shape,
                        CanBePerformed = status.Bay1Check
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Shutter,
                        CanBePerformed = true
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay1Check
                    },
                },

                Bay2 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Check,
                        CanBePerformed = status.VerticalOffsetCalibration
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Laser,
                        CanBePerformed = true
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Shape,
                        CanBePerformed = status.Bay2Check
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Shutter,
                        CanBePerformed = true
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay2Check
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay2Check
                    },
                },

                Bay3 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Check,
                        CanBePerformed = status.VerticalOffsetCalibration
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Laser,
                        CanBePerformed = true
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Shape,
                        CanBePerformed = status.Bay3Check
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Shutter,
                        CanBePerformed = true
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay3Check
                    },
                },

                BeltBurnishing = new SetupStepStatus
                {
                    IsCompleted = status.BeltBurnishing,
                    CanBePerformed = this.verticalOriginSetupStatusProvider.Get().IsCompleted
                },
                CellsHeightCheck = new SetupStepStatus
                {
                    IsCompleted = status.CellsHeightCheck,
                    CanBePerformed = status.VerticalOffsetCalibration
                },
                AllLoadingUnits = new SetupStepStatus
                {
                    IsCompleted = status.AllLoadingUnits,
                    CanBePerformed = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit
                },
                HorizontalHoming = new SetupStepStatus
                {
                    IsCompleted = status.HorizontalHoming,
                    CanBePerformed = status.WeightMeasurement
                },
                PanelsCheck = new SetupStepStatus
                {
                    IsCompleted = status.PanelsCheck,
                    CanBePerformed = status.VerticalOffsetCalibration
                },
                VerticalOriginCalibration = this.verticalOriginSetupStatusProvider.Get(),
                VerticalOffsetCalibration = new SetupStepStatus
                {
                    IsCompleted = status.VerticalOffsetCalibration,
                    CanBePerformed = status.VerticalResolution
                },
                VerticalResolution = new SetupStepStatus
                {
                    IsCompleted = status.VerticalResolution,
                    CanBePerformed = status.BeltBurnishing
                },
                WeightMeasurement = new SetupStepStatus
                {
                    IsCompleted = status.WeightMeasurement,
                    CanBePerformed = true // TODO this should be probably conditioned by the bay checks
                },
            };

            return statusCapabilities;
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
