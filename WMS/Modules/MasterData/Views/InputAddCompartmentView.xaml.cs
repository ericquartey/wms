using System;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    /// <summary>
    /// Interaction logic for WmsInputCompartmentView.xaml
    /// </summary>
    public partial class InputAddCompartmentView : UserControl
    {
        #region Constructors

        public InputAddCompartmentView()
        {
            this.InitializeComponent();
        }

        #endregion Constructors
    }

    //private void DisenableAllInput()
    //{
    //    this.WidthText.IsEnabled = false;
    //    this.HeightText.IsEnabled = false;
    //    this.PositionXText.IsEnabled = false;
    //    this.PositionYText.IsEnabled = false;
    //    this.ArticleText.IsEnabled = false;
    //    this.QuantityText.IsEnabled = false;
    //    this.CapacityText.IsEnabled = false;
    //    //this.CreateCompartment.IsEnabled = false;
    //}

    //private void EnableAllInput()
    //{
    //    this.WidthText.IsEnabled = true;
    //    this.HeightText.IsEnabled = true;
    //    this.PositionXText.IsEnabled = true;
    //    this.PositionYText.IsEnabled = true;
    //    this.ArticleText.IsEnabled = true;
    //    this.QuantityText.IsEnabled = true;
    //    this.CapacityText.IsEnabled = true;
    //    //this.CreateCompartment.IsEnabled = true;
    //}
}
