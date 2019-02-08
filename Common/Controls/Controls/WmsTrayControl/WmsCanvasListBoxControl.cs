using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class WmsCanvasListBoxControl : ListBox
    {
        #region Fields

        public static readonly DependencyProperty BackgroundGrodLinesProperty =
                             DependencyProperty.Register(nameof(BackgroundGrodLines), typeof(DrawingBrush), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty BackgroundStepEndProperty =
                             DependencyProperty.Register(nameof(BackgroundStepEnd), typeof(double), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty BackgroundStepStartProperty =
                               DependencyProperty.Register(nameof(BackgroundStepStart), typeof(double), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(nameof(Compartments),
            typeof(IEnumerable<ICompartment>), typeof(WmsCanvasListBoxControl), new FrameworkPropertyMetadata(OnCompartmentsChanged));

        public static readonly DependencyProperty DimensionHeightProperty = DependencyProperty.Register(nameof(DimensionHeight),
            typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0, OnDimensionHeightChanged));

        public static readonly DependencyProperty DimensionWidthProperty = DependencyProperty.Register(nameof(DimensionWidth),
            typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0, OnDimensionWidthChanged));

        public static readonly DependencyProperty GridLinesColorProperty =
                                DependencyProperty.Register(nameof(GridLinesColor), typeof(Brush), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(Brushes.LightGray, OnGridLinesColorChanged));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(nameof(IsCompartmentSelectable), typeof(bool), typeof(WmsCanvasListBoxControl),
            new UIPropertyMetadata(true, OnIsCompartmentSelectableChanged));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly),
            typeof(bool), typeof(WmsCanvasListBoxControl), new FrameworkPropertyMetadata(OnIsReadOnlyChanged));

        public static readonly DependencyProperty MinTrayWidthProperty =
                     DependencyProperty.Register(nameof(MinTrayWidth), typeof(double), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty OriginXProperty = DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Tray Origin Y;
        /// </summary>
        public static readonly DependencyProperty OriginYProperty =
            DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty ParentHeightProperty =
            DependencyProperty.Register(nameof(ParentHeight), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty ParentWidthProperty =
            DependencyProperty.Register(nameof(ParentWidth), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty PenSizeProperty =
                           DependencyProperty.Register(nameof(PenSize), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(1.0, OnStepChanged));

        public static readonly DependencyProperty RulerSizeProperty =
            DependencyProperty.Register(nameof(RulerSize), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(0.0, OnRulerSizeChanged));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(nameof(SelectedColorFilterFunc), typeof(Func<ICompartment, ICompartment, string>),
            typeof(WmsCanvasListBoxControl), new FrameworkPropertyMetadata(OnSelectedColorFilterFuncChanged));

        public static readonly DependencyProperty SelectedCompartmentProperty = DependencyProperty.Register(nameof(SelectedCompartment), typeof(ICompartment),
            typeof(WmsCanvasListBoxControl), new FrameworkPropertyMetadata(OnSelectedCompartmentChanged));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(nameof(ShowBackground),
            typeof(bool), typeof(WmsCanvasListBoxControl), new FrameworkPropertyMetadata(OnShowBackgroundChanged));

        public static readonly DependencyProperty StepPixelProperty = DependencyProperty.Register(nameof(StepPixel), typeof(double), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty StepProperty =
                                DependencyProperty.Register(nameof(Step), typeof(double), typeof(WmsCanvasListBoxControl), new UIPropertyMetadata(100.0, OnStepChanged));

        public static readonly DependencyProperty TrayBackgroundProperty =
                                DependencyProperty.Register(nameof(TrayBackground), typeof(Brush), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty TrayHeightProperty =
                                DependencyProperty.Register(nameof(TrayHeight), typeof(double), typeof(WmsCanvasListBoxControl));

        public static readonly DependencyProperty TrayWidthProperty =
                               DependencyProperty.Register(nameof(TrayWidth), typeof(double), typeof(WmsCanvasListBoxControl));

        private const string DEFAULTBACKGROUND = "CommonSecondaryMedium";

        private const string DEFAULTCOMPARTMENTCOLOR = "DefaultCompartmentColor";

        private const int OFFSET = 1;

        #endregion

        #region Constructors

        public WmsCanvasListBoxControl()
        {
            this.SnapsToDevicePixels = true;
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        #endregion

        #region Properties

        public DrawingBrush BackgroundGrodLines
        {
            get => (DrawingBrush)this.GetValue(BackgroundGrodLinesProperty);
            set => this.SetValue(BackgroundGrodLinesProperty, value);
        }

        public double BackgroundStepEnd
        {
            get => (double)this.GetValue(BackgroundStepEndProperty);
            set => this.SetValue(BackgroundStepEndProperty, value);
        }

        public double BackgroundStepStart
        {
            get => (double)this.GetValue(BackgroundStepStartProperty);
            set => this.SetValue(BackgroundStepStartProperty, value);
        }

        public IEnumerable<ICompartment> Compartments
        {
            get => (IEnumerable<ICompartment>)this.GetValue(CompartmentsProperty);
            set => this.SetValue(CompartmentsProperty, value);
        }

        public double DimensionHeight
        {
            get => (double)this.GetValue(DimensionHeightProperty);
            set => this.SetValue(DimensionHeightProperty, value);
        }

        public double DimensionWidth
        {
            get => (double)this.GetValue(DimensionWidthProperty);
            set => this.SetValue(DimensionWidthProperty, value);
        }

        public Brush GridLinesColor
        {
            get => (Brush)this.GetValue(GridLinesColorProperty);
            set => this.SetValue(GridLinesColorProperty, value);
        }

        public bool IsCompartmentSelectable
        {
            get => (bool)this.GetValue(IsCompartmentSelectableProperty);
            set => this.SetValue(IsCompartmentSelectableProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        public double MinTrayWidth
        {
            get => (double)this.GetValue(MinTrayWidthProperty);
            set => this.SetValue(MinTrayWidthProperty, value);
        }

        public double OriginX
        {
            get => (double)this.GetValue(OriginXProperty);
            set => this.SetValue(OriginXProperty, value);
        }

        public double OriginY
        {
            get => (double)this.GetValue(OriginYProperty);
            set => this.SetValue(OriginYProperty, value);
        }

        public double ParentHeight
        {
            get => (double)this.GetValue(ParentHeightProperty);
            set => this.SetValue(ParentHeightProperty, value);
        }

        public double ParentWidth
        {
            get => (double)this.GetValue(ParentWidthProperty);
            set => this.SetValue(ParentWidthProperty, value);
        }

        public double PenSize
        {
            get => (double)this.GetValue(PenSizeProperty);
            set => this.SetValue(PenSizeProperty, value);
        }

        public double RulerSize
        {
            get => (double)this.GetValue(RulerSizeProperty);
            set => this.SetValue(RulerSizeProperty, value);
        }

        public Func<ICompartment, ICompartment, string> SelectedColorFilterFunc
        {
            get => (Func<ICompartment, ICompartment, string>)this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public ICompartment SelectedCompartment
        {
            get => (ICompartment)this.GetValue(SelectedCompartmentProperty);
            set => this.SetValue(SelectedCompartmentProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public double Step
        {
            get => (double)this.GetValue(StepProperty);
            set => this.SetValue(StepProperty, value);
        }

        public double StepPixel
        {
            get => (double)this.GetValue(StepPixelProperty);
            set => this.SetValue(StepPixelProperty, value);
        }

        public Brush TrayBackground
        {
            get => (Brush)this.GetValue(TrayBackgroundProperty);
            set => this.SetValue(TrayBackgroundProperty, value);
        }

        public double TrayHeight
        {
            get => (double)this.GetValue(TrayHeightProperty);
            set => this.SetValue(TrayHeightProperty, value);
        }

        public double TrayWidth
        {
            get => (double)this.GetValue(TrayWidthProperty);
            set => this.SetValue(TrayWidthProperty, value);
        }

        #endregion

        #region Methods

        public static double ConvertMillimetersToPixel(double value, double pixel, double mm)
        {
            return mm > 0 ? (pixel * value / mm) : value;
        }

        public static Position ConvertWithStandardOrigin(Position compartmentOrigin, double originX, double originY,
                                                         double dimensionWidth, double dimensionHeight, int widthCompartment, int heightCompartment)
        {
            var ret = new Position { X = compartmentOrigin.X, Y = compartmentOrigin.Y };

            if (originX.Equals(0) && originY.Equals(dimensionHeight))
            {
                ret.Y = (int)dimensionHeight - compartmentOrigin.Y - heightCompartment;
            }
            else if (originX.Equals(dimensionWidth) && originY.Equals(0))
            {
                ret.X = dimensionWidth - compartmentOrigin.X - widthCompartment;
            }
            else if (originX.Equals(dimensionWidth) && originY.Equals(dimensionHeight))
            {
                ret.X = dimensionWidth - compartmentOrigin.X - widthCompartment;
                ret.Y = dimensionHeight - compartmentOrigin.Y - heightCompartment;
            }

            return ret;
        }

        public static void DrawSnappedLinesBetweenPoints(DrawingContext dc, Pen pen, double lineThickness, params Point[] points)
        {
            var guidelineSet = new GuidelineSet();
            foreach (var point in points)
            {
                guidelineSet.GuidelinesX.Add(point.X);
                guidelineSet.GuidelinesY.Add(point.Y);
            }

            var half = lineThickness / 2;
            points = points.Select(p => new Point(p.X + half, p.Y + half)).ToArray();
            dc.PushGuidelineSet(guidelineSet);

            for (var i = 0; i < points.Length - 1; i = i + 2)
            {
                dc.DrawLine(pen, points[i], points[i + 1]);
            }

            dc.Pop();
        }

        public void ResizeCompartments()
        {
            if (this.Items == null)
            {
                return;
            }

            foreach (var compartment in this.Items.AsEnumerable())
            {
                this.ResizeCompartment(compartment);
            }
        }

        public void SetBackground()
        {
            if (this.TrayWidth <= 0 ||
                this.TrayHeight <= 0)
            {
                return;
            }

            this.StepPixel = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);

            this.PenSize = this.GetSizeOfPen() / 4;
            var offSet = this.PenSize / 2;
            this.BackgroundStepStart = this.PenSize - offSet;
            this.BackgroundStepEnd = this.StepPixel - this.PenSize + offSet;
        }

        public void SetControlSize()
        {
            var widthNewCalculated = this.ParentWidth;
            var heightNewCalculated = this.ParentHeight;
            heightNewCalculated -= this.RulerSize;
            widthNewCalculated -= this.RulerSize;
            if (this.DimensionHeight <= 0 ||
                this.DimensionWidth <= 0 ||
                widthNewCalculated <= 0 ||
                heightNewCalculated <= 0)
            {
                return;
            }

            var heightConverted = ConvertMillimetersToPixel(this.DimensionHeight, widthNewCalculated, this.DimensionWidth);

            if (heightConverted > heightNewCalculated)
            {
                widthNewCalculated = ConvertMillimetersToPixel(this.DimensionWidth, heightNewCalculated, this.DimensionHeight);
            }
            else
            {
                heightNewCalculated = heightConverted;
            }

            this.Width = widthNewCalculated;
            this.Height = heightNewCalculated;
            this.MinTrayWidth = widthNewCalculated + this.RulerSize + 1;

            this.TrayHeight = heightNewCalculated;
            this.TrayWidth = widthNewCalculated;

            this.ResizeCompartments();
            this.SetSelectedItem();
            this.SetBackground();
        }

        public void SetSize(double heightNewCalculated, double widthNewCalculated)
        {
            this.ParentHeight = heightNewCalculated;
            this.ParentWidth = widthNewCalculated;
            this.SetControlSize();
        }

        public void UpdateCompartments()
        {
            if (this.Compartments == null)
            {
                this.ItemsSource = null;
                return;
            }

            var newItems = new ObservableCollection<WmsCompartmentViewModel>();
            foreach (var compartment in this.Compartments)
            {
                var newCompartment = new WmsCompartmentViewModel
                {
                    CompartmentDetails = compartment,
                    Width = compartment.Width ?? 0,
                    Height = compartment.Height ?? 0,
                    Left = compartment.XPosition ?? 0,
                    Top = compartment.YPosition ?? 0,
                    ColorFill = this.SelectedColorFilterFunc?.Invoke(compartment, this.SelectedCompartment) ?? Application.Current.Resources[DEFAULTCOMPARTMENTCOLOR].ToString(),
                    IsReadOnly = this.IsReadOnly,
                    IsSelectable = this.IsCompartmentSelectable
                };
                newItems.Add(newCompartment);
                this.ResizeCompartment(newCompartment);
            }

            this.ItemsSource = newItems;
        }

        public void UpdateIsReadOnly()
        {
            if (this.Items == null)
            {
                return;
            }

            foreach (var compartment in this.Items.AsEnumerable())
            {
                compartment.IsReadOnly = this.IsReadOnly;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (this.StepPixel.Equals(0))
            {
                return;
            }

            if (this.ShowBackground == false)
            {
                return;
            }

            var penSize = this.GetSizeOfPen();
            var pen = new Pen
            {
                DashCap = PenLineCap.Square,
                Brush = this.GridLinesColor,
                Thickness = penSize,
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square
            };

            var stepXPixel = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);
            var stepYPixel = ConvertMillimetersToPixel(this.Step, this.TrayHeight, this.DimensionHeight);

            var posY = (this.OriginY.Equals(0)) ? stepYPixel : this.ActualHeight - stepYPixel - OFFSET;
            var points = new List<Point>();
            while (posY > 0 && posY < this.TrayHeight)
            {
                points.Add(new Point(0, posY));
                points.Add(new Point(this.TrayWidth, posY));
                if (this.OriginY == 0)
                {
                    posY += stepYPixel;
                }
                else
                {
                    posY -= stepYPixel;
                }
            }

            var isOriginZero = this.OriginX.Equals(0);
            var posX = isOriginZero ? stepXPixel : this.ActualWidth - stepXPixel - OFFSET;
            while (posX > 0 && posX < this.TrayWidth)
            {
                points.Add(new Point(posX, 0));
                points.Add(new Point(posX, this.TrayHeight));
                var isOriginXZero = this.OriginX == 0;
                if (isOriginXZero)
                {
                    posX += stepXPixel;
                }
                else
                {
                    posX -= stepXPixel;
                }
            }

            points.Add(new Point(-1, this.ActualHeight));
            points.Add(new Point(this.TrayWidth, this.ActualHeight));
            DrawSnappedLinesBetweenPoints(drawingContext, pen, penSize, points.ToArray());

            points.Clear();
            points.Add(new Point(this.TrayWidth, -1));
            points.Add(new Point(this.TrayWidth, this.ActualHeight));
            pen.Brush = Application.Current.Resources[DEFAULTBACKGROUND] as Brush;

            DrawSnappedLinesBetweenPoints(drawingContext, pen, penSize, points.ToArray());
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is WmsCompartmentViewModel newCompartment)
            {
                this.SelectedCompartment = newCompartment.CompartmentDetails;
            }
        }

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.UpdateCompartments();
            }
        }

        private static void OnDimensionHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.SetControlSize();
            }
        }

        private static void OnDimensionWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.SetControlSize();
            }
        }

        private static void OnGridLinesColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetBackground();
            }
        }

        private static void OnIsCompartmentSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.UpdateIsCompartmentSelectable();
            }
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.UpdateIsReadOnly();
            }
        }

        private static void OnRulerSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.SetControlSize();
            }
        }

        private static void OnSelectedColorFilterFuncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.UpdateColorCompartments();
            }
        }

        private static void OnSelectedCompartmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.SetSelectedItem();
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.SetBackground();
            }
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WmsCanvasListBoxControl wmsCanvasListBox)
            {
                wmsCanvasListBox.SetControlSize();
            }
        }

        private double GetSizeOfPen()
        {
            var ma = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            return 1 / ma.M11;
        }

        private void ResizeCompartment(WmsCompartmentViewModel compartment)
        {
            if (compartment == null)
            {
                return;
            }

            if (compartment.CompartmentDetails.Width == null || compartment.CompartmentDetails.Height == null ||
                compartment.CompartmentDetails.XPosition == null || compartment.CompartmentDetails.YPosition == null)
            {
                return;
            }

            var compartmentOrigin = new Position
            {
                X = compartment.CompartmentDetails.XPosition.Value,
                Y = compartment.CompartmentDetails.YPosition.Value
            };

            var convertedCompartmentOrigin = ConvertWithStandardOrigin(
                compartmentOrigin,
                this.OriginX,
                this.OriginY,
                this.DimensionWidth,
                this.DimensionHeight,
                (int)compartment.CompartmentDetails.Width,
                (int)compartment.CompartmentDetails.Height);

            compartment.Top = ConvertMillimetersToPixel(
                convertedCompartmentOrigin.Y,
                this.TrayHeight,
                this.DimensionHeight);
            compartment.Left = ConvertMillimetersToPixel(
                convertedCompartmentOrigin.X,
                this.TrayWidth,
                this.DimensionWidth);

            var height = ConvertMillimetersToPixel(
            (double)compartment.CompartmentDetails.Height,
             this.TrayHeight,
             this.DimensionHeight);
            var width = ConvertMillimetersToPixel(
            (double)compartment.CompartmentDetails.Width,
             this.TrayWidth,
             this.DimensionWidth);

            compartment.Height = height;
            compartment.Width = width;
        }

        private void SetSelectedItem()
        {
            if (this.SelectedCompartment == null ||
                this.Items == null)
            {
                this.SelectedItem = null;
                return;
            }

            var foundCompartment = this.Items.AsEnumerable().FirstOrDefault(c => c.CompartmentDetails.Id == this.SelectedCompartment.Id);
            if (foundCompartment == null)
            {
                this.SelectedItem = null;
                return;
            }

            if (this.SelectedItem != null &&
                ((WmsCompartmentViewModel)this.SelectedItem).CompartmentDetails.Id == this.SelectedCompartment.Id)
            {
                return;
            }

            this.SelectedItem = foundCompartment;
            this.UpdateColorCompartments();
            this.ResizeCompartment(this.SelectedItem as WmsCompartmentViewModel);
        }

        private void UpdateColorCompartments()
        {
            if (this.Items == null || this.SelectedColorFilterFunc == null)
            {
                return;
            }

            foreach (var item in this.Items.AsEnumerable())
            {
                item.ColorFill = this.SelectedColorFilterFunc.Invoke(item.CompartmentDetails, this.SelectedCompartment) ?? Application.Current.Resources[DEFAULTCOMPARTMENTCOLOR].ToString();
            }
        }

        private void UpdateIsCompartmentSelectable()
        {
            if (this.Items == null)
            {
                return;
            }

            foreach (var compartment in this.Items.AsEnumerable())
            {
                compartment.IsSelectable = this.IsCompartmentSelectable;
            }
        }

        #endregion
    }
}
