using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class LoadingUnitsProvider : Interfaces.ILoadingUnitsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<LoadingUnitsProvider> logger;

        #endregion

        #region Constructors

        public LoadingUnitsProvider(DataLayerContext dataContext, ILogger<LoadingUnitsProvider> logger)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public IEnumerable<LoadingUnit> GetAll()
        {
            return this.dataContext.LoadingUnits.ToArray();
        }

        public LoadingUnit GetById(int id)
        {
            var loadingUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == id);
            if (loadingUnit is null)
            {
                throw new Exceptions.EntityNotFoundException(id);
            }

            return loadingUnit;
        }

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            var loadingUnits = this.dataContext.LoadingUnits.Select(l =>
                 new LoadingUnitSpaceStatistics
                 {
                     MissionsCount = l.MissionsCount,
                     Code = l.Code,
                 }).ToArray();

            return loadingUnits;
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            var loadingUnits = this.dataContext.LoadingUnits.Select(l =>
                 new LoadingUnitWeightStatistics
                 {
                     Height = l.Height,
                     GrossWeight = l.GrossWeight,
                     Tare = l.Tare,
                     Code = l.Code,
                     MaxNetWeight = l.MaxNetWeight,
                     MaxWeightPercentage = (l.GrossWeight - l.Tare) * 100 / l.MaxNetWeight,
                 }).ToArray();

            return loadingUnits;
        }

        public async Task LoadFromAsync(string fileNamePath)
        {
            if (this.dataContext.LoadingUnits.Any())
            {
                return;
            }

            this.logger.LogInformation("Importing loading units catalog from configuration file ...");

            using (var jsonFile = new JSchemaValidatingReader(new JsonTextReader(new System.IO.StreamReader(fileNamePath))))
            {
                int? width;
                int? depth;
                IEnumerable<(string Id, decimal MaxLoadCapacity, decimal Tare)> loadingUnitClasses = null;
                IEnumerable<(int Code, string Class)> loadingUnits = null;

                jsonFile.Schema = JSchema.Load(new JsonTextReader(new System.IO.StreamReader("configuration/schemas/loading-units-schema.json")));
                while (await jsonFile.ReadAsync())
                {
                    if (jsonFile.TokenType == JsonToken.PropertyName && jsonFile.Value is string propertyName)
                    {
                        if (propertyName == "loading-unit-classes")
                        {
                            loadingUnitClasses = await this.ReadAllLoadingUnitClassesAsync(jsonFile);
                        }
                        else if (propertyName == "loading-units")
                        {
                            loadingUnits = await this.ReadAllLoadingUnitsAsync(jsonFile);
                        }
                        else if (propertyName == "width")
                        {
                            while (!(width = await jsonFile.ReadAsInt32Async()).HasValue)
                            {
                            }
                        }
                        else if (propertyName == "depth")
                        {
                            while (!(depth = await jsonFile.ReadAsInt32Async()).HasValue)
                            {
                            }
                        }
                    }
                }

                foreach (var loadingUnit in loadingUnits)
                {
                    var loadingUnitClass = loadingUnitClasses.SingleOrDefault(c => c.Id == loadingUnit.Class);
                    if (loadingUnitClass.Id == null)
                    {
                        throw new Exception($"Loading unit class '{loadingUnit.Class}' is not defined");
                    }

                    this.dataContext.LoadingUnits.Add(
                        new LoadingUnit
                        {
                            Code = loadingUnit.Code.ToString(),
                            MaxNetWeight = loadingUnitClass.MaxLoadCapacity,
                            Tare = loadingUnitClass.Tare,
                        });
                }

                this.dataContext.SaveChanges();
            }
        }

        private async Task<IEnumerable<(string Id, decimal MaxLoadCapacity, decimal Tare)>> ReadAllLoadingUnitClassesAsync(JSchemaValidatingReader jsonFile)
        {
            var classes = new List<(string id, decimal maxLoadCapaciy, decimal tare)>();
            while (await jsonFile.ReadAsync() && jsonFile.TokenType != JsonToken.EndArray)
            {
                if (jsonFile.TokenType == JsonToken.StartObject)
                {
                    (string Id, decimal MaxLoadCapacity, decimal Tare) loadingUnitClass = (string.Empty, 0, 0);

                    while (await jsonFile.ReadAsync() && jsonFile.TokenType != JsonToken.EndObject)
                    {
                        if (jsonFile.TokenType == JsonToken.PropertyName && jsonFile.Value is string propertyName)
                        {
                            if (propertyName == "id")
                            {
                                while (await jsonFile.ReadAsync() && jsonFile.TokenType != JsonToken.String)
                                {
                                }

                                loadingUnitClass.Id = jsonFile.Value as string;
                            }
                            else if (propertyName == "max-load-capacity")
                            {
                                decimal? maxLoadCapacity;
                                while (!(maxLoadCapacity = await jsonFile.ReadAsDecimalAsync()).HasValue)
                                {
                                }

                                loadingUnitClass.MaxLoadCapacity = maxLoadCapacity.Value;
                            }
                            else if (propertyName == "tare")
                            {
                                decimal? tare;
                                while (!(tare = await jsonFile.ReadAsDecimalAsync()).HasValue)
                                {
                                }

                                loadingUnitClass.Tare = tare.Value;
                            }
                        }
                    }

                    classes.Add(loadingUnitClass);
                }
            }

            return classes;
        }

        private async Task<IEnumerable<(int Code, string Class)>> ReadAllLoadingUnitsAsync(JSchemaValidatingReader jsonFile)
        {
            var loadingUnits = new List<(int Code, string Class)>();
            while (await jsonFile.ReadAsync() && jsonFile.TokenType != JsonToken.EndArray)
            {
                if (jsonFile.TokenType == JsonToken.StartObject)
                {
                    (int Code, string Class) loadingUnit = (0, string.Empty);

                    while (await jsonFile.ReadAsync() && jsonFile.TokenType != JsonToken.EndObject)
                    {
                        if (jsonFile.TokenType == JsonToken.PropertyName && jsonFile.Value is string propertyName)
                        {
                            if (propertyName == "class")
                            {
                                while (await jsonFile.ReadAsync() && jsonFile.TokenType != JsonToken.String)
                                {
                                }

                                loadingUnit.Class = jsonFile.Value as string;
                            }
                            else if (propertyName == "code")
                            {
                                int? code;
                                while (!(code = await jsonFile.ReadAsInt32Async()).HasValue)
                                {
                                }

                                loadingUnit.Code = code.Value;
                            }
                        }
                    }

                    loadingUnits.Add(loadingUnit);
                }
            }

            return loadingUnits;
        }

        #endregion
    }
}
