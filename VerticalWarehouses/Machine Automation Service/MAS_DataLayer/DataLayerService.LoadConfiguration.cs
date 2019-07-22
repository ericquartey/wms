using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_Utils.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayerService
    {
        #region Methods

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        /// <param name="configurationFilePath">Configuration parameters to load</param>
        /// <exception cref="DataLayerExceptionCode.UnknownInfoFileException">Exception for a wrong info file input name</exception>
        /// <exception cref="DataLayerExceptionCode.UndefinedTypeException">Exception for an unknown data type</exception>
        private async Task LoadConfigurationValuesInfoAsync(string configurationFilePath)
        {
            using (var streamReader = new StreamReader(configurationFilePath))
            {
                var json = streamReader.ReadToEnd();
                var jsonObject = JObject.Parse(json);

                foreach (var jsonCategory in jsonObject)
                {
                    if (!Enum.TryParse(jsonCategory.Key, false, out ConfigurationCategory jsonElementCategory))
                    {
                        throw new DataLayerException($"Invalid configuration category: {jsonCategory.Key} found in configuration file");
                    }

                    foreach (var jsonData in (JObject)jsonCategory.Value)
                    {
                        switch (jsonElementCategory)
                        {
                            case ConfigurationCategory.GeneralInfo:
                                if (!Enum.TryParse(jsonData.Key, false, out GeneralInfo generalInfoData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)generalInfoData, jsonData.Value);

                                break;

                            case ConfigurationCategory.SetupNetwork:
                                if (!Enum.TryParse(jsonData.Key, false, out SetupNetwork setupNetworkData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)setupNetworkData, jsonData.Value);

                                break;

                            case ConfigurationCategory.SetupStatus:
                                if (!Enum.TryParse(jsonData.Key, false, out SetupStatus setupStatusData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)setupStatusData, jsonData.Value);

                                break;

                            case ConfigurationCategory.VerticalAxis:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalAxis verticalAxisData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)verticalAxisData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalAxis:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalAxis horizontalAxisData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalAxisData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalMovementForwardProfile:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalMovementForwardProfile horizontalMovementForwardProfileData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalMovementForwardProfileData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalMovementBackwardProfile:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalMovementBackwardProfile horizontalMovementBackwardProfileData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalMovementBackwardProfileData, jsonData.Value);

                                break;

                            case ConfigurationCategory.VerticalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalManualMovements verticalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)verticalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalManualMovements horizontalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)horizontalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BeltBurnishing:
                                if (!Enum.TryParse(jsonData.Key, false, out BeltBurnishing beltBurnishingData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)beltBurnishingData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ResolutionCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out ResolutionCalibration resolutionCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)resolutionCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.OffsetCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out OffsetCalibration offsetCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)offsetCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.CellControl:
                                if (!Enum.TryParse(jsonData.Key, false, out CellControl cellControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)cellControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.PanelControl:
                                if (!Enum.TryParse(jsonData.Key, false, out PanelControl panelControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)panelControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ShutterHeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out ShutterHeightControl shutterHeightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)shutterHeightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.WeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out WeightControl weightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)weightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BayPositionControl:
                                if (!Enum.TryParse(jsonData.Key, false, out BayPositionControl bayPositionControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)bayPositionControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.LoadFirstDrawer:
                                if (!Enum.TryParse(jsonData.Key, false, out LoadFirstDrawer loadFirstDrawerData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                await this.SaveConfigurationDataAsync(jsonElementCategory, (long)loadFirstDrawerData, jsonData.Value);

                                break;
                        }
                    }
                }
            }
        }

        private async Task SaveConfigurationDataAsync(
                    ConfigurationCategory elementCategory,
            long configurationData,
            JToken jsonDataValue)
        {
            if (!Enum.TryParse(jsonDataValue.Type.ToString(), false, out ConfigurationDataType generalInfoConfigurationDataType))
            {
                throw new DataLayerException($"Invalid configuration data type: {jsonDataValue.Type.ToString()} for data {configurationData} in section {elementCategory} found in configuration file");
            }

            try
            {
                switch (generalInfoConfigurationDataType)
                {
                    case ConfigurationDataType.Boolean:
                        await this.SetBoolConfigurationValueAsync(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<bool>());
                        break;

                    case ConfigurationDataType.Date:
                        await this.SetDateTimeConfigurationValueAsync(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<DateTime>());
                        break;

                    case ConfigurationDataType.Integer:
                        await this.SetIntegerConfigurationValueAsync(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<int>());
                        break;

                    case ConfigurationDataType.Float:
                        await this.SetDecimalConfigurationValueAsync(
                            configurationData,
                            (long)elementCategory,
                            jsonDataValue.Value<decimal>());
                        break;

                    case ConfigurationDataType.String:
                        var stringValue = jsonDataValue.Value<string>();
                        if (IPAddress.TryParse(stringValue, out var configurationValue))
                        {
                            await this.SetIPAddressConfigurationValueAsync(configurationData, (long)elementCategory, configurationValue);
                        }
                        else
                        {
                            await this.SetStringConfigurationValueAsync(configurationData, (long)elementCategory, stringValue);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}");

                //TEMP throw new DataLayerException($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}", DataLayerExceptionCode.SaveData, ex);
                this.SendMessage(new DLExceptionMessageData(ex, string.Empty, 0));
            }
        }

        #endregion
    }
}
