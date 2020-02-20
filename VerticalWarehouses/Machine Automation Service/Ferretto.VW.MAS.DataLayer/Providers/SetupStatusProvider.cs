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
            var setup = this.setupProceduresDataProvider.GetAll();

            var statusCapabilities = new SetupStatusCapabilities
            {
                Bay1 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1HeightCheck.IsCompleted,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1HeightCheck.IsBypassed,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1Laser.IsCompleted,
                        CanBePerformed = true,
                        IsBypassed = setup.Bay1Laser.IsBypassed,
                    },
                    Profile = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1ProfileCheck.IsCompleted,
                        CanBePerformed = setup.Bay1HeightCheck.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1ProfileCheck.IsBypassed,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1ShutterTest.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        InProgress = setup.Bay1ShutterTest.InProgress,
                        IsBypassed = setup.Bay1ShutterTest.IsBypassed,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1FirstLoadingUnit.IsCompleted || setup.Bay2FirstLoadingUnit.IsCompleted || setup.Bay3FirstLoadingUnit.IsCompleted,
                        CanBePerformed = setup.WeightMeasurement.IsCompleted && setup.Bay1HeightCheck.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1FirstLoadingUnit.IsBypassed,
                    },
                    CarouselCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1CarouselCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1CarouselCalibration.IsBypassed,
                    },
                },

                Bay2 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2HeightCheck.IsCompleted,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay2HeightCheck.IsBypassed,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2Laser.IsCompleted,
                        CanBePerformed = true,
                        IsBypassed = setup.Bay2Laser.IsBypassed,
                    },
                    Profile = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2ProfileCheck.IsCompleted,
                        CanBePerformed = setup.Bay2HeightCheck.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay2ProfileCheck.IsBypassed,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2ShutterTest.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        InProgress = setup.Bay2ShutterTest.InProgress,
                        IsBypassed = setup.Bay2ShutterTest.IsBypassed,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1FirstLoadingUnit.IsCompleted || setup.Bay2FirstLoadingUnit.IsCompleted || setup.Bay3FirstLoadingUnit.IsCompleted,
                        CanBePerformed = setup.WeightMeasurement.IsCompleted && setup.Bay2HeightCheck.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1FirstLoadingUnit.IsBypassed,
                    },
                    CarouselCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2CarouselCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay2CarouselCalibration.IsBypassed,
                    },
                },

                Bay3 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3HeightCheck.IsCompleted,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3HeightCheck.IsBypassed,
                    },
                    Laser = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3Laser.IsCompleted,
                        CanBePerformed = true,
                        IsBypassed = setup.Bay3Laser.IsBypassed,
                    },
                    Profile = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3ProfileCheck.IsCompleted,
                        CanBePerformed = setup.Bay3HeightCheck.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3ProfileCheck.IsBypassed,
                    },
                    Shutter = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3ShutterTest.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        InProgress = setup.Bay3ShutterTest.InProgress,
                        IsBypassed = setup.Bay3ShutterTest.IsBypassed,
                    },
                    FirstLoadingUnit = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1FirstLoadingUnit.IsCompleted || setup.Bay2FirstLoadingUnit.IsCompleted || setup.Bay3FirstLoadingUnit.IsCompleted,
                        CanBePerformed = setup.WeightMeasurement.IsCompleted && setup.Bay3HeightCheck.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3FirstLoadingUnit.IsBypassed,
                    },
                    CarouselCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3CarouselCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3CarouselCalibration.IsBypassed,
                    },
                },
                BeltBurnishing = new SetupStepStatus
                {
                    IsCompleted = setup.BeltBurnishingTest.IsCompleted,
                    CanBePerformed = setup.VerticalOriginCalibration.IsCompleted && setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                    InProgress = setup.BeltBurnishingTest.InProgress,
                    IsBypassed = setup.BeltBurnishingTest.IsBypassed,
                },
                CellsHeightCheck = new SetupStepStatus
                {
                    IsCompleted = setup.CellsHeightCheck.IsCompleted,
                    CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                    IsBypassed = setup.CellsHeightCheck.IsBypassed,
                },
                CellPanelsCheck = new SetupStepStatus
                {
                    IsCompleted = setup.CellPanelsCheck.IsCompleted,
                    InProgress = setup.CellPanelsCheck.InProgress,
                    CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                    IsBypassed = setup.CellPanelsCheck.IsBypassed,
                },
                VerticalOriginCalibration = new SetupStepStatus
                {
                    IsCompleted = setup.VerticalOriginCalibration.IsCompleted,
                    IsBypassed = setup.VerticalOriginCalibration.IsBypassed,
                },
                VerticalOffsetCalibration = new SetupStepStatus
                {
                    IsCompleted = setup.VerticalOffsetCalibration.IsCompleted,
                    CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                    IsBypassed = setup.VerticalOffsetCalibration.IsBypassed,
                },
                VerticalResolutionCalibration = new SetupStepStatus
                {
                    IsCompleted = setup.VerticalResolutionCalibration.IsCompleted,
                    CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                    IsBypassed = setup.VerticalResolutionCalibration.IsBypassed,
                },
                WeightMeasurement = new SetupStepStatus
                {
                    IsCompleted = setup.WeightMeasurement.IsCompleted,
                    CanBePerformed = true, // TODO this should be probably conditioned by the bay checks
                    IsBypassed = setup.WeightMeasurement.IsBypassed,
                },
            };

            return statusCapabilities;
        }

        #endregion
    }
}
