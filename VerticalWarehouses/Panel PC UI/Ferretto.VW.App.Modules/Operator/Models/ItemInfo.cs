﻿using System;
using System.Linq;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class ItemInfo : Item
    {
        #region Constructors

        public ItemInfo(ProductInMachine product, int machineId)
                             : this(product?.Item, machineId)
        {
            this.Sscc = product.Sscc;
            this.Lot = product.Lot;
            this.ExpirationDate = product.ExpirationDate;
            this.SerialNumber = product.SerialNumber;

            if (product.Machines?.Any() == true)
            {
                this.IsQtyOnMachine = product?.Machines.Any(m => m.Id == machineId && m.ItemAvailableQuantity > 0) == true;

                this.MachinesInfo = string.Join(", ", product.Machines.Select(m => m.Nickname).ToArray());
                this.AvailableQuantity = (double?)product.Machines.Sum(m => m.ItemAvailableQuantity);
                this.Machines = product.Machines.Select(m => new MachinePick() { AvailableQuantityItem = (double?)m.ItemAvailableQuantity, Id = m.Id, Nickname = m.Nickname });
            }
        }

        public ItemInfo(Item item, int machineId)
        {
            if (item is null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }

            this.Sscc = string.Empty;
            this.Id = item.Id;
            this.Code = item.Code;
            this.Description = item.Description;
            this.MeasureUnitDescription = item.MeasureUnitDescription;
            this.Machines = item.Machines;
            this.PutTolerance = item.PutTolerance;
            this.AbcClassDescription = item.AbcClassDescription;
            this.Note = item.Note;
            this.Image = item.Image;
            this.IsDraperyItem = item.IsDraperyItem;
            this.PickTolerance = item.PickTolerance;
            this.AverageWeight = item.AverageWeight;
            this.UnitWeight = item.UnitWeight;
            this.IsQtyOnMachine = item.Machines?.Any(m => m.Id == machineId && m.AvailableQuantityItem > 0) == true;

            if (item.Machines?.Any() == true)
            {
                this.MachinesInfo = string.Join(", ", item.Machines.Select(m => m.Nickname).ToArray());
                this.AvailableQuantity = item.Machines.Sum(m => m.AvailableQuantityItem);
            }
        }

        #endregion

        #region Properties

        public double? AvailableQuantity { get; }

        public bool IsQtyOnMachine { get; }

        public string Lot { get; }

        public string MachinesInfo { get; }

        public string MeasureUnit => this.MeasureUnitDescription?.ToLowerInvariant() ?? Resources.Localized.Get("OperatorApp.Pieces");

        public double PickIncrement => this.PickTolerance.HasValue ? System.Math.Pow(10, -this.PickTolerance.Value) : 1;

        public string SerialNumber { get; }

        public string Sscc { get; }

        public DateTimeOffset? ExpirationDate { get; }

        #endregion
    }
}
