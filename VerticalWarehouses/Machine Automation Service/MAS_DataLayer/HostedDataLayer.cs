//using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using Ferretto.VW.Common_Utils;
//using Ferretto.VW.Common_Utils.Enumerations;
//using Ferretto.VW.Common_Utils.Events;
//using Ferretto.VW.Common_Utils.Messages;
//using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
//using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public class HostedDataLayer : BackgroundService, IHostedDataLayer, IHostedWriteLogService
    {
        //#region Fields

        //private readonly IEventAggregator eventAggregator;

        //private readonly DataLayerContext inMemoryDataContext;

        //#endregion

        //#region Constructors

        //public HostedDataLayer(string connectionString, DataLayerContext inMemoryDataContext,
        //    IEventAggregator eventAggregator)
        //{
        //    if (inMemoryDataContext == null)
        //        throw new DataLayerException(DataLayerExceptionEnum.DATALAYER_CONTEXT_EXCEPTION);

        //    if (eventAggregator == null) throw new DataLayerException(DataLayerExceptionEnum.EVENTAGGREGATOR_EXCEPTION);

        //    this.inMemoryDataContext = inMemoryDataContext;

        //    this.eventAggregator = eventAggregator;

        //    using (var initialContext = new DataLayerContext(
        //        new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options))
        //    {
        //        initialContext.Database.Migrate();

        //        if (!initialContext.ConfigurationValues.Any())
        //        {
        //            //TODO reovery database from permanent storage
        //        }

        //        foreach (var configurationValue in initialContext.ConfigurationValues)
        //            this.inMemoryDataContext.ConfigurationValues.Add(configurationValue);

        //        this.inMemoryDataContext.SaveChanges();
        //    }

        //    // The old WriteLogService
        //    var webApiCommandEvent = eventAggregator.GetEvent<CommandEvent>();

        //    webApiCommandEvent.Subscribe(this.LogWriting);
        //}

        //#endregion

        //#region Methods

        //public List<Cell> GetCellList()
        //{
        //    return this.inMemoryDataContext.Cells.ToList();
        //}

        //public decimal GetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        //{
        //    decimal returnDecimalValue = 0;

        //    var configurationValue =
        //        this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

        //    if (configurationValue != null)
        //    {
        //        if (configurationValue.VarType == DataTypeEnum.decimalType)
        //        {
        //            if (!decimal.TryParse(configurationValue.VarValue, out returnDecimalValue))
        //                throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //    else
        //        throw new ArgumentNullException();

        //    return returnDecimalValue;
        //}

        //public decimal GetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        //{
        //    decimal returnDecimalValue = 0;

        //    var runtimeValue =
        //        this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

        //    if (runtimeValue != null)
        //    {
        //        if (runtimeValue.VarType == DataTypeEnum.decimalType)
        //        {
        //            if (!decimal.TryParse(runtimeValue.VarValue, out returnDecimalValue))
        //                throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //    else
        //        throw new ArgumentNullException();

        //    return returnDecimalValue;
        //}

        //public ReturnMissionPosition GetFreeBlockPosition(decimal drawerHeight, int drawerId)
        //{
        //    var cellSpacing = this.GetIntegerConfigurationValue(ConfigurationValueEnum.cellSpacing);

        //    var cellsNumber = (int)Math.Ceiling(drawerHeight / cellSpacing);

        //    // INFO Always take into account +1 drawer cell height, to avoid impacts between two next drowers
        //    cellsNumber += 1;

        //    // INFO Inserted a control to free a booked block, in the case the drawer is refused after the weight control
        //    var inMemoryFreeBlockAlreadyBooked = this.inMemoryDataContext.FreeBlocks.FirstOrDefault(s => s.DrawerId >= drawerId);

        //    if (inMemoryFreeBlockAlreadyBooked != null)
        //    {
        //        inMemoryFreeBlockAlreadyBooked.BlockSize = 0;
        //        inMemoryFreeBlockAlreadyBooked.DrawerId = 0;

        //        this.inMemoryDataContext.SaveChanges();
        //    }

        //    var inMemoryFreeBlockFirstByPriority = this.inMemoryDataContext.FreeBlocks.OrderBy(s => s.Priority)
        //        .FirstOrDefault(s => s.BlockSize >= cellsNumber);

        //    if (inMemoryFreeBlockFirstByPriority == null)
        //    {
        //        throw new InMemoryDataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKING_EXCEPTION);
        //    }

        //    // INFO Change the BookedCells number in the FreeBlock table
        //    inMemoryFreeBlockFirstByPriority.BookedCellsNumber = cellsNumber;
        //    inMemoryFreeBlockFirstByPriority.DrawerId = drawerId;
        //    this.inMemoryDataContext.SaveChanges();

        //    var returnMissionPosition = new ReturnMissionPosition
        //    {
        //        ReturnCoord = inMemoryFreeBlockFirstByPriority.Coord,
        //        ReturnSide = inMemoryFreeBlockFirstByPriority.Side
        //    };

        //    return returnMissionPosition;
        //}

        //public int GetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        //{
        //    var returnIntegerValue = 0;

        //    var configurationValue =
        //        this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

        //    if (configurationValue != null)
        //    {
        //        if (configurationValue.VarType == DataTypeEnum.integerType)
        //        {
        //            if (!int.TryParse(configurationValue.VarValue, out returnIntegerValue))
        //                throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //    else
        //        throw new ArgumentNullException();

        //    return returnIntegerValue;
        //}

        //public int GetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        //{
        //    var returnIntegerValue = 0;

        //    var runtimeValue =
        //        this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

        //    if (runtimeValue != null)
        //    {
        //        if (runtimeValue.VarType == DataTypeEnum.integerType)
        //        {
        //            if (!int.TryParse(runtimeValue.VarValue, out returnIntegerValue))
        //                throw new InMemoryDataLayerException(DataLayerExceptionEnum.PARSE_EXCEPTION);
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //    else
        //        throw new ArgumentNullException();

        //    return returnIntegerValue;
        //}

        //public string GetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum)
        //{
        //    var returnStringValue = "";

        //    var configurationValue =
        //        this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

        //    if (configurationValue != null)
        //    {
        //        if (configurationValue.VarType == DataTypeEnum.stringType)
        //            returnStringValue = configurationValue.VarValue;
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //    else
        //        throw new ArgumentNullException();

        //    return returnStringValue;
        //}

        //public string GetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum)
        //{
        //    var returnStringValue = "";

        //    var runtimeValue =
        //        this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

        //    if (runtimeValue != null)
        //    {
        //        if (runtimeValue.VarType == DataTypeEnum.stringType)
        //            returnStringValue = runtimeValue.VarValue;
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //    else
        //        throw new ArgumentNullException();

        //    return returnStringValue;
        //}

        //public bool LogWriting(string logMessage)
        //{
        //    var updateOperation = true;

        //    try
        //    {
        //        this.inMemoryDataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    catch (DbUpdateException exception)
        //    {
        //        updateOperation = false;
        //    }

        //    return updateOperation;
        //}

        //public void LogWriting(CommandMessage command_EventParameter)
        //{
        //    string logMessage;

        //    switch (command_EventParameter.Type)
        //    {
        //        case MessageType.Homing:
        //            {
        //                logMessage = "Vertical Homing";
        //                break;
        //            }
        //        default:
        //            {
        //                logMessage = "Unknown Action";

        //                break;
        //            }
        //    }

        //    this.inMemoryDataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
        //    this.inMemoryDataContext.SaveChanges();
        //}

        //public void ReturnMissionEnded(int drawerId)
        //{
        //    var inMemoryFreeBlockSearchBookedCells = this.inMemoryDataContext.FreeBlocks.FirstOrDefault(s => s.BookedCellsNumber > 0 && s.DrawerId == drawerId);

        //    if (inMemoryFreeBlockSearchBookedCells == null)
        //    {
        //        throw new InMemoryDataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKED_EXCEPTION);
        //    }

        //    var filledStartCell = inMemoryFreeBlockSearchBookedCells.StartCell;
        //    var filledLastCell = filledStartCell + inMemoryFreeBlockSearchBookedCells.BookedCellsNumber * 2;

        //    for (var currentCell = filledStartCell; currentCell <= filledLastCell; currentCell += 2)
        //    {
        //        var inMemoryCellsSearchFilledCell = this.inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == currentCell);

        //        if (inMemoryCellsSearchFilledCell == null)
        //        {
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
        //        }

        //        inMemoryCellsSearchFilledCell.Status = Status.Occupied;
        //    }

        //    // INFO Run the Free Block table calculation after the update
        //    this.CreateFreeBlockTable();
        //}

        //public bool SetCellList(List<Cell> listCells)
        //{
        //    var setCellList = false;

        //    if (listCells != null)
        //    {
        //        setCellList = true;

        //        foreach (var cell in listCells)
        //        {
        //            var inMemoryCellCurrentValue =
        //                this.inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == cell.CellId);

        //            if (inMemoryCellCurrentValue != null)
        //            {
        //                inMemoryCellCurrentValue.Coord = cell.Coord;
        //                inMemoryCellCurrentValue.Priority = cell.Priority;
        //                inMemoryCellCurrentValue.Side = cell.Side;
        //                inMemoryCellCurrentValue.Status = cell.Status;

        //                this.inMemoryDataContext.SaveChanges();
        //            }
        //            else
        //                throw new ArgumentNullException();
        //        }
        //    }

        //    return setCellList;
        //}

        //public void SetDecimalConfigurationValue(ConfigurationValueEnum configurationValueEnum, decimal value)
        //{
        //    var configurationValue =
        //        this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

        //    if (configurationValue == null)
        //    {
        //        var newConfigurationValue = new ConfigurationValue();
        //        newConfigurationValue.VarName = configurationValueEnum;
        //        newConfigurationValue.VarType = DataTypeEnum.decimalType;
        //        newConfigurationValue.VarValue = value.ToString();

        //        this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        if (configurationValue.VarType == DataTypeEnum.decimalType)
        //        {
        //            configurationValue.VarValue = value.ToString();
        //            this.inMemoryDataContext.SaveChanges();
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //}

        //public void SetDecimalRuntimeValue(RuntimeValueEnum runtimeValueEnum, decimal value)
        //{
        //    var runtimeValue =
        //        this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

        //    if (runtimeValue == null)
        //    {
        //        var newRuntimeValue = new RuntimeValue();
        //        newRuntimeValue.VarName = runtimeValueEnum;
        //        newRuntimeValue.VarType = DataTypeEnum.decimalType;
        //        newRuntimeValue.VarValue = value.ToString();

        //        this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        if (runtimeValue.VarType == DataTypeEnum.decimalType)
        //        {
        //            runtimeValue.VarValue = value.ToString();
        //            this.inMemoryDataContext.SaveChanges();
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //}

        //public void SetIntegerConfigurationValue(ConfigurationValueEnum configurationValueEnum, int value)
        //{
        //    var configurationValue =
        //        this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

        //    if (configurationValue == null)
        //    {
        //        var newConfigurationValue = new ConfigurationValue();
        //        newConfigurationValue.VarName = configurationValueEnum;
        //        newConfigurationValue.VarType = DataTypeEnum.integerType;
        //        newConfigurationValue.VarValue = value.ToString();

        //        this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        if (configurationValue.VarType == DataTypeEnum.integerType)
        //        {
        //            configurationValue.VarValue = value.ToString();
        //            this.inMemoryDataContext.SaveChanges();
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //}

        //public void SetIntegerRuntimeValue(RuntimeValueEnum runtimeValueEnum, int value)
        //{
        //    var runtimeValue =
        //        this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

        //    if (runtimeValue == null)
        //    {
        //        var newRuntimeValue = new RuntimeValue();
        //        newRuntimeValue.VarName = runtimeValueEnum;
        //        newRuntimeValue.VarType = DataTypeEnum.integerType;
        //        newRuntimeValue.VarValue = value.ToString();

        //        this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        if (runtimeValue.VarType == DataTypeEnum.integerType)
        //        {
        //            runtimeValue.VarValue = value.ToString();
        //            this.inMemoryDataContext.SaveChanges();
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //}

        //public void SetStringConfigurationValue(ConfigurationValueEnum configurationValueEnum, string value)
        //{
        //    var configurationValue =
        //        this.inMemoryDataContext.ConfigurationValues.FirstOrDefault(s => s.VarName == configurationValueEnum);

        //    if (configurationValue == null)
        //    {
        //        var newConfigurationValue = new ConfigurationValue();
        //        newConfigurationValue.VarName = configurationValueEnum;
        //        newConfigurationValue.VarType = DataTypeEnum.stringType;
        //        newConfigurationValue.VarValue = value;

        //        this.inMemoryDataContext.ConfigurationValues.Add(newConfigurationValue);
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        if (configurationValue.VarType == DataTypeEnum.stringType)
        //        {
        //            configurationValue.VarValue = value;
        //            this.inMemoryDataContext.SaveChanges();
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //}

        //public void SetStringRuntimeValue(RuntimeValueEnum runtimeValueEnum, string value)
        //{
        //    var runtimeValue =
        //        this.inMemoryDataContext.RuntimeValues.FirstOrDefault(s => s.VarName == runtimeValueEnum);

        //    if (runtimeValue == null)
        //    {
        //        var newRuntimeValue = new RuntimeValue();
        //        newRuntimeValue.VarName = runtimeValueEnum;
        //        newRuntimeValue.VarType = DataTypeEnum.stringType;
        //        newRuntimeValue.VarValue = value;

        //        this.inMemoryDataContext.RuntimeValues.Add(newRuntimeValue);
        //        this.inMemoryDataContext.SaveChanges();
        //    }
        //    else
        //    {
        //        if (runtimeValue.VarType == DataTypeEnum.stringType)
        //        {
        //            runtimeValue.VarValue = value;
        //            this.inMemoryDataContext.SaveChanges();
        //        }
        //        else
        //            throw new InMemoryDataLayerException(DataLayerExceptionEnum.DATATYPE_EXCEPTION);
        //    }
        //}

        #region Methods

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        //private void CreateFreeBlockTable()
        //{
        //    var cellTablePopulated = false;

        //    var freeBlockCounter = 1;

        //    var cellCounterEven = 0;
        //    var cellCounterOdd = 0;

        //    var evenFreeBlock = new FreeBlock();
        //    var oddFreeBlock = new FreeBlock();

        //    var evenCellBeforePriority = -1;
        //    var oddCellBeforePriority = -1;

        //    // INFO Remove all the records from the FreeBlocks table
        //    this.inMemoryDataContext.FreeBlocks.RemoveRange(this.inMemoryDataContext.FreeBlocks);
        //    this.inMemoryDataContext.SaveChanges();

        //    foreach (var cell in this.inMemoryDataContext.Cells.OrderBy(cell => cell.CellId))
        //    {
        //        cellTablePopulated = true;

        //        if (cell.Side == Side.FrontEven)
        //        {
        //            if (cellCounterEven != 0 && (cell.Status == Status.Free || cell.Status == Status.Disabled) && evenCellBeforePriority < cell.Priority)
        //            {
        //                cellCounterEven++;
        //            }

        //            if (cell.Status == Status.Free && cellCounterEven == 0 /*&& evenCellBeforePriority < cell.Priority*/)
        //            {
        //                evenFreeBlock.StartCell = cell.CellId;
        //                evenFreeBlock.FreeBlockId = freeBlockCounter;
        //                evenFreeBlock.Priority = cell.Priority;
        //                evenFreeBlock.Coord = cell.Coord;
        //                evenFreeBlock.Side = cell.Side;

        //                freeBlockCounter++;
        //                cellCounterEven++;
        //            }

        //            if (cellCounterEven != 0 && (cell.Status == Status.Occupied || cell.Status == Status.Unusable || evenCellBeforePriority > cell.Priority))
        //            {
        //                evenFreeBlock.BlockSize = cellCounterEven;
        //                evenFreeBlock.BookedCellsNumber = 0;

        //                cellCounterEven = 0;
        //                this.inMemoryDataContext.FreeBlocks.Add(evenFreeBlock);
        //            }

        //            // INFO Saving the Priority before
        //            evenCellBeforePriority = cell.Priority;
        //        }
        //        else
        //        {
        //            if (cellCounterOdd != 0 && (cell.Status == Status.Free || cell.Status == Status.Disabled) && oddCellBeforePriority < cell.Priority)
        //            {
        //                cellCounterOdd++;
        //            }

        //            if (cell.Status == Status.Free && cellCounterOdd == 0 /*&& oddCellBeforePriority < cell.Priority*/)
        //            {
        //                oddFreeBlock.StartCell = cell.CellId;
        //                oddFreeBlock.FreeBlockId = freeBlockCounter;
        //                oddFreeBlock.Priority = cell.Priority;
        //                oddFreeBlock.Coord = cell.Coord;
        //                oddFreeBlock.Side = cell.Side;

        //                freeBlockCounter++;
        //                cellCounterOdd++;
        //            }

        //            if (cellCounterOdd != 0 && (cell.Status == Status.Occupied || cell.Status == Status.Unusable || oddCellBeforePriority > cell.Priority))
        //            {
        //                oddFreeBlock.BlockSize = cellCounterOdd;
        //                oddFreeBlock.BookedCellsNumber = 0;

        //                cellCounterOdd = 0;
        //                this.inMemoryDataContext.FreeBlocks.Add(oddFreeBlock);
        //            }

        //            // INFO Saving the Priority before
        //            oddCellBeforePriority = cell.Priority;
        //        }
        //    }

        //    if (!cellTablePopulated)
        //    {
        //        throw new InMemoryDataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
        //    }

        //    if (!this.inMemoryDataContext.FreeBlocks.Any())
        //    {
        //        throw new InMemoryDataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKING_EXCEPTION);
        //    }
        //}

        //#endregion
    }
}
