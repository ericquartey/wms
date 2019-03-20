using System;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerConverter
    {
        #region Methods

        public DataTypeEnum ConvertConfigurationValue(GeneralInfoEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case GeneralInfoEnum.Address:
                case GeneralInfoEnum.CAP:
                case GeneralInfoEnum.City:
                case GeneralInfoEnum.ClientCode:
                case GeneralInfoEnum.ClientName:
                case GeneralInfoEnum.Country:
                case GeneralInfoEnum.Latitude:
                case GeneralInfoEnum.Longitude:
                case GeneralInfoEnum.Model:
                case GeneralInfoEnum.Order:
                case GeneralInfoEnum.Province:
                case GeneralInfoEnum.Serial:
                    returnValue = DataTypeEnum.stringType;
                    break;

                case GeneralInfoEnum.AlfaNumBay1:
                case GeneralInfoEnum.AlfaNumBay2:
                case GeneralInfoEnum.AlfaNumBay3:
                case GeneralInfoEnum.LaserBay1:
                case GeneralInfoEnum.LaserBay2:
                case GeneralInfoEnum.LaserBay3:
                    returnValue = DataTypeEnum.booleanType;
                    break;

                case GeneralInfoEnum.Bay1Height1:
                case GeneralInfoEnum.Bay1Height2:
                case GeneralInfoEnum.Bay2Height1:
                case GeneralInfoEnum.Bay2Height2:
                case GeneralInfoEnum.Bay3Height1:
                case GeneralInfoEnum.Bay3Height2:
                case GeneralInfoEnum.Bay1Position1:
                case GeneralInfoEnum.Bay1Position2:
                case GeneralInfoEnum.Bay2Position1:
                case GeneralInfoEnum.Bay2Position2:
                case GeneralInfoEnum.Bay3Position1:
                case GeneralInfoEnum.Bay3Position2:
                case GeneralInfoEnum.Height:
                case GeneralInfoEnum.MaxWeight:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                case GeneralInfoEnum.Bay1Type:
                case GeneralInfoEnum.Bay2Type:
                case GeneralInfoEnum.Bay3Type:
                case GeneralInfoEnum.Shutter1Type:
                case GeneralInfoEnum.Shutter2Type:
                case GeneralInfoEnum.Shutter3Type:
                case GeneralInfoEnum.DrawersQuantity:
                case GeneralInfoEnum.BaysQuantity:
                    returnValue = DataTypeEnum.integerType;
                    break;

                case GeneralInfoEnum.InstallationDate:
                case GeneralInfoEnum.ProductionDate:
                    returnValue = DataTypeEnum.dateTimeType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(SetupNetworkEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case SetupNetworkEnum.MachineNumber:
                case SetupNetworkEnum.Inverter1Port:
                case SetupNetworkEnum.Inverter2Port:
                case SetupNetworkEnum.IOExpansion1Port:
                case SetupNetworkEnum.IOExpansion2Port:
                case SetupNetworkEnum.IOExpansion3Port:
                    returnValue = DataTypeEnum.integerType;
                    break;

                case SetupNetworkEnum.WMS_ON:
                    returnValue = DataTypeEnum.booleanType;
                    break;

                case SetupNetworkEnum.AlfaNumBay1:
                case SetupNetworkEnum.AlfaNumBay2:
                case SetupNetworkEnum.AlfaNumBay3:
                case SetupNetworkEnum.Inverter1:
                case SetupNetworkEnum.Inverter2:
                case SetupNetworkEnum.IOExpansion1:
                case SetupNetworkEnum.IOExpansion2:
                case SetupNetworkEnum.IOExpansion3:
                case SetupNetworkEnum.LaserBay1:
                case SetupNetworkEnum.LaserBay2:
                case SetupNetworkEnum.LaserBay3:
                case SetupNetworkEnum.PPC1MasterIPAddress:
                case SetupNetworkEnum.PPC2SlaveIPAddress:
                case SetupNetworkEnum.PPC3SlaveIPAddress:
                case SetupNetworkEnum.SQLServerIPAddress:
                    returnValue = DataTypeEnum.IPAddressType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(SetupStatusEnum configurationValueEnum)
        {
            return DataTypeEnum.booleanType;
        }

        public DataTypeEnum ConvertConfigurationValue(VerticalAxisEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case VerticalAxisEnum.HomingSearchDirection:
                case VerticalAxisEnum.HomingExecuted:
                    returnValue = DataTypeEnum.booleanType;
                    break;

                case VerticalAxisEnum.HomingExitAcceleration:
                case VerticalAxisEnum.HomingExitDeceleration:
                case VerticalAxisEnum.HomingExitSpeed:
                case VerticalAxisEnum.HomingSearchAcceleration:
                case VerticalAxisEnum.HomingSearchDeceleration:
                case VerticalAxisEnum.HomingSearchSpeed:
                case VerticalAxisEnum.LowerBound:
                case VerticalAxisEnum.MaxAcceleration:
                case VerticalAxisEnum.MaxDeceleration:
                case VerticalAxisEnum.MaxSpeed:
                case VerticalAxisEnum.Offset:
                case VerticalAxisEnum.Resolution:
                case VerticalAxisEnum.UpperBound:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(HorizontalAxisEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalAxisEnum.HomingExecuted:
                    returnValue = DataTypeEnum.booleanType;
                    break;

                case HorizontalAxisEnum.AntiClockWiseRun:
                case HorizontalAxisEnum.ClockWiseRun:
                case HorizontalAxisEnum.MaxAcceleration:
                case HorizontalAxisEnum.MaxDeceleration:
                case HorizontalAxisEnum.MaxSpeed:
                case HorizontalAxisEnum.Offset:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(HorizontalMovementForwardProfileEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementForwardProfileEnum.TotalSteps:
                    returnValue = DataTypeEnum.integerType;
                    break;

                case HorizontalMovementForwardProfileEnum.InitialSpeed:
                case HorizontalMovementForwardProfileEnum.Step1AccDec:
                case HorizontalMovementForwardProfileEnum.Step1Position:
                case HorizontalMovementForwardProfileEnum.Step1Speed:
                case HorizontalMovementForwardProfileEnum.Step2AccDec:
                case HorizontalMovementForwardProfileEnum.Step2Position:
                case HorizontalMovementForwardProfileEnum.Step2Speed:
                case HorizontalMovementForwardProfileEnum.Step3AccDec:
                case HorizontalMovementForwardProfileEnum.Step3Position:
                case HorizontalMovementForwardProfileEnum.Step3Speed:
                case HorizontalMovementForwardProfileEnum.Step4AccDec:
                case HorizontalMovementForwardProfileEnum.Step4Position:
                case HorizontalMovementForwardProfileEnum.Step4Speed:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(HorizontalMovementBackwardProfileEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalMovementBackwardProfileEnum.TotalSteps:
                    returnValue = DataTypeEnum.integerType;
                    break;

                case HorizontalMovementBackwardProfileEnum.InitialSpeed:
                case HorizontalMovementBackwardProfileEnum.Step1AccDec:
                case HorizontalMovementBackwardProfileEnum.Step1Position:
                case HorizontalMovementBackwardProfileEnum.Step1Speed:
                case HorizontalMovementBackwardProfileEnum.Step2AccDec:
                case HorizontalMovementBackwardProfileEnum.Step2Position:
                case HorizontalMovementBackwardProfileEnum.Step2Speed:
                case HorizontalMovementBackwardProfileEnum.Step3AccDec:
                case HorizontalMovementBackwardProfileEnum.Step3Position:
                case HorizontalMovementBackwardProfileEnum.Step3Speed:
                case HorizontalMovementBackwardProfileEnum.Step4AccDec:
                case HorizontalMovementBackwardProfileEnum.Step4Position:
                case HorizontalMovementBackwardProfileEnum.Step4Speed:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(VerticalManualMovementsEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case VerticalManualMovementsEnum.FeedRate:
                case VerticalManualMovementsEnum.InitialTargetPosition:
                case VerticalManualMovementsEnum.RecoveryTargetPosition:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(HorizontalManualMovementsEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case HorizontalManualMovementsEnum.FeedRate:
                case HorizontalManualMovementsEnum.InitialTargetPosition:
                case HorizontalManualMovementsEnum.RecoveryTargetPosition:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(BeltBurnishingEnum configurationValueEnum)
        {
            return DataTypeEnum.integerType;
        }

        public DataTypeEnum ConvertConfigurationValue(ResolutionCalibrationEnum configurationValueEnum)
        {
            return DataTypeEnum.decimalType;
        }

        public DataTypeEnum ConvertConfigurationValue(OffsetCalibrationEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case OffsetCalibrationEnum.ReferenceCell:
                    returnValue = DataTypeEnum.integerType;
                    break;

                case OffsetCalibrationEnum.FeedRate:
                case OffsetCalibrationEnum.StepValue:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(CellControlEnum configurationValueEnum)
        {
            return DataTypeEnum.decimalType;
        }

        public DataTypeEnum ConvertConfigurationValue(PanelControlEnum configurationValueEnum)
        {
            DataTypeEnum returnValue;
            switch (configurationValueEnum)
            {
                case PanelControlEnum.FrontInitialReferenceCell:
                case PanelControlEnum.FrontPanelQuantity:
                case PanelControlEnum.BackInitialReferenceCell:
                case PanelControlEnum.BackPanelQuantity:
                    returnValue = DataTypeEnum.integerType;
                    break;

                case PanelControlEnum.StepValue:
                case PanelControlEnum.FeedRate:
                    returnValue = DataTypeEnum.decimalType;
                    break;

                default:
                    returnValue = DataTypeEnum.UndefinedType;
                    break;
            }
            return returnValue;
        }

        public DataTypeEnum ConvertConfigurationValue(ShutterHeightControlEnum configurationValueEnum)
        {
            return DataTypeEnum.decimalType;
        }

        public DataTypeEnum ConvertConfigurationValue(WeightControlEnum configurationValueEnum)
        {
            return DataTypeEnum.decimalType;
        }

        public DataTypeEnum ConvertConfigurationValue(BayPositionControlEnum configurationValueEnum)
        {
            return DataTypeEnum.decimalType;
        }

        public DataTypeEnum ConvertConfigurationValue(LoadFirstDrawerEnum configurationValueEnum)
        {
            return DataTypeEnum.decimalType;
        }

        public ConfigurationCategoryValueEnum GetJSonElementConfigurationCategory(KeyValuePair<string, JToken> jsonElement)
        {
            ConfigurationCategoryValueEnum returnValue;
            if (Enum.GetNames(typeof(GeneralInfoEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.GeneralInfoEnum;
            }
            else if (Enum.GetNames(typeof(SetupNetworkEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.SetupNetworkEnum;
            }
            else if (Enum.GetNames(typeof(SetupStatusEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.SetupStatusEnum;
            }
            else if (Enum.GetNames(typeof(VerticalAxisEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.VerticalAxisEnum;
            }
            else if (Enum.GetNames(typeof(HorizontalAxisEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.HorizontalAxisEnum;
            }
            else if (Enum.GetNames(typeof(HorizontalMovementForwardProfileEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.HorizontalMovementForwardProfileEnum;
            }
            else if (Enum.GetNames(typeof(HorizontalMovementBackwardProfileEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.HorizontalMovementBackwardProfileEnum;
            }
            else if (Enum.GetNames(typeof(VerticalManualMovementsEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.VerticalManualMovementsEnum;
            }
            else if (Enum.GetNames(typeof(HorizontalManualMovementsEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.HorizontalManualMovementsEnum;
            }
            else if (Enum.GetNames(typeof(BeltBurnishingEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.BeltBurnishingEnum;
            }
            else if (Enum.GetNames(typeof(ResolutionCalibrationEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.ResolutionCalibrationEnum;
            }
            else if (Enum.GetNames(typeof(OffsetCalibrationEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.OffsetCalibrationEnum;
            }
            else if (Enum.GetNames(typeof(CellControlEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.CellControlEnum;
            }
            else if (Enum.GetNames(typeof(PanelControlEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.PanelControlEnum;
            }
            else if (Enum.GetNames(typeof(ShutterHeightControlEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.ShutterHeightControlEnum;
            }
            else if (Enum.GetNames(typeof(WeightControlEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.WeightControlEnum;
            }
            else if (Enum.GetNames(typeof(BayPositionControlEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.BayPositionControlEnum;
            }
            else if (Enum.GetNames(typeof(LoadFirstDrawerEnum)).FirstOrDefault(x => jsonElement.Key.ToString() == x) != null)
            {
                returnValue = ConfigurationCategoryValueEnum.LoadFirstDrawerEnum;
            }
            else
            {
                returnValue = ConfigurationCategoryValueEnum.Undefined;
            }
            return returnValue;
        }

        #endregion
    }
}
