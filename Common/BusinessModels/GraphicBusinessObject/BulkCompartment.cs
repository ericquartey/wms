using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class BulkCompartment : BusinessObject
    {
        #region Fields

        private int column;
        private int height;
        private int id;
        private int row;
        private int width;
        private int xPosition;
        private int yPosition;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.BulkCompartmentColumn), ResourceType = typeof(BusinessObjects))]
        public int Column
        {
            get => this.column;
            set => this.SetProperty(ref this.column, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public int Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        public int Id
        {
            get => this.id;
            set => this.SetProperty(ref this.id, value);
        }

        [Display(Name = nameof(BusinessObjects.BulkCompartmentRow), ResourceType = typeof(BusinessObjects))]
        public int Row
        {
            get => this.row;
            set => this.SetProperty(ref this.row, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public int XPosition
        {
            get => this.xPosition;
            set => this.SetProperty(ref this.xPosition, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public int YPosition
        {
            get => this.yPosition;
            set => this.SetProperty(ref this.yPosition, value);
        }

        #endregion Properties
    }
}
