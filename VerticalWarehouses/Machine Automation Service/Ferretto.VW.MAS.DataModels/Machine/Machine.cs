using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Machine : DataModel, IValidable
    {
        #region Properties

        public bool AggregateList { get; set; }

        public string BackupServer { get; set; }

        public string BackupServerPassword { get; set; }

        public string BackupServerUsername { get; set; }

        /// <summary>
        /// Gets or sets the bays of the machine.
        /// </summary>
        public List<Bay> Bays { get; set; }

        /// <summary>
        /// Enables the management of logic containers inside Load Units (DEIMA)
        /// </summary>
        public bool Box { get; set; }

        public bool CanUserEnableWms { get; set; }

        /// <summary>
        /// Gets or sets the machine's elevator.
        /// </summary>
        public Elevator Elevator { get; set; }

        /// <summary>
        /// Enabe note in LU operation
        /// </summary>
        public bool EnabeNoteRules { get; set; }

        public int ExpireCountPrecent { get; set; } = 90;

        public int ExpireDays { get; set; } = 14;

        public bool FireAlarm { get; set; }

        /// <summary>
        /// Gets or sets the machine height, in millimeters.
        /// </summary>
        public double Height { get; set; }

        public int HorizontalCyclesToCalibrate { get; set; } = 10;

        public int HorizontalPositionToCalibrate { get; set; } = 10000;

        /// <summary>
        /// Get a value indicating if add item by list operation is enabled.
        /// </summary>
        public bool IsAddItemByList { get; set; }

        public bool IsAxisChanged { get; set; }

        public bool IsCarrefour { get; set; }

        public bool IsDbSaveOnServer { get; set; }

        public bool IsDbSaveOnTelemetry { get; set; }

        /// <summary>
        /// Get a value indicating if disable editing the available quantity of item in the picking operation.
        /// </summary>
        public bool IsDisableQtyItemEditingPick { get; set; }

        /// <summary>
        /// in inventory operation the barcode is only checked for matching and do not confirm operation
        /// </summary>
        public bool IsDoubleConfirmBarcodeInventory { get; set; }

        /// <summary>
        /// in pick operation the barcode is only checked for matching and do not confirm operation
        /// </summary>
        public bool IsDoubleConfirmBarcodePick { get; set; }

        /// <summary>
        /// in put operation the barcode is only checked for matching and do not confirm operation
        /// </summary>
        public bool IsDoubleConfirmBarcodePut { get; set; }

        /// <summary>
        /// Tendaggi Paradiso
        /// </summary>
        public bool IsDrapery { get; set; }

        /// <summary>
        /// Get/set a value indicating if add item operation is enabled.
        /// </summary>
        public bool IsEnableAddItem { get; set; }

        /// <summary>
        /// Get/set a value indicating if the pick operation and put operation for a given item can
        /// be performed in the loading unit view (IDROINOX)
        /// </summary>
        public bool IsEnableHandlingItemOperations { get; set; }

        public bool IsHeartBeat { get; set; }

        public bool IsLoadUnitFixed { get; set; }

        /// <summary>
        /// search items only in local machine (Deima compatibility)
        /// </summary>
        public bool IsLocalMachineItems { get; set; }

        /// <summary>
        /// The reason view shows order list besides reason list (SIDERPOL)
        /// </summary>
        public bool IsOrderList { get; set; }

        public bool IsQuantityLimited { get; set; }

        /// <summary>
        /// Get/set a value indicating if it is requested a confirm on last operation to be
        /// performed in a loading unit.
        /// </summary>
        public bool IsRequestConfirmForLastOperationOnLoadingUnit { get; set; }

        /// <summary>
        /// enable the ABC rotation class handling
        /// </summary>
        public bool IsRotationClass { get; set; }

        /// <summary>
        /// Get/set a value indicating if the stock value is updating by difference (IDROINOX)
        /// </summary>
        public bool IsUpdatingStockByDifference { get; set; }

        /// <summary>
        /// When this value is greater than zero it is used to separate item barcode from serial
        /// number barcode (DEIMA)
        /// </summary>
        public int ItemUniqueIdLength { get; set; }

        public bool ListPickConfirm { get; set; }

        public bool ListPutConfirm { get; set; }

        /// <summary>
        /// Load Unit depth, in millimeters
        /// </summary>
        public double LoadUnitDepth { get; set; }

        /// <summary>
        /// Load Unit maximum height, in millimeters
        /// </summary>
        public double LoadUnitMaxHeight { get; set; }

        public double LoadUnitMaxNetWeight { get; set; }

        /// <summary>
        /// Load Unit minimum height, in millimeters
        /// </summary>
        public double LoadUnitMinHeight { get; set; }

        /// <summary>
        /// Gets or sets the weight of an empty load unit.
        /// </summary>
        public double LoadUnitTare { get; set; }

        /// <summary>
        /// Gets or sets the percent of net weight to be considered very heavy in FindEmptyCell. To
        /// disable heavy bin management use 0. suggested value is 85.
        /// </summary>
        public double LoadUnitVeryHeavyPercent { get; set; }

        /// <summary>
        /// Load Unit width, in millimeters
        /// </summary>
        public double LoadUnitWidth { get; set; }

        /// <summary>
        /// Gets or sets the maximum gross weight that the machine can have.
        /// </summary>
        public double MaxGrossWeight { get; set; }

        /// <summary>
        /// Gets or sets the machine's model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the panels on which the cells are mounted.
        /// </summary>
        public IEnumerable<CellPanel> Panels { get; set; }

        /// <summary>
        /// Gets or sets the machine's serial number.
        /// </summary>
        public string SerialNumber { get; set; }

        public bool Simulation { get; set; }

        /// <summary>
        /// Gets or sets the tote barcode length (KOHLER)
        /// </summary>
        public int ToteBarcodeLength { get; set; }

        public bool TouchHelper { get; set; }

        public int VerticalCyclesToCalibrate { get; set; } = 50;

        public int? WaitingListPriorityHighlighted { get; set; }

        #endregion

        #region Methods

        public void Validate()
        {
            if (this.Bays.Select(b => b.Number).Distinct().Count() != this.Bays.Count())
            {
                throw new Exception("C'è più di una baia definita con lo stesso numero.");
            }

            this.Bays.ForEach(b => b.Validate());

            this.Panels.ForEach(p => p.Validate());
            this.Elevator.Validate();
        }

        #endregion
    }
}
