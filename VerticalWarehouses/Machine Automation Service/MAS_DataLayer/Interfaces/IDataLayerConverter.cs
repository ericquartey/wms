using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Newtonsoft.Json.Linq;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IDataLayerConverter
    {
        #region Methods

        /// <summary>
        /// Get the data type on the input data
        /// </summary>
        /// <param name="configurationValueEnum"></param>
        /// <returns></returns>
        DataTypeEnum ConvertConfigurationValue(GeneralInfoEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(SetupNetworkEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(SetupStatusEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(VerticalAxisEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(HorizontalAxisEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(HorizontalMovementForwardProfileEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(HorizontalMovementBackwardProfileEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(VerticalManualMovementsEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(HorizontalManualMovementsEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(BeltBurnishingEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(ResolutionCalibrationEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(OffsetCalibrationEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(CellControlEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(PanelControlEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(ShutterHeightControlEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(WeightControlEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(BayPositionControlEnum configurationValueEnum);

        DataTypeEnum ConvertConfigurationValue(LoadFirstDrawerEnum configurationValueEnum);

        ConfigurationCategoryValueEnum GetJSonElementConfigurationCategory(KeyValuePair<string, JToken> jsonElement);

        #endregion
    }
}
