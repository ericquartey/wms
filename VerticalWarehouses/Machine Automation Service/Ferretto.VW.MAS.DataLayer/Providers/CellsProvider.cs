using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class CellsProvider : Interfaces.ICellsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<CellsProvider> logger;

        #endregion

        #region Constructors

        public CellsProvider(DataLayerContext dataContext, ILogger<CellsProvider> logger)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.dataContext = dataContext;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public IEnumerable<Cell> GetAll()
        {
            return this.dataContext.Cells
                .Include(c => c.Panel)
                .ToArray();
        }

        public CellStatisticsSummary GetStatistics()
        {
            var totalCells = this.dataContext.Cells.Count();

            var cellsWithSide = this.dataContext.Cells.Include(c => c.Panel);

            var cellStatusStatistics = cellsWithSide
                .GroupBy(c => c.Status)
                .Select(g =>
                    new CellStatusStatistics
                    {
                        Status = g.Key,
                        TotalFrontCells = g.Count(c => c.Side == WarehouseSide.Front),
                        TotalBackCells = g.Count(c => c.Side == WarehouseSide.Back),
                        RatioFrontCells = g.Count(c => c.Side == WarehouseSide.Front) / (double)totalCells,
                        RatioBackCells = g.Count(c => c.Side == WarehouseSide.Back) / (double)totalCells,
                    });

            var occupiedOrUnusableCellsCount = this.dataContext.Cells
                .Count(c => c.Status == CellStatus.Occupied || c.Status == CellStatus.Unusable);

            var cellStatistics = new CellStatisticsSummary()
            {
                CellStatusStatistics = cellStatusStatistics,
                TotalCells = totalCells,
                TotalFrontCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                TotalBackCells = cellsWithSide.Count(c => c.Side == WarehouseSide.Front),
                CellOccupationPercentage = 100.0 * occupiedOrUnusableCellsCount / totalCells,
            };

            return cellStatistics;
        }

        public void LoadFrom(string fileNamePath)
        {
            if (this.dataContext.Cells.Any())
            {
                return;
            }

            this.logger.LogInformation("Importing cell definitions from configuration file ...");

            using (var jsonFile = new JSchemaValidatingReader(new JsonTextReader(new System.IO.StreamReader(fileNamePath))))
            {
                jsonFile.Schema = JSchema.Load(new JsonTextReader(new System.IO.StreamReader("configuration/schemas/cells-schema.json")));
                while (jsonFile.Read())
                {
                    if (jsonFile.TokenType == JsonToken.PropertyName && jsonFile.Value is string propertyName)
                    {
                        if (propertyName == "panels")
                        {
                            ReadAllPanels(this.dataContext, jsonFile);
                        }
                    }
                }
            }
        }

        public Cell UpdateHeight(int cellId, decimal height)
        {
            var cell = this.dataContext.Cells
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Id == cellId);

            if (cell is null)
            {
                throw new Exceptions.EntityNotFoundException(cellId);
            }

            var cellsOnSameSide = this.dataContext.Cells
                .Where(c => c.Side == cell.Side)
                .OrderBy(c => c.Position);

            var higherCell = cellsOnSameSide.FirstOrDefault(c => c.Position > cell.Position);
            var lowerCell = cellsOnSameSide.FirstOrDefault(c => c.Position < cell.Position);

            if ((higherCell == null
                ||
                higherCell.Position > height)
                &&
                (lowerCell == null
                ||
                lowerCell.Position < height))
            {
                cell.Position = height;

                this.dataContext.Cells.Update(cell);
                this.dataContext.SaveChanges();
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    Resources.Cells.TheSpecifiedHeightIsNotBetweenTheAdjacentCellsHeights);
            }

            return this.dataContext.Cells
                .Include(c => c.Panel)
                .SingleOrDefault(c => c.Id == cellId);
        }

        private static void ReadAllCells(DataLayerContext dataContext, JsonReader jsonFile, CellPanel panel)
        {
            while (jsonFile.Read() && jsonFile.TokenType != JsonToken.EndArray)
            {
                if (jsonFile.TokenType == JsonToken.StartObject)
                {
                    var cell = new Cell { PanelId = panel.Id, Status = CellStatus.Free };
                    dataContext.Cells.Add(cell);
                    while (jsonFile.Read() && jsonFile.TokenType != JsonToken.EndObject)
                    {
                        if (jsonFile.TokenType == JsonToken.PropertyName && jsonFile.Value is string propertyName)
                        {
                            if (string.Equals(propertyName, nameof(Cell.Id), StringComparison.InvariantCultureIgnoreCase))
                            {
                                int? id;
                                while (!(id = jsonFile.ReadAsInt32()).HasValue) { }

                                cell.Id = id.Value;
                            }
                            else if (string.Equals(propertyName, nameof(Cell.Position), StringComparison.InvariantCultureIgnoreCase))
                            {
                                decimal? position;
                                while (!(position = jsonFile.ReadAsDecimal()).HasValue) { }

                                cell.Position = position.Value;
                            }
                            else if (string.Equals(propertyName, nameof(Cell.Priority), StringComparison.InvariantCultureIgnoreCase))
                            {
                                int? priority;
                                while (!(priority = jsonFile.ReadAsInt32()).HasValue) { }

                                cell.Priority = priority.Value;
                            }
                            else if (string.Equals(propertyName, nameof(Cell.Status), StringComparison.InvariantCultureIgnoreCase))
                            {
                                while (jsonFile.Read() && jsonFile.TokenType != JsonToken.String) { }

                                cell.Status = (CellStatus)Enum.Parse(typeof(CellStatus), jsonFile.Value.ToString(), true);
                            }
                        }
                    }
                }
            }
        }

        private static void ReadAllPanels(DataLayerContext dataContext, JsonReader jsonFile)
        {
            while (jsonFile.Read())
            {
                if (jsonFile.TokenType == JsonToken.StartObject)
                {
                    var panel = new CellPanel();
                    dataContext.CellPanels.Add(panel);
                    dataContext.SaveChanges();
                    while (jsonFile.Read() && jsonFile.TokenType != JsonToken.EndObject)
                    {
                        if (jsonFile.TokenType == JsonToken.PropertyName && jsonFile.Value is string propertyName)
                        {
                            if (string.Equals(propertyName, nameof(CellPanel.Side), StringComparison.InvariantCultureIgnoreCase))
                            {
                                while (jsonFile.Read() && jsonFile.TokenType != JsonToken.String) { }
                                panel.Side = (WarehouseSide)Enum.Parse(typeof(WarehouseSide), jsonFile.Value.ToString(), true);
                            }
                            else if (string.Equals(propertyName, nameof(CellPanel.Cells), StringComparison.InvariantCultureIgnoreCase))
                            {
                                ReadAllCells(dataContext, jsonFile, panel);
                            }
                        }
                    }
                }
            }

            dataContext.SaveChanges();
        }

        #endregion
    }
}
