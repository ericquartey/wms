using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class SetupStatusProvider : ISetupStatusProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

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
            IBaysDataProvider baysDataProvider,
            IConfiguration configuration)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.verticalOriginSetupStatusProvider = verticalOriginSetupStatusProvider ?? throw new ArgumentNullException(nameof(verticalOriginSetupStatusProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
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

            var bays = this.baysDataProvider.GetAll();
            var bay1 = bays.Where(a => a.Number == CommonUtils.Messages.Enumerations.BayNumber.BayOne).First();
            var bay2 = bays.Where(a => a.Number == CommonUtils.Messages.Enumerations.BayNumber.BayTwo).FirstOrDefault() ?? new DataModels.Bay();
            var bay3 = bays.Where(a => a.Number == CommonUtils.Messages.Enumerations.BayNumber.BayThree).FirstOrDefault() ?? new DataModels.Bay();

            var bay1AllTestCompleted = false;
            var bay2AllTestCompleted = false;
            var bay3AllTestCompleted = false;

            if (!(bay1 is null))
            {
                bay1AllTestCompleted = (setup.Bay1CarouselCalibration.IsCompleted || bay1.Carousel is null) &&
                               (setup.Bay1ExternalCalibration.IsCompleted || !bay1.IsExternal) &&
                               (setup.Bay1ShutterTest.IsCompleted || bay1.Shutter is null) &&
                                setup.Bay1HeightCheck.IsCompleted &&
                               //(setup.Bay1Laser.IsCompleted || bay1.Accessories?.LaserPointer is null) &&
                               setup.Bay1FullTest.IsCompleted &&
                                setup.Bay1ProfileCheck.IsCompleted;
            }

            if (!(bay2 is null))
            {
                bay2AllTestCompleted = (setup.Bay2CarouselCalibration.IsCompleted || bay2.Carousel is null) &&
                               (setup.Bay2ExternalCalibration.IsCompleted || !bay2.IsExternal) &&
                               (setup.Bay2ShutterTest.IsCompleted || bay2.Shutter is null) &&
                                setup.Bay2HeightCheck.IsCompleted &&
                               //(setup.Bay2Laser.IsCompleted || bay2.Accessories?.LaserPointer is null) &&
                               setup.Bay2FullTest.IsCompleted &&
                                setup.Bay2ProfileCheck.IsCompleted;
            }

            if (!(bay3 is null))
            {
                bay3AllTestCompleted = (setup.Bay3CarouselCalibration.IsCompleted || bay3.Carousel is null) &&
                               (setup.Bay3ExternalCalibration.IsCompleted || !bay3.IsExternal) &&
                               (setup.Bay3ShutterTest.IsCompleted || bay3.Shutter is null) &&
                                setup.Bay3HeightCheck.IsCompleted &&
                               //(setup.Bay3Laser.IsCompleted || bay3.Accessories?.LaserPointer is null) &&
                               setup.Bay3FullTest.IsCompleted &&
                                setup.Bay3ProfileCheck.IsCompleted;
            }

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
                    //Laser = new SetupStepStatus
                    //{
                    //    IsCompleted = setup.Bay1Laser.IsCompleted,
                    //    CanBePerformed = true,
                    //    IsBypassed = setup.Bay1Laser.IsBypassed,
                    //},
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
                    CarouselCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1CarouselCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1CarouselCalibration.IsBypassed,
                    },
                    ExternalBayCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1ExternalCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay1ExternalCalibration.IsBypassed,
                    },
                    FullTest = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay1FullTest?.IsCompleted ?? false,
                        InProgress = setup.Bay1FullTest?.InProgress ?? false,
                        CanBePerformed = setup.Bay1FullTest.IsCompleted && setup.Bay1FullTest.IsCompleted,
                        IsBypassed = setup.Bay1FullTest?.IsBypassed ?? false,
                    },

                    IsAllTestCompleted = bay1AllTestCompleted,
                },
                Bay2 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2HeightCheck.IsCompleted,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay2HeightCheck.IsBypassed,
                    },
                    //Laser = new SetupStepStatus
                    //{
                    //    IsCompleted = setup.Bay2Laser.IsCompleted,
                    //    CanBePerformed = true,
                    //    IsBypassed = setup.Bay2Laser.IsBypassed,
                    //},
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
                    CarouselCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2CarouselCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay2CarouselCalibration.IsBypassed,
                    },
                    ExternalBayCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2ExternalCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay2ExternalCalibration.IsBypassed,
                    },
                    FullTest = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay2FullTest?.IsCompleted ?? false,
                        InProgress = setup.Bay2FullTest?.InProgress ?? false,
                        CanBePerformed = setup.Bay2FullTest.IsCompleted && setup.Bay1FullTest.IsCompleted,
                        IsBypassed = setup.Bay2FullTest?.IsBypassed ?? false,
                    },

                    IsAllTestCompleted = bay2AllTestCompleted,
                },
                Bay3 = new BaySetupStatus
                {
                    Check = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3HeightCheck.IsCompleted,
                        CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3HeightCheck.IsBypassed,
                    },
                    //Laser = new SetupStepStatus
                    //{
                    //    IsCompleted = setup.Bay3Laser.IsCompleted,
                    //    CanBePerformed = true,
                    //    IsBypassed = setup.Bay3Laser.IsBypassed,
                    //},
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
                    CarouselCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3CarouselCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3CarouselCalibration.IsBypassed,
                    },
                    ExternalBayCalibration = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3ExternalCalibration.IsCompleted,
                        CanBePerformed = setup.VerticalOriginCalibration.IsCompleted,
                        IsBypassed = setup.Bay3ExternalCalibration.IsBypassed,
                    },
                    FullTest = new SetupStepStatus
                    {
                        IsCompleted = setup.Bay3FullTest?.IsCompleted ?? false,
                        InProgress = setup.Bay3FullTest?.InProgress ?? false,
                        CanBePerformed = setup.Bay3FullTest.IsCompleted && setup.Bay1FullTest.IsCompleted,
                        IsBypassed = setup.Bay3FullTest?.IsBypassed ?? false,
                    },

                    IsAllTestCompleted = bay3AllTestCompleted,
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
                LoadFirstDrawerTest = new SetupStepStatus
                {
                    IsCompleted = setup.LoadFirstDrawerTest?.IsCompleted ?? false,
                    InProgress = setup.LoadFirstDrawerTest?.InProgress ?? false,
                    CanBePerformed = setup.VerticalOffsetCalibration.IsCompleted && setup.VerticalOriginCalibration.IsCompleted,
                    IsBypassed = setup.LoadFirstDrawerTest?.IsBypassed ?? false,
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
                HorizontalChainCalibration = new SetupStepStatus
                {
                    IsCompleted = setup.HorizontalChainCalibration.IsCompleted,
                    CanBePerformed = setup.VerticalOriginCalibration.IsCompleted, // todo define conditions
                    IsBypassed = setup.HorizontalChainCalibration.IsBypassed,
                },
                DepositAndPickUpTest = new SetupStepStatus
                {
                    IsCompleted = setup.DepositAndPickUpTest.IsCompleted,
                    CanBePerformed = setup.DepositAndPickUpTest.IsCompleted, // todo define conditions
                }
            };

            return statusCapabilities;
        }

        #endregion
    }
}
