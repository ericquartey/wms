using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm.UI;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    public class WmsCanvasItemsControl : System.Windows.Controls.ListBox
    {
        #region Fields

        private WmsTrayCanvas canvas;

        private LoadingUnitDetails loadingUnitDetails;

        #endregion Fields

        #region Constructors

        public WmsCanvasItemsControl()
        {
        }

        #endregion Constructors

        #region Properties

        public WmsTrayControl TrayControl { get; set; }

        #endregion Properties

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.SizeChanged += this.WmsCanvasItemsControl_SizeChanged;
            this.Loaded += this.WmsCanvasItemsControl_Loaded;
            //this.UnselectAll();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            //var selectedCompartment = e.AddedItems[0];
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WmsCompartmentViewModel newCompartment)
            {
                newCompartment.IsSelected = true;
                this.TrayControl.SelectedItem = newCompartment.CompartmentDetails;
                newCompartment.ColorFill = Colors.Red.ToString();
                newCompartment.ColorBorder = Colors.Gray.ToString();

                //TODO:: pass to COmpartmentViewModel

                //Command = "{Binding RelativeSource={RelativeSource Mode=FindAncestor, AncestorType={x:Type wms:WmsView}}, Path=DataContext.CreateNewCompartmentCommand}"

                //Binding binding = new Binding("CompartmentSelectedProperty", compartment, null);
                //BindingOperations.SetBinding()

                //if (this.DataContext is WmsTrayControlViewModel wmsTrayControlViewModel)
                //{
                //    //wmsTrayControlViewModel.CompartmentDetailsProperty = new CompartmentDetails()
                //    //{
                //    //    Width = (int)compartment.OriginWidth,
                //    //    Height = (int)compartment.OriginHeight,
                //    //    XPosition = (int)compartment.OriginLeft,
                //    //    YPosition = (int)compartment.OriginTop
                //    //};
                //    //wmsTrayControlViewModel.CompartmentDetailsProperty.OnUpdateCompartmentEvent(null);

                //    wmsTrayControlViewModel.UpdateInputForm(new CompartmentDetails()
                //    {
                //        Width = (int)compartment.CompartmentDetails.Width,
                //        Height = (int)compartment.CompartmentDetails.Height,
                //        XPosition = (int)compartment.CompartmentDetails.XPosition,
                //        YPosition = (int)compartment.CompartmentDetails.YPosition
                //    });
                //}
            }
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is WmsCompartmentViewModel oldCompartment)
            {
                oldCompartment.ColorFill = Colors.Aquamarine.ToString();
                oldCompartment.ColorBorder = Colors.RoyalBlue.ToString();
            }
        }

        //private static void OnSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        private void WmsCanvasItemsControl_Loaded(Object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.DataContext is WmsTrayControlViewModel wmsTrayControlViewModel)
            {
                this.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                this.loadingUnitDetails = wmsTrayControlViewModel.LoadingUnitProperty;

                var widthNewCalculated = this.ActualWidth;
                var heightNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.loadingUnitDetails.Length, widthNewCalculated, this.loadingUnitDetails.Width);

                this.canvas.Width = widthNewCalculated;
                this.canvas.Height = heightNewCalculated;
            }
        }

        private void WmsCanvasItemsControl_SizeChanged(Object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (this.canvas == null)
            {
                this.canvas = LayoutTreeHelper.GetVisualChildren(this).OfType<WmsTrayCanvas>().FirstOrDefault();
            }

            if (this.loadingUnitDetails != null)
            {
                var widthNewCalculated = this.ActualWidth;
                var heightNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.loadingUnitDetails.Length, widthNewCalculated, this.loadingUnitDetails.Width);

                if (heightNewCalculated > this.ActualHeight)
                {
                    heightNewCalculated = this.ActualHeight;
                    widthNewCalculated = GraphicUtils.ConvertMillimetersToPixel(this.loadingUnitDetails.Width, heightNewCalculated, this.loadingUnitDetails.Length);
                }
                this.canvas.Height = heightNewCalculated;
                this.canvas.Width = widthNewCalculated;
            }
        }

        #endregion Methods
    }
}
