using System;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService
    {
        #region Methods

        public ConfigurationDataType CheckConfigurationValueType(GeneralInfo configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case GeneralInfo.Address:
                case GeneralInfo.Zip:
                case GeneralInfo.City:
                case GeneralInfo.ClientCode:
                case GeneralInfo.ClientName:
                case GeneralInfo.Country:
                case GeneralInfo.Latitude:
                case GeneralInfo.Longitude:
                case GeneralInfo.Model:
                case GeneralInfo.Order:
                case GeneralInfo.Province:
                case GeneralInfo.Serial:
                    returnValue = ConfigurationDataType.String;
                    break;

                case GeneralInfo.AlfaNumBay1:
                case GeneralInfo.AlfaNumBay2:
                case GeneralInfo.AlfaNumBay3:
                case GeneralInfo.LaserBay1:
                case GeneralInfo.LaserBay2:
                case GeneralInfo.LaserBay3:
                    returnValue = ConfigurationDataType.Boolean;
                    break;

                case GeneralInfo.Bay1Height1:
                case GeneralInfo.Bay1Height2:
                case GeneralInfo.Bay2Height1:
                case GeneralInfo.Bay2Height2:
                case GeneralInfo.Bay3Height1:
                case GeneralInfo.Bay3Height2:
                case GeneralInfo.Bay1Position1:
                case GeneralInfo.Bay1Position2:
                case GeneralInfo.Bay2Position1:
                case GeneralInfo.Bay2Position2:
                case GeneralInfo.Bay3Position1:
                case GeneralInfo.Bay3Position2:
                case GeneralInfo.Height:
                    returnValue = ConfigurationDataType.Float;
                    break;

                case GeneralInfo.Bay1Type:
                case GeneralInfo.Bay2Type:
                case GeneralInfo.Bay3Type:
                case GeneralInfo.Shutter1Type:
                case GeneralInfo.Shutter2Type:
                case GeneralInfo.Shutter3Type:
                case GeneralInfo.BaysQuantity:
                case GeneralInfo.Barrier1Height:
                case GeneralInfo.Barrier2Height:
                case GeneralInfo.Barrier3Height:
                case GeneralInfo.MaxGrossWeight:
                case GeneralInfo.MaxDrawerGrossWeight:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case GeneralInfo.InstallationDate:
                case GeneralInfo.ProductionDate:
                    returnValue = ConfigurationDataType.Date;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(SetupNetwork configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case SetupNetwork.MachineNumber:
                case SetupNetwork.Inverter1Port:
                case SetupNetwork.InverterIndexMaster:
                case SetupNetwork.InverterIndexChain:
                case SetupNetwork.InverterIndexBay1:
                case SetupNetwork.InverterIndexBay2:
                case SetupNetwork.InverterIndexBay3:
                case SetupNetwork.InverterIndexShutter1:
                case SetupNetwork.InverterIndexShutter2:
                case SetupNetwork.InverterIndexShutter3:
                case SetupNetwork.IOExpansion1Port:
                case SetupNetwork.IOExpansion2Port:
                case SetupNetwork.IOExpansion3Port:
                case SetupNetwork.SQLServerPort:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case SetupNetwork.WMS_ON:
                case SetupNetwork.IOExpansion1Installed:
                case SetupNetwork.IOExpansion2Installed:
                case SetupNetwork.IOExpansion3Installed:
                    returnValue = ConfigurationDataType.Boolean;
                    break;

                case SetupNetwork.AlfaNumBay1:
                case SetupNetwork.AlfaNumBay2:
                case SetupNetwork.AlfaNumBay3:
                case SetupNetwork.Inverter1:
                case SetupNetwork.IOExpansion1IPAddress:
                case SetupNetwork.IOExpansion2IPAddress:
                case SetupNetwork.IOExpansion3IPAddress:
                case SetupNetwork.LaserBay1:
                case SetupNetwork.LaserBay2:
                case SetupNetwork.LaserBay3:
                case SetupNetwork.PPC1MasterIPAddress:
                case SetupNetwork.PPC2SlaveIPAddress:
                case SetupNetwork.PPC3SlaveIPAddress:
                case SetupNetwork.SQLServerIPAddress:
                    returnValue = ConfigurationDataType.IPAddress;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(SetupStatus configurationValueEnum)
        {
            return ConfigurationDataType.Boolean;
        }

        public ConfigurationDataType CheckConfigurationValueType(VerticalAxis configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case VerticalAxis.HomingSearchDirection:
                case VerticalAxis.HomingExecuted:
                    returnValue = ConfigurationDataType.Boolean;
                    break;

                case VerticalAxis.HomingExitAcceleration:
                case VerticalAxis.HomingExitDeceleration:
                case VerticalAxis.HomingExitSpeed:
                case VerticalAxis.HomingSearchAcceleration:
                case VerticalAxis.HomingSearchDeceleration:
                case VerticalAxis.HomingSearchSpeed:
                case VerticalAxis.LowerBound:
                case VerticalAxis.MaxEmptyAcceleration:
                case VerticalAxis.MaxEmptyDeceleration:
                case VerticalAxis.MaxEmptySpeed:
                case VerticalAxis.MaxFullAcceleration:
                case VerticalAxis.MaxFullDeceleration:
                case VerticalAxis.MinFullSpeed:
                case VerticalAxis.Offset:
                case VerticalAxis.Resolution:
                case VerticalAxis.UpperBound:
                case VerticalAxis.TakingOffset:
                case VerticalAxis.DepositOffset:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(HorizontalAxis configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalAxis.HomingExecuted:
                    returnValue = ConfigurationDataType.Boolean;
                    break;

                case HorizontalAxis.AntiClockWiseRun:
                case HorizontalAxis.ClockWiseRun:
                case HorizontalAxis.MaxEmptyAcceleration:
                case HorizontalAxis.MaxEmptyDeceleration:
                case HorizontalAxis.MaxEmptySpeed:
                case HorizontalAxis.MaxFullSpeed:
                case HorizontalAxis.MaxFullAcceleration:
                case HorizontalAxis.MaxFullDeceleration:
                case HorizontalAxis.Offset:
                case HorizontalAxis.Resolution:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(HorizontalMovementLongerProfile configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementLongerProfile.TotalSteps:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case HorizontalMovementLongerProfile.MovementCorrection:
                case HorizontalMovementLongerProfile.P0Acceleration:
                case HorizontalMovementLongerProfile.P0Deceleration:
                case HorizontalMovementLongerProfile.P0Quote:
                case HorizontalMovementLongerProfile.P0SpeedV1:
                case HorizontalMovementLongerProfile.P1Acceleration:
                case HorizontalMovementLongerProfile.P1Deceleration:
                case HorizontalMovementLongerProfile.P1Quote:
                case HorizontalMovementLongerProfile.P1SpeedV2:
                case HorizontalMovementLongerProfile.P2Acceleration:
                case HorizontalMovementLongerProfile.P2Deceleration:
                case HorizontalMovementLongerProfile.P2Quote:
                case HorizontalMovementLongerProfile.P2SpeedV3:
                case HorizontalMovementLongerProfile.P3Acceleration:
                case HorizontalMovementLongerProfile.P3Deceleration:
                case HorizontalMovementLongerProfile.P3Quote:
                case HorizontalMovementLongerProfile.P3SpeedV4:
                case HorizontalMovementLongerProfile.P4Acceleration:
                case HorizontalMovementLongerProfile.P4Deceleration:
                case HorizontalMovementLongerProfile.P4Quote:
                case HorizontalMovementLongerProfile.P4SpeedV5:
                case HorizontalMovementLongerProfile.P5Acceleration:
                case HorizontalMovementLongerProfile.P5Deceleration:
                case HorizontalMovementLongerProfile.P5Quote:
                case HorizontalMovementLongerProfile.P5Speed:
                case HorizontalMovementLongerProfile.TotalMovement:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(HorizontalMovementShorterProfile configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementShorterProfile.TotalSteps:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case HorizontalMovementShorterProfile.MovementCorrection:
                case HorizontalMovementShorterProfile.P0Acceleration:
                case HorizontalMovementShorterProfile.P0Deceleration:
                case HorizontalMovementShorterProfile.P0Quote:
                case HorizontalMovementShorterProfile.P0SpeedV1:
                case HorizontalMovementShorterProfile.P1Acceleration:
                case HorizontalMovementShorterProfile.P1Deceleration:
                case HorizontalMovementShorterProfile.P1Quote:
                case HorizontalMovementShorterProfile.P1SpeedV2:
                case HorizontalMovementShorterProfile.P2Acceleration:
                case HorizontalMovementShorterProfile.P2Deceleration:
                case HorizontalMovementShorterProfile.P2Quote:
                case HorizontalMovementShorterProfile.P2SpeedV3:
                case HorizontalMovementShorterProfile.P3Acceleration:
                case HorizontalMovementShorterProfile.P3Deceleration:
                case HorizontalMovementShorterProfile.P3Quote:
                case HorizontalMovementShorterProfile.P3SpeedV4:
                case HorizontalMovementShorterProfile.P4Acceleration:
                case HorizontalMovementShorterProfile.P4Deceleration:
                case HorizontalMovementShorterProfile.P4Quote:
                case HorizontalMovementShorterProfile.P4SpeedV5:
                case HorizontalMovementShorterProfile.P5Acceleration:
                case HorizontalMovementShorterProfile.P5Deceleration:
                case HorizontalMovementShorterProfile.P5Quote:
                case HorizontalMovementShorterProfile.P5Speed:
                case HorizontalMovementShorterProfile.TotalMovement:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(VerticalManualMovements configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case VerticalManualMovements.FeedRate:
                case VerticalManualMovements.PositiveTargetDirection:
                case VerticalManualMovements.NegativeTargetDirection:
                case VerticalManualMovements.FeedRateAfterZero:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(HorizontalManualMovements configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalManualMovements.FeedRate:
                case HorizontalManualMovements.InitialTargetPosition:
                case HorizontalManualMovements.RecoveryTargetPosition:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(BeltBurnishing configurationValueEnum)
        {
            return ConfigurationDataType.Integer;
        }

        public ConfigurationDataType CheckConfigurationValueType(ResolutionCalibration configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        public ConfigurationDataType CheckConfigurationValueType(OffsetCalibration configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case OffsetCalibration.ReferenceCell:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case OffsetCalibration.FeedRate:
                case OffsetCalibration.StepValue:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(CellControl configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        public ConfigurationDataType CheckConfigurationValueType(PanelControl configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case PanelControl.FrontInitialReferenceCell:
                case PanelControl.FrontPanelQuantity:
                case PanelControl.BackInitialReferenceCell:
                case PanelControl.BackPanelQuantity:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case PanelControl.StepValue:
                case PanelControl.FeedRate:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(ShutterHeightControl configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        public ConfigurationDataType CheckConfigurationValueType(WeightControl configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        public ConfigurationDataType CheckConfigurationValueType(ShutterManualMovements configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        public ConfigurationDataType CheckConfigurationValueType(BayPositionControl configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        public ConfigurationDataType CheckConfigurationValueType(LoadFirstDrawer configurationValueEnum)
        {
            return ConfigurationDataType.Float;
        }

        private bool CheckConfigurationDataType(long parameter, ConfigurationCategory category, ConfigurationDataType type)
        {
            ConfigurationDataType actualParameterType;
            switch (category)
            {
                case ConfigurationCategory.Undefined:
                    {
                        throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
                    }
                case ConfigurationCategory.GeneralInfo:
                    {
                        actualParameterType = this.CheckConfigurationValueType((GeneralInfo)parameter);
                        break;
                    }
                case ConfigurationCategory.SetupNetwork:
                    {
                        actualParameterType = this.CheckConfigurationValueType((SetupNetwork)parameter);
                        break;
                    }
                case ConfigurationCategory.VerticalAxis:
                    {
                        actualParameterType = this.CheckConfigurationValueType((VerticalAxis)parameter);
                        break;
                    }
                case ConfigurationCategory.HorizontalAxis:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalAxis)parameter);
                        break;
                    }
                case ConfigurationCategory.HorizontalMovementLongerProfile:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalMovementLongerProfile)parameter);
                        break;
                    }
                case ConfigurationCategory.HorizontalMovementShorterProfile:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalMovementShorterProfile)parameter);
                        break;
                    }
                case ConfigurationCategory.VerticalManualMovements:
                    {
                        actualParameterType = this.CheckConfigurationValueType((VerticalManualMovements)parameter);
                        break;
                    }
                case ConfigurationCategory.HorizontalManualMovements:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalManualMovements)parameter);
                        break;
                    }
                case ConfigurationCategory.BeltBurnishing:
                    {
                        actualParameterType = this.CheckConfigurationValueType((BeltBurnishing)parameter);
                        break;
                    }
                case ConfigurationCategory.ResolutionCalibration:
                    {
                        actualParameterType = this.CheckConfigurationValueType((ResolutionCalibration)parameter);
                        break;
                    }
                case ConfigurationCategory.OffsetCalibration:
                    {
                        actualParameterType = this.CheckConfigurationValueType((OffsetCalibration)parameter);
                        break;
                    }
                case ConfigurationCategory.CellControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((CellControl)parameter);
                        break;
                    }
                case ConfigurationCategory.PanelControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((PanelControl)parameter);
                        break;
                    }
                case ConfigurationCategory.ShutterHeightControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((ShutterHeightControl)parameter);
                        break;
                    }
                case ConfigurationCategory.WeightControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((WeightControl)parameter);
                        break;
                    }
                case ConfigurationCategory.BayPositionControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((BayPositionControl)parameter);
                        break;
                    }
                case ConfigurationCategory.LoadFirstDrawer:
                    {
                        actualParameterType = this.CheckConfigurationValueType((LoadFirstDrawer)parameter);
                        break;
                    }
                case ConfigurationCategory.ShutterManualMovements:
                    {
                        actualParameterType = this.CheckConfigurationValueType((ShutterManualMovements)parameter);
                        break;
                    }
                default:
                    throw new ArgumentNullException();
            }

            return actualParameterType.Equals(type);
        }

        #endregion
    }
}
