using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ferretto.Common.BusinessModels
{
    public interface IFilter
    {
        #region Properties

        Func<CompartmentDetails, CompartmentDetails, Color> ColorFunc { get; }
        string Description { get; }
        int Id { get; }
        CompartmentDetails Selected { get; set; }

        #endregion Properties
    };

    public class ArticleFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            Color color = Colors.Orange;
            if (selected != null)
            {
                if ((compartment.MaterialStatusId != 0 || compartment == selected) && compartment.MaterialStatusId == selected.MaterialStatusId)
                {
                    color = (Color)ColorConverter.ConvertFromString("#76FF03");
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString("#90A4AE");
                }
            }
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, CompartmentDetails, Color> ColorFunc { get => this.colorFunc; }
        public string Description { get => "Article"; }
        public int Id { get => 1; }
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }

    public class CompartmentFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            Color color = Colors.Red;
            if (selected != null)
            {
                if ((compartment.CompartmentTypeId != 0 || compartment == selected) && compartment.CompartmentTypeId == selected.CompartmentTypeId)
                {
                    color = (Color)ColorConverter.ConvertFromString("#76FF03");
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString("#90A4AE");
                }
            }
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, CompartmentDetails, Color> ColorFunc { get => this.colorFunc; }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }

    public class FillingFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            int stock = compartment.Stock;
            int? max = compartment.MaxCapacity;
            Color color = Colors.GreenYellow;

            if (max == null)
            {
                color = (Color)ColorConverter.ConvertFromString("#BDBDBD");
            }
            else
            {
                if (selected == null)
                {
                    double filling = ((double)stock / (int)max) * 100;
                    if (stock == 0 || (filling >= 0 && filling < 40))
                    {
                        color = (Color)ColorConverter.ConvertFromString("#76FF03");
                    }
                    if (filling >= 40 && filling < 60)
                    {
                        color = (Color)ColorConverter.ConvertFromString("#D4E157");
                    }
                    if (filling >= 60 && filling < 80)
                    {
                        color = (Color)ColorConverter.ConvertFromString("#FF9800");
                    }
                    if (filling >= 80 && filling <= 99)
                    {
                        color = (Color)ColorConverter.ConvertFromString("#F44336");
                    }
                    if (filling == 100)
                    {
                        color = (Color)ColorConverter.ConvertFromString("#D50000");
                    }
                }
                else
                {
                    int stockSelected = selected.Stock;
                    int? maxSelected = selected.MaxCapacity;
                    double fillingSelected = ((double)stockSelected / (int)maxSelected) * 100;
                    double filling = ((double)stock / (int)max) * 100;
                    if (filling == fillingSelected)
                    {
                        color = (Color)ColorConverter.ConvertFromString("#76FF03");
                    }
                    else
                    {
                        color = (Color)ColorConverter.ConvertFromString("#90A4AE");
                    }
                }
            }

            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, CompartmentDetails, Color> ColorFunc { get => this.colorFunc; }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }

    public class LinkedItemFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            Color color = Colors.Blue;
            if (selected != null)
            {
                if ((compartment.ItemPairing != 0 || compartment == selected) && compartment.ItemPairing == selected.ItemPairing)
                {
                    color = (Color)ColorConverter.ConvertFromString("#76FF03");
                }
                else
                {
                    color = (Color)ColorConverter.ConvertFromString("#90A4AE");
                }
            }
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, CompartmentDetails, Color> ColorFunc { get => this.colorFunc; }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }

    public class NotImplementdFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment, CompartmentDetails selected)
        {
            Color color = Colors.Gray;
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, CompartmentDetails, Color> ColorFunc { get => this.colorFunc; }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }
        public CompartmentDetails Selected { get; set; }

        #endregion Properties
    }
}
