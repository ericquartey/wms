using System;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.MAS_DataLayer.Enumerations;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer
    {
        #region Methods

        public DataType CheckConfigurationValueType(GeneralInfo configurationValueEnum)
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

        public DataType CheckConfigurationValueType(SetupNetwork configurationValueEnum)
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

                case SetupNetwork.AlfaNumBay1Net:
                case SetupNetwork.AlfaNumBay2Net:
                case SetupNetwork.AlfaNumBay3Net:
                case SetupNetwork.Inverter1:
                case SetupNetwork.Inverter2:
                case SetupNetwork.IOExpansion1:
                case SetupNetwork.IOExpansion2:
                case SetupNetwork.IOExpansion3:
                case SetupNetwork.LaserBay1Net:
                case SetupNetwork.LaserBay2Net:
                case SetupNetwork.LaserBay3Net:
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

        public DataType CheckConfigurationValueType(SetupStatus configurationValueEnum)
        {
            return DataType.Boolean;
        }

        public DataType CheckConfigurationValueType(VerticalAxis configurationValueEnum)
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

        public DataType CheckConfigurationValueType(HorizontalAxis configurationValueEnum)
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

        public DataType CheckConfigurationValueType(HorizontalMovementForwardProfile configurationValueEnum)
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

        public DataType CheckConfigurationValueType(HorizontalMovementBackwardProfile configurationValueEnum)
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

        public DataType CheckConfigurationValueType(VerticalManualMovements configurationValueEnum)
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

        public DataType CheckConfigurationValueType(HorizontalManualMovements configurationValueEnum)
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

        public DataType CheckConfigurationValueType(BeltBurnishing configurationValueEnum)
        {
            return DataType.Integer;
        }

        public DataType CheckConfigurationValueType(ResolutionCalibration configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType CheckConfigurationValueType(OffsetCalibration configurationValueEnum)
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

        public DataType CheckConfigurationValueType(CellControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType CheckConfigurationValueType(PanelControl configurationValueEnum)
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

        public DataType CheckConfigurationValueType(ShutterHeightControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType CheckConfigurationValueType(WeightControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType CheckConfigurationValueType(BayPositionControl configurationValueEnum)
        {
            return DataType.Float;
        }

        public DataType CheckConfigurationValueType(LoadFirstDrawer configurationValueEnum)
        {
            return DataType.Float;
        }

        private bool CheckDataType(long parameter, long category, DataType type)
        {
            var actualParameterType = DataType.Undefined;
            switch (category)
            {
                case (long)ConfigurationCategory.Undefined:
                    {
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
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
                default:
                    throw new ArgumentNullException();
            }

            return actualParameterType.Equals(type);
        }

        #endregion
    }
}
