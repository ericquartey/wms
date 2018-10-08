using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for WmsTrayControl.xaml
    /// </summary>
    public partial class WmsTrayControl : Canvas
    {
        #region Fields

        public static readonly DependencyProperty LoadingUnitProperty = DependencyProperty.Register(
                    nameof(LoadingUnit), typeof(LoadingUnitDetails), typeof(Canvas), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLoadingUnitChanged)));

        //public static readonly DependencyProperty MouseUpCommandProperty = DependencyProperty.RegisterAttached(
        //    "MouseUpCommand", typeof(ICommand), typeof(WmsTrayControl), new FrameworkPropertyMetadata(new PropertyChangedCallback(MouseUpCommandChanged)));

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
                    nameof(ReadOnly), typeof(bool), typeof(Canvas), new PropertyMetadata(false));

        #endregion Fields

        #region Constructors

        public WmsTrayControl()
        {
            this.InitializeComponent();
            //this.DataContext = new WmsTrayControlViewModel();
        }

        #endregion Constructors

        #region Properties

        public LoadingUnitDetails LoadingUnit
        {
            get => (LoadingUnitDetails)this.GetValue(LoadingUnitProperty);
            set => this.SetValue(LoadingUnitProperty, value);
        }

        public bool ReadOnly
        {
            get => (bool)this.GetValue(ReadOnlyProperty);
            set => this.SetValue(ReadOnlyProperty, value);
        }

        #endregion Properties

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            base.MeasureOverride(constraint);
            var h = base.ActualHeight;
            var w = base.ActualWidth;
            Grid parent = base.Parent as Grid;
            var width_parent = w;// parent.Width;
            var height_parent = h;// parent.Height;

            width_parent = 0;
            height_parent = 0;

            //var x = LayoutTreeHelper.GetVisualParents(this).FirstOrDefault(v => v is Grid);

            //if (base.InternalChildren != null && base.InternalChildren.Capacity > 0)
            //{
            //    double width = base
            //    .InternalChildren
            //    .OfType<UIElement>()
            //    .Max(i => i.DesiredSize.Width + (double)i.GetValue(Canvas.LeftProperty));

            //    double height = base
            //        .InternalChildren
            //        .OfType<UIElement>()
            //        .Max(i => i.DesiredSize.Height + (double)i.GetValue(Canvas.TopProperty));

            //IEnumerator enumerator = base.Children.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    Rectangle element = enumerator.Current as Rectangle;
            //    if (element != null)
            //    {
            //        if (element.Name.Equals(CompartmentDrawUserControl.NAME_RECTANGLE_DRAWER))
            //        {
            //            element.Width = 1000;
            //            element.Height = 200;
            //        }
            //    }
            //}
            //OK
            //var Children = base.Children;
            //foreach (UIElement child in Children)
            //{
            //    var r = child as Rectangle;
            //    r.Width = constraint.Width - 80;
            //}
            if (Double.IsNaN(width_parent))
            {
                width_parent = 0;
            }
            if (Double.IsNaN(height_parent))
            {
                height_parent = 0;
            }
            return new Size(width_parent, height_parent);
        }

        private static void OnLoadingUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataContext = ((WmsTrayControl)d).DataContext;
            if (dataContext != null)
            {
                var viewModel = dataContext as WmsTrayControlViewModel;
                if (viewModel != null)
                {
                    viewModel.UpdateTray((LoadingUnitDetails)e.NewValue);
                }
            }
        }

        #endregion Methods

        //private static void MouseUpCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    FrameworkElement element = (FrameworkElement)d;

        //    element.MouseUp += new MouseButtonEventHandler(element_MouseUp);
        //}

        //private static void element_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    FrameworkElement element = (FrameworkElement)sender;

        //    ICommand command = GetMouseUpCommand(element);

        //    command.Execute(e);
        //}

        //public static ICommand GetMouseUpCommand(UIElement element)
        //{
        //    return (ICommand)element.GetValue(MouseUpCommandProperty);
        //}

        //public static void SetMouseUpCommand(UIElement element, ICommand value)
        //{
        //    element.SetValue(MouseUpCommandProperty, value);
        //}
    }
}
