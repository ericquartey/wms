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

                case ConfigurationCategory.DepositAndPickUp:
                    {
                        actualParameterType = this.CheckConfigurationValueType((BeltBurnishing)parameter);
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
