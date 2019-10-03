using System;
using System.IO;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService
    {
        #region Methods

        /// <summary>
        /// This method is been invoked during the installation, to load the general_info.json file
        /// </summary>
        /// <param name="configurationFilePath">Configuration parameters to load</param>
        private void LoadConfigurationValuesInfo(string configurationFilePath)
        {
            var dataContext = this.scope.ServiceProvider.GetRequiredService<DataLayerContext>();

                if (dataContext.ConfigurationValues.Any())
                {
                    return;
                }

                this.Logger.LogInformation($"First run: loading machine configration from external JSON file ...");

                string fileContents = null;
                using (var streamReader = new StreamReader(configurationFilePath))
                {
                    fileContents = streamReader.ReadToEnd();
                }

                var jsonObject = JObject.Parse(fileContents);

                var schema = JSchema.Load(new JsonTextReader(new StreamReader("configuration/schemas/vertimag-configuration-schema.json")));

                jsonObject.Validate(schema);

                foreach (var jsonCategory in jsonObject)
                {
                    if (string.Equals(jsonCategory.Key, nameof(Machine), StringComparison.OrdinalIgnoreCase))
                    {
                        var settings = new JsonSerializerSettings();
                        settings.Converters.Add(new IPAddressConverter());

                        var machine = JsonConvert.DeserializeObject<Machine>(jsonCategory.Value.ToString(), settings);

                        dataContext.Machines.Add(machine);
                        dataContext.SaveChanges();

                        continue;
                    }
                    else if (jsonCategory.Key == "$schema")
                    {
                        continue;
                    }

                    if (!Enum.TryParse(jsonCategory.Key, false, out ConfigurationCategory jsonElementCategory))
                    {
                        throw new DataLayerException($"Invalid configuration category: {jsonCategory.Key} found in configuration file");
                    }

                    foreach (var jsonData in (JObject)jsonCategory.Value)
                    {
                        switch (jsonElementCategory)
                        {
                            case ConfigurationCategory.VerticalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out VerticalManualMovements verticalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)verticalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.HorizontalManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out HorizontalManualMovements horizontalManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)horizontalManualMovementsData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BeltBurnishing:
                                if (!Enum.TryParse(jsonData.Key, false, out BeltBurnishing beltBurnishingData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)beltBurnishingData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ResolutionCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out ResolutionCalibration resolutionCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)resolutionCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.OffsetCalibration:
                                if (!Enum.TryParse(jsonData.Key, false, out OffsetCalibration offsetCalibrationData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)offsetCalibrationData, jsonData.Value);

                                break;

                            case ConfigurationCategory.CellControl:
                                if (!Enum.TryParse(jsonData.Key, false, out CellControl cellControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)cellControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.PanelControl:
                                if (!Enum.TryParse(jsonData.Key, false, out PanelControl panelControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)panelControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ShutterHeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out ShutterHeightControl shutterHeightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)shutterHeightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.WeightControl:
                                if (!Enum.TryParse(jsonData.Key, false, out WeightControl weightControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)weightControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.BayPositionControl:
                                if (!Enum.TryParse(jsonData.Key, false, out BayPositionControl bayPositionControlData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)bayPositionControlData, jsonData.Value);

                                break;

                            case ConfigurationCategory.LoadFirstDrawer:
                                if (!Enum.TryParse(jsonData.Key, false, out LoadFirstDrawer loadFirstDrawerData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)loadFirstDrawerData, jsonData.Value);

                                break;

                            case ConfigurationCategory.ShutterManualMovements:
                                if (!Enum.TryParse(jsonData.Key, false, out ShutterManualMovements shutterManualMovementsData))
                                {
                                    throw new DataLayerException($"Invalid configuration data: {jsonData.Key} in section {jsonCategory.Key} found in configuration file");
                                }

                                this.SaveConfigurationData(jsonElementCategory, (long)shutterManualMovementsData, jsonData.Value);

                                break;
                        }
                    }
                }

        }

        private void SaveConfigurationData(
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
                        this.SetBoolConfigurationValue(
                            configurationData,
                            elementCategory,
                            jsonDataValue.Value<bool>());
                        break;

                    case ConfigurationDataType.Date:
                        this.SetDateTimeConfigurationValue(
                            configurationData,
                            elementCategory,
                            jsonDataValue.Value<DateTime>());
                        break;

                    case ConfigurationDataType.Integer:
                        this.SetIntegerConfigurationValue(
                            configurationData,
                            elementCategory,
                            jsonDataValue.Value<int>());
                        break;

                    case ConfigurationDataType.Float:
                        this.SetDecimalConfigurationValue(
                            configurationData,
                            elementCategory,
                            jsonDataValue.Value<decimal>());
                        break;

                    case ConfigurationDataType.String:
                        var stringValue = jsonDataValue.Value<string>();
                        if (IPAddress.TryParse(stringValue, out var configurationValue)
                            &&
                            (stringValue.Count(c => c == ':') >= 2
                            ||
                            stringValue.Count(c => c == '.') == 3))
                        {
                            this.SetIpAddressConfigurationValue(configurationData, elementCategory, configurationValue);
                        }
                        else
                        {
                            this.SetStringConfigurationValue(configurationData, elementCategory, stringValue);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}");

                // TEMP throw new DataLayerException($"Exception: {ex.Message} while storing parameter {jsonDataValue.Path} in category {elementCategory}", DataLayerExceptionCode.SaveData, ex);
                this.SendErrorMessage(new DLExceptionMessageData(ex, string.Empty));
            }
        }

        #endregion
    }
}
