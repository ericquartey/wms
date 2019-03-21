using Ferretto.VW.MAS_DataLayer.Enumerations;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer
    {
        #region Methods

        public DataType ConvertConfigurationValue(GeneralInfo configurationValueEnum)
        {
            DataType returnValue;
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
                    returnValue = DataType.String;
                    break;

                case GeneralInfo.AlfaNumBay1:
                case GeneralInfo.AlfaNumBay2:
                case GeneralInfo.AlfaNumBay3:
                case GeneralInfo.LaserBay1:
                case GeneralInfo.LaserBay2:
                case GeneralInfo.LaserBay3:
                    returnValue = DataType.Boolean;
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
                case GeneralInfo.MaxWeight:
                    returnValue = DataType.Float;
                    break;

                case GeneralInfo.Bay1Type:
                case GeneralInfo.Bay2Type:
                case GeneralInfo.Bay3Type:
                case GeneralInfo.Shutter1Type:
                case GeneralInfo.Shutter2Type:
                case GeneralInfo.Shutter3Type:
                case GeneralInfo.DrawersQuantity:
                case GeneralInfo.BaysQuantity:
                    returnValue = DataType.Integer;
                    break;

                case GeneralInfo.InstallationDate:
                case GeneralInfo.ProductionDate:
                    returnValue = DataType.Date;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(SetupNetwork configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case SetupNetwork.MachineNumber:
                case SetupNetwork.Inverter1Port:
                case SetupNetwork.Inverter2Port:
                case SetupNetwork.IOExpansion1Port:
                case SetupNetwork.IOExpansion2Port:
                case SetupNetwork.IOExpansion3Port:
                    returnValue = DataType.Integer;
                    break;

                case SetupNetwork.WMS_ON:
                    returnValue = DataType.Boolean;
                    break;

                case SetupNetwork.AlfaNumBay1:
                case SetupNetwork.AlfaNumBay2:
                case SetupNetwork.AlfaNumBay3:
                case SetupNetwork.Inverter1:
                case SetupNetwork.Inverter2:
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
                    returnValue = DataType.IPAddress;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(SetupStatus configurationValueEnum)
        {
            return DataType.Boolean;
        }

        public DataType ConvertConfigurationValue(VerticalAxis configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case VerticalAxis.HomingSearchDirection:
                case VerticalAxis.HomingExecuted:
                    returnValue = DataType.Boolean;
                    break;

                case VerticalAxis.HomingExitAcceleration:
                case VerticalAxis.HomingExitDeceleration:
                case VerticalAxis.HomingExitSpeed:
                case VerticalAxis.HomingSearchAcceleration:
                case VerticalAxis.HomingSearchDeceleration:
                case VerticalAxis.HomingSearchSpeed:
                case VerticalAxis.LowerBound:
                case VerticalAxis.MaxAcceleration:
                case VerticalAxis.MaxDeceleration:
                case VerticalAxis.MaxSpeed:
                case VerticalAxis.Offset:
                case VerticalAxis.Resolution:
                case VerticalAxis.UpperBound:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(HorizontalAxis configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalAxis.HomingExecuted:
                    returnValue = DataType.Boolean;
                    break;

                case HorizontalAxis.AntiClockWiseRun:
                case HorizontalAxis.ClockWiseRun:
                case HorizontalAxis.MaxAcceleration:
                case HorizontalAxis.MaxDeceleration:
                case HorizontalAxis.MaxSpeed:
                case HorizontalAxis.Offset:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(HorizontalMovementForwardProfile configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementForwardProfile.TotalSteps:
                    returnValue = DataType.Integer;
                    break;

                case HorizontalMovementForwardProfile.InitialSpeed:
                case HorizontalMovementForwardProfile.Step1AccDec:
                case HorizontalMovementForwardProfile.Step1Position:
                case HorizontalMovementForwardProfile.Step1Speed:
                case HorizontalMovementForwardProfile.Step2AccDec:
                case HorizontalMovementForwardProfile.Step2Position:
                case HorizontalMovementForwardProfile.Step2Speed:
                case HorizontalMovementForwardProfile.Step3AccDec:
                case HorizontalMovementForwardProfile.Step3Position:
                case HorizontalMovementForwardProfile.Step3Speed:
                case HorizontalMovementForwardProfile.Step4AccDec:
                case HorizontalMovementForwardProfile.Step4Position:
                case HorizontalMovementForwardProfile.Step4Speed:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(HorizontalMovementBackwardProfile configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementBackwardProfile.TotalSteps:
                    returnValue = DataType.Integer;
                    break;

                case HorizontalMovementBackwardProfile.InitialSpeed:
                case HorizontalMovementBackwardProfile.Step1AccDec:
                case HorizontalMovementBackwardProfile.Step1Position:
                case HorizontalMovementBackwardProfile.Step1Speed:
                case HorizontalMovementBackwardProfile.Step2AccDec:
                case HorizontalMovementBackwardProfile.Step2Position:
                case HorizontalMovementBackwardProfile.Step2Speed:
                case HorizontalMovementBackwardProfile.Step3AccDec:
                case HorizontalMovementBackwardProfile.Step3Position:
                case HorizontalMovementBackwardProfile.Step3Speed:
                case HorizontalMovementBackwardProfile.Step4AccDec:
                case HorizontalMovementBackwardProfile.Step4Position:
                case HorizontalMovementBackwardProfile.Step4Speed:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(VerticalManualMovements configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case VerticalManualMovements.FeedRate:
                case VerticalManualMovements.InitialTargetPosition:
                case VerticalManualMovements.RecoveryTargetPosition:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(HorizontalManualMovements configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalManualMovements.FeedRate:
                case HorizontalManualMovements.InitialTargetPosition:
                case HorizontalManualMovements.RecoveryTargetPosition:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(BeltBurnishing configurationValueEnum)
        {
            return DataType.Integer;
        }

        public DataType ConvertConfigurationValue(ResolutionCalibration configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType ConvertConfigurationValue(OffsetCalibration configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case OffsetCalibration.ReferenceCell:
                    returnValue = DataType.Integer;
                    break;

                case OffsetCalibration.FeedRate:
                case OffsetCalibration.StepValue:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(CellControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType ConvertConfigurationValue(PanelControl configurationValueEnum)
        {
            DataType returnValue;
            switch (configurationValueEnum)
            {
                case PanelControl.FrontInitialReferenceCell:
                case PanelControl.FrontPanelQuantity:
                case PanelControl.BackInitialReferenceCell:
                case PanelControl.BackPanelQuantity:
                    returnValue = DataType.Integer;
                    break;

                case PanelControl.StepValue:
                case PanelControl.FeedRate:
                    returnValue = DataType.Float;
                    break;

                default:
                    returnValue = DataType.Undefined;
                    break;
            }
            return returnValue;
        }

        public DataType ConvertConfigurationValue(ShutterHeightControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType ConvertConfigurationValue(WeightControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType ConvertConfigurationValue(BayPositionControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType ConvertConfigurationValue(LoadFirstDrawer configurationValueEnum)
        {
            return DataType.Float;
        }

        #endregion
    }
}
