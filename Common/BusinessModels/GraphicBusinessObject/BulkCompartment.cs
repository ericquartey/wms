using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class BulkCompartment : INotifyPropertyChanged
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

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        [Display(Name = nameof(BusinessObjects.BulkCompartmentColumn), ResourceType = typeof(BusinessObjects))]
        public int Column
        {
            get => this.column;
            set
            {
                this.column = value;
                this.NotifyPropertyChanged(nameof(this.Column));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public int Height
        {
            get => this.height;
            set
            {
                this.height = value;
                this.NotifyPropertyChanged(nameof(this.Height));
            }
        }

        public int Id
        {
            get => this.id;
            set
            {
                this.id = value;
                this.NotifyPropertyChanged(nameof(this.Id));
            }
        }

        [Display(Name = nameof(BusinessObjects.BulkCompartmentRow), ResourceType = typeof(BusinessObjects))]
        public int Row
        {
            get => this.row;
            set
            {
                this.row = value;
                this.NotifyPropertyChanged(nameof(this.Row));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set
            {
                this.width = value;
                this.NotifyPropertyChanged(nameof(this.Width));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public int XPosition
        {
            get => this.xPosition;
            set
            {
                this.xPosition = value;

                this.NotifyPropertyChanged(nameof(this.XPosition));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public int YPosition
        {
            get => this.yPosition;
            set
            {
                this.yPosition = value;
                this.NotifyPropertyChanged(nameof(this.YPosition));
            }
        }

        #endregion Properties

        #region Methods

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}
