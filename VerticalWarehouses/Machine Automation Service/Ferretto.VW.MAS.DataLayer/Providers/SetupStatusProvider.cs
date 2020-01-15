using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class SetupStatusProvider : ISetupStatusProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly DataLayerContext dataContext;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IVerticalOriginVolatileSetupStatusProvider verticalOriginSetupStatusProvider;

        #endregion

        #region Constructors

        public SetupStatusProvider(
            DataLayerContext dataContext,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IVerticalOriginVolatileSetupStatusProvider verticalOriginSetupStatusProvider,
            IConfiguration configuration)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.verticalOriginSetupStatusProvider = verticalOriginSetupStatusProvider ?? throw new ArgumentNullException(nameof(verticalOriginSetupStatusProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Methods

        public void CompleteVerticalOrigin()
        {
            this.verticalOriginSetupStatusProvider.Complete();

            lock (this.dataContext)
            {
                var procedureParameters = this.setupProceduresDataProvider.GetVerticalOriginCalibration();
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            }
        }

        public SetupStatusCapabilities Get()
        {
            var status = this.dataContext.SetupStatus.FirstOrDefault();

            var setup = this.setupProceduresDataProvider.GetAll();

            var verticalOrigin = setup.VerticalOriginCalibration;

            var statusCapabilities = new SetupStatusCapabilities
            {
                Bay1 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1HeightCheck,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && verticalOrigin.IsCompleted,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Laser,
                        CanBePerformed = true,
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Shape,
                        CanBePerformed = status.Bay1HeightCheck && verticalOrigin.IsCompleted,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1Shutter,
                        CanBePerformed = true,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay1HeightCheck && verticalOrigin.IsCompleted,
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay1HeightCheck && verticalOrigin.IsCompleted,
                    },
                },

                Bay2 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2HeightCheck,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && verticalOrigin.IsCompleted,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Laser,
                        CanBePerformed = true,
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Shape,
                        CanBePerformed = status.Bay2HeightCheck && verticalOrigin.IsCompleted,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay2Shutter,
                        CanBePerformed = true,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay2HeightCheck && verticalOrigin.IsCompleted,
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay2HeightCheck && verticalOrigin.IsCompleted,
                    },
                },

                Bay3 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3HeightCheck,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && verticalOrigin.IsCompleted,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Laser,
                        CanBePerformed = true,
                    },
                    Shape = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Shape,
                        CanBePerformed = status.Bay3HeightCheck && verticalOrigin.IsCompleted,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = status.Bay3Shutter,
                        CanBePerformed = true,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = status.Bay1FirstLoadingUnit || status.Bay2FirstLoadingUnit || status.Bay3FirstLoadingUnit,
                        CanBePerformed = status.WeightMeasurement && status.Bay3HeightCheck && verticalOrigin.IsCompleted,
                    },
                    AllLoadingUnits = new SetupStepStatus
                    {
                        IsCompleted = status.AllLoadingUnits,
                        CanBePerformed = status.WeightMeasurement && status.Bay3HeightCheck && verticalOrigin.IsCompleted,
                    },
                },
                BeltBurnishing = new SetupStepStatus
                {
                    IsCompleted = setup.BeltBurnishingTest.IsCompleted,
                    CanBePerformed = verticalOrigin.IsCompleted,
                    InProgress = setup.BeltBurnishingTest.InProgress,
                },
                CellsHeightCheck = new SetupStepStatus
                {
                    IsCompleted = setup.CellsHeightCheck.IsCompleted,
                    CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && verticalOrigin.IsCompleted,
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
                CellPanelsCheck = new SetupStepStatus
                {
                    IsCompleted = setup.CellPanelsCheck.IsCompleted,
                    CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && verticalOrigin.IsCompleted,
                },
                VerticalOriginCalibration = {
                    IsCompleted = setup.VerticalOriginCalibration.IsCompleted,
                },
                VerticalOffsetCalibration = new SetupStepStatus
                {
                    IsCompleted = setup.VerticalOffsetCalibration.IsCompleted,
                    CanBePerformed = setup.VerticalResolutionCalibration.IsCompleted && verticalOrigin.IsCompleted,
                },
                VerticalResolutionCalibration = new SetupStepStatus
                {
                    IsCompleted = setup.VerticalResolutionCalibration.IsCompleted,
                    CanBePerformed = setup.BeltBurnishingTest.IsCompleted && verticalOrigin.IsCompleted,
                },
                WeightMeasurement = new SetupStepStatus
                {
                    IsCompleted = status.WeightMeasurement,
                    CanBePerformed = true, // TODO this should be probably conditioned by the bay checks
                },
            };

            return statusCapabilities;
        }

        #endregion
    }
}
