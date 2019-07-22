using System;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer
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
                case GeneralInfo.MaxGrossWeight:
                    returnValue = ConfigurationDataType.Float;
                    break;

                case GeneralInfo.Bay1Type:
                case GeneralInfo.Bay2Type:
                case GeneralInfo.Bay3Type:
                case GeneralInfo.Shutter1Type:
                case GeneralInfo.Shutter2Type:
                case GeneralInfo.Shutter3Type:
                case GeneralInfo.DrawersQuantity:
                case GeneralInfo.BaysQuantity:
                case GeneralInfo.Barrier1Height:
                case GeneralInfo.Barrier2Height:
                case GeneralInfo.Barrier3Height:
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
                    returnValue = ConfigurationDataType.Boolean;
                    break;

                case SetupNetwork.AlfaNumBay1:
                case SetupNetwork.AlfaNumBay2:
                case SetupNetwork.AlfaNumBay3:
                case SetupNetwork.Inverter1:
                case SetupNetwork.IOExpansion1:
                case SetupNetwork.IOExpansion2:
                case SetupNetwork.IOExpansion3:
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

        public ConfigurationDataType CheckConfigurationValueType(HorizontalMovementForwardProfile configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementForwardProfile.TotalSteps:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case HorizontalMovementForwardProfile.MovementCorrection:
                case HorizontalMovementForwardProfile.P0Acceleration:
                case HorizontalMovementForwardProfile.P0Deceleration:
                case HorizontalMovementForwardProfile.P0Quote:
                case HorizontalMovementForwardProfile.P0SpeedV1:
                case HorizontalMovementForwardProfile.P1Acceleration:
                case HorizontalMovementForwardProfile.P1Deceleration:
                case HorizontalMovementForwardProfile.P1Quote:
                case HorizontalMovementForwardProfile.P1SpeedV2:
                case HorizontalMovementForwardProfile.P2Acceleration:
                case HorizontalMovementForwardProfile.P2Deceleration:
                case HorizontalMovementForwardProfile.P2Quote:
                case HorizontalMovementForwardProfile.P2SpeedV3:
                case HorizontalMovementForwardProfile.P3Acceleration:
                case HorizontalMovementForwardProfile.P3Deceleration:
                case HorizontalMovementForwardProfile.P3Quote:
                case HorizontalMovementForwardProfile.P3SpeedV4:
                case HorizontalMovementForwardProfile.P4Acceleration:
                case HorizontalMovementForwardProfile.P4Deceleration:
                case HorizontalMovementForwardProfile.P4Quote:
                case HorizontalMovementForwardProfile.P4SpeedV5:
                case HorizontalMovementForwardProfile.P5Acceleration:
                case HorizontalMovementForwardProfile.P5Deceleration:
                case HorizontalMovementForwardProfile.P5Quote:
                case HorizontalMovementForwardProfile.P5Speed:
                case HorizontalMovementForwardProfile.TotalMovement:
                    returnValue = ConfigurationDataType.Float;
                    break;

                default:
                    returnValue = ConfigurationDataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public ConfigurationDataType CheckConfigurationValueType(HorizontalMovementBackwardProfile configurationValueEnum)
        {
            ConfigurationDataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementBackwardProfile.TotalSteps:
                    returnValue = ConfigurationDataType.Integer;
                    break;

                case HorizontalMovementBackwardProfile.MovementCorrection:
                case HorizontalMovementBackwardProfile.P0Acceleration:
                case HorizontalMovementBackwardProfile.P0Deceleration:
                case HorizontalMovementBackwardProfile.P0Quote:
                case HorizontalMovementBackwardProfile.P0SpeedV1:
                case HorizontalMovementBackwardProfile.P1Acceleration:
                case HorizontalMovementBackwardProfile.P1Deceleration:
                case HorizontalMovementBackwardProfile.P1Quote:
                case HorizontalMovementBackwardProfile.P1SpeedV2:
                case HorizontalMovementBackwardProfile.P2Acceleration:
                case HorizontalMovementBackwardProfile.P2Deceleration:
                case HorizontalMovementBackwardProfile.P2Quote:
                case HorizontalMovementBackwardProfile.P2SpeedV3:
                case HorizontalMovementBackwardProfile.P3Acceleration:
                case HorizontalMovementBackwardProfile.P3Deceleration:
                case HorizontalMovementBackwardProfile.P3Quote:
                case HorizontalMovementBackwardProfile.P3SpeedV4:
                case HorizontalMovementBackwardProfile.P4Acceleration:
                case HorizontalMovementBackwardProfile.P4Deceleration:
                case HorizontalMovementBackwardProfile.P4Quote:
                case HorizontalMovementBackwardProfile.P4SpeedV5:
                case HorizontalMovementBackwardProfile.P5Acceleration:
                case HorizontalMovementBackwardProfile.P5Deceleration:
                case HorizontalMovementBackwardProfile.P5Quote:
                case HorizontalMovementBackwardProfile.P5Speed:
                case HorizontalMovementBackwardProfile.TotalMovement:
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
                case VerticalManualMovements.InitialTargetPosition:
                case VerticalManualMovements.RecoveryTargetPosition:
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

        private bool CheckConfigurationDataType(long parameter, long category, ConfigurationDataType type)
        {
            var actualParameterType = ConfigurationDataType.Undefined;

            switch (category)
            {
                case (long)ConfigurationCategory.Undefined:
                    {
                        throw new DataLayerException(DataLayerExceptionCode.DatatypeException);
                    }
                case (long)ConfigurationCategory.GeneralInfo:
                    {
                        actualParameterType = this.CheckConfigurationValueType((GeneralInfo)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.SetupNetwork:
                    {
                        actualParameterType = this.CheckConfigurationValueType((SetupNetwork)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.SetupStatus:
                    {
                        actualParameterType = this.CheckConfigurationValueType((SetupStatus)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.VerticalAxis:
                    {
                        actualParameterType = this.CheckConfigurationValueType((VerticalAxis)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.HorizontalAxis:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalAxis)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.HorizontalMovementForwardProfile:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalMovementForwardProfile)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.HorizontalMovementBackwardProfile:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalMovementBackwardProfile)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.VerticalManualMovements:
                    {
                        actualParameterType = this.CheckConfigurationValueType((VerticalManualMovements)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.HorizontalManualMovements:
                    {
                        actualParameterType = this.CheckConfigurationValueType((HorizontalManualMovements)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.BeltBurnishing:
                    {
                        actualParameterType = this.CheckConfigurationValueType((BeltBurnishing)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.ResolutionCalibration:
                    {
                        actualParameterType = this.CheckConfigurationValueType((ResolutionCalibration)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.OffsetCalibration:
                    {
                        actualParameterType = this.CheckConfigurationValueType((OffsetCalibration)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.CellControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((CellControl)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.PanelControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((PanelControl)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.ShutterHeightControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((ShutterHeightControl)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.WeightControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((WeightControl)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.BayPositionControl:
                    {
                        actualParameterType = this.CheckConfigurationValueType((BayPositionControl)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.LoadFirstDrawer:
                    {
                        actualParameterType = this.CheckConfigurationValueType((LoadFirstDrawer)parameter);
                        break;
                    }
                case (long)ConfigurationCategory.ShutterManualMovements:
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
