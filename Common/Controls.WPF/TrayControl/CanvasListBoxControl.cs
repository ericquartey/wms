using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ferretto.Common.Controls.WPF
{
    public class CanvasListBoxControl : ListBox
    {
        #region Fields

        public static readonly DependencyProperty BackgroundGridLinesProperty = DependencyProperty.Register(
            nameof(BackgroundGridLines),
            typeof(DrawingBrush),
            typeof(CanvasListBoxControl));

        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(
            nameof(Compartments),
            typeof(IEnumerable<IDrawableCompartment>),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(OnCompartmentsChanged));

        public static readonly DependencyProperty DefaultCompartmentColorProperty = DependencyProperty.Register(
            nameof(DefaultCompartmentColor),
            typeof(string),
            typeof(CanvasListBoxControl));

        public static readonly DependencyProperty DimensionHeightProperty = DependencyProperty.Register(
            nameof(DimensionHeight),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(0.0, OnDimensionHeightChanged));

        public static readonly DependencyProperty DimensionWidthProperty = DependencyProperty.Register(
            nameof(DimensionWidth),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(0.0, OnDimensionWidthChanged));

        public static readonly DependencyProperty GridLinesColorProperty = DependencyProperty.Register(
            nameof(GridLinesColor),
            typeof(Brush),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(Brushes.LightGray, OnGridLinesColorChanged));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(
            nameof(IsCompartmentSelectable),
            typeof(bool),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(true, OnIsCompartmentSelectableChanged));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(OnIsReadOnlyChanged));

        public static readonly DependencyProperty OriginHorizontalProperty = DependencyProperty.Register(
            nameof(OriginHorizontal),
            typeof(OriginHorizontal),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(OriginHorizontal.Left));

        public static readonly DependencyProperty OriginVerticalProperty = DependencyProperty.Register(
            nameof(OriginVertical),
            typeof(OriginVertical),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(OriginVertical.Bottom));

        public static readonly DependencyProperty ParentHeightProperty = DependencyProperty.Register(
            nameof(ParentHeight),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty ParentWidthProperty = DependencyProperty.Register(
            nameof(ParentWidth),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(0.0));

        public static readonly DependencyProperty PenSizeProperty = DependencyProperty.Register(
            nameof(PenSize),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(1.0, OnStepChanged));

        public static readonly DependencyProperty RulerSizeProperty = DependencyProperty.Register(
            nameof(RulerSize),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(0.0, OnRulerSizeChanged));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
            nameof(SelectedColorFilterFunc),
            typeof(Func<IDrawableCompartment, IDrawableCompartment, string>),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(OnSelectedColorFilterFuncChanged));

        public static readonly DependencyProperty SelectedCompartmentProperty = DependencyProperty.Register(
            nameof(SelectedCompartment),
            typeof(IDrawableCompartment),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(OnSelectedCompartmentChanged));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
            nameof(ShowBackground),
            typeof(bool),
            typeof(CanvasListBoxControl),
            new FrameworkPropertyMetadata(true, OnShowBackgroundChanged));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(
            nameof(ShowRuler),
            typeof(bool),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(true, OnShowRulerPropertyChanged));

        public static readonly DependencyProperty StepPixelProperty = DependencyProperty.Register(
            nameof(StepPixel),
            typeof(double),
            typeof(CanvasListBoxControl));

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(CanvasListBoxControl),
            new UIPropertyMetadata(100.0, OnStepChanged));

        public static readonly DependencyProperty TrayBackgroundProperty = DependencyProperty.Register(
            nameof(TrayBackground),
            typeof(Brush),
            typeof(CanvasListBoxControl));

        public static readonly DependencyProperty TrayHeightProperty = DependencyProperty.Register(
            nameof(TrayHeight),
            typeof(double),
            typeof(CanvasListBoxControl));

        public static readonly DependencyProperty TrayWidthProperty = DependencyProperty.Register(
            nameof(TrayWidth),
            typeof(double),
            typeof(CanvasListBoxControl));

        private const double HALF_MARK = 0.5;

        private const double ROUND_DIMENSION = 0.001;

        #endregion

        #region Properties

        public DrawingBrush BackgroundGridLines
        {
            get => (DrawingBrush)this.GetValue(BackgroundGridLinesProperty);
            set => this.SetValue(BackgroundGridLinesProperty, value);
        }

        public IEnumerable<IDrawableCompartment> Compartments
        {
            get => (IEnumerable<IDrawableCompartment>)this.GetValue(CompartmentsProperty);
            set => this.SetValue(CompartmentsProperty, value);
        }

        public string DefaultCompartmentColor
        {
            get => (string)this.GetValue(DefaultCompartmentColorProperty);
            set => this.SetValue(DefaultCompartmentColorProperty, value);
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

        public OriginHorizontal OriginHorizontal
        {
            get => (OriginHorizontal)this.GetValue(OriginHorizontalProperty);
            set => this.SetValue(OriginHorizontalProperty, value);
        }

        public OriginVertical OriginVertical
        {
            get => (OriginVertical)this.GetValue(OriginVerticalProperty);
            set => this.SetValue(OriginVerticalProperty, value);
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

        public Func<IDrawableCompartment, IDrawableCompartment, string> SelectedColorFilterFunc
        {
            get => (Func<IDrawableCompartment, IDrawableCompartment, string>)this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public IDrawableCompartment SelectedCompartment
        {
            get => (IDrawableCompartment)this.GetValue(SelectedCompartmentProperty);
            set => this.SetValue(SelectedCompartmentProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public bool ShowRuler
        {
            get => (bool)this.GetValue(ShowRulerProperty);
            set => this.SetValue(ShowRulerProperty, value);
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

        public static Point ConvertWithStandardOrigin(
            Point compartmentOrigin,
            OriginHorizontal originHorizontal,
            OriginVertical originVertical,
            double dimensionWidth,
            double dimensionHeight,
            double widthCompartment,
            double depthCompartment)
        {
            var ret = new Point { X = compartmentOrigin.X, Y = compartmentOrigin.Y };

            if (originHorizontal == OriginHorizontal.Left && originVertical == OriginVertical.Bottom)
            {
                ret.Y = dimensionHeight - compartmentOrigin.Y - depthCompartment;
            }
            else if (originHorizontal == OriginHorizontal.Right && originVertical == OriginVertical.Top)
            {
                ret.X = dimensionWidth - compartmentOrigin.X - widthCompartment;
            }
            else if (originHorizontal == OriginHorizontal.Right && originVertical == OriginVertical.Bottom)
            {
                ret.X = dimensionWidth - compartmentOrigin.X - widthCompartment;
                ret.Y = dimensionHeight - compartmentOrigin.Y - depthCompartment;
            }

            return ret;
        }

        public void SetBackground()
        {
            if (this.TrayWidth <= 0 ||
                this.TrayHeight <= 0)
            {
                return;
            }

            this.StepPixel = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);
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

            var adjustWidth = ((this.DimensionHeight * widthNewCalculated) % this.DimensionWidth) / this.DimensionHeight;
            var heightConverted = ConvertMillimetersToPixel(this.DimensionHeight, widthNewCalculated - adjustWidth, this.DimensionWidth);

            if (heightConverted > heightNewCalculated)
            {
                var adjustHeight = ((this.DimensionWidth * heightNewCalculated) % this.DimensionHeight) / this.DimensionWidth;
                heightNewCalculated -= adjustHeight;
                widthNewCalculated = ConvertMillimetersToPixel(this.DimensionWidth, heightNewCalculated, this.DimensionHeight);
            }
            else
            {
                widthNewCalculated -= adjustWidth;
                heightNewCalculated = heightConverted;
            }

            this.TrayHeight = heightNewCalculated;
            this.TrayWidth = widthNewCalculated;

            this.UpdateCompartments();
            this.SetSelectedItem();
            this.SetBackground();

            this.Width = (int)(widthNewCalculated + ROUND_DIMENSION);
            this.Height = (int)(heightNewCalculated + ROUND_DIMENSION);
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

            var newItems = new ObservableCollection<CompartmentViewModel>();
            foreach (var compartment in this.Compartments)
            {
                var newCompartment = new CompartmentViewModel
                {
                    CompartmentDetails = compartment,
                    Width = compartment.Width ?? 0,
                    Depth = compartment.Depth ?? 0,
                    Left = compartment.XPosition ?? 0,
                    Top = compartment.YPosition ?? 0,
                    ColorFill = this.GetColorFilter(compartment),
                    IsReadOnly = this.IsReadOnly,
                    IsSelectable = this.IsCompartmentSelectable,
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

            foreach (var compartment in this.Items.AsCompartmentViewModel())
            {
                compartment.ColorFill = this.GetColorFilter(compartment.CompartmentDetails);
                compartment.IsReadOnly = this.IsReadOnly;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            this.DrawGridLines(drawingContext);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e == null)
            {
                return;
            }

            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CompartmentViewModel newCompartment)
            {
                this.SelectedCompartment = newCompartment.CompartmentDetails;
            }
        }

        private static void OnCompartmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetControlSize();
            }
        }

        private static void OnDimensionHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetControlSize();
            }
        }

        private static void OnDimensionWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetControlSize();
            }
        }

        private static void OnGridLinesColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetBackground();
            }
        }

        private static void OnIsCompartmentSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.UpdateIsCompartmentSelectable();
            }
        }

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.UpdateIsReadOnly();
            }
        }

        private static void OnRulerSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetControlSize();
            }
        }

        private static void OnSelectedColorFilterFuncChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.UpdateColorCompartments();
            }
        }

        private static void OnSelectedCompartmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetSelectedItem();
            }
        }

        private static void OnShowBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetBackground();
            }
        }

        private static void OnShowRulerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.UpdateLayout();
            }
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CanvasListBoxControl canvasListBox)
            {
                canvasListBox.SetControlSize();
            }
        }

        private void DrawGridLines(DrawingContext drawingContext)
        {
            if (!this.ShowBackground)
            {
                return;
            }

            var penSize = this.GetSizeOfPen();
            var points = new List<Point>();
            var stepXPixel = ConvertMillimetersToPixel(this.Step, this.TrayWidth, this.DimensionWidth);
            var stepYPixel = ConvertMillimetersToPixel(this.Step, this.TrayHeight, this.DimensionHeight);

            var posY = this.OriginVertical == OriginVertical.Top ? stepYPixel : this.TrayHeight;
            while (posY > 1 && posY <= this.TrayHeight)
            {
                points.Add(new Point(1, posY));
                points.Add(new Point(this.TrayWidth, posY));
                if (this.OriginVertical == OriginVertical.Top)
                {
                    posY += stepYPixel;
                }
                else
                {
                    posY -= stepYPixel;
                }
            }

            var posX = this.OriginHorizontal == OriginHorizontal.Left ? stepXPixel : this.TrayWidth;
            posX += HALF_MARK;
            while (posX > 1 && posX <= this.TrayWidth)
            {
                points.Add(new Point(posX, this.GetCorrectedMarginOffset()));
                points.Add(new Point(posX, this.TrayHeight));
                if (this.OriginHorizontal == OriginHorizontal.Left)
                {
                    posX += stepXPixel;
                }
                else
                {
                    posX -= stepXPixel;
                }
            }

            var penLines = new Pen
            {
                DashCap = PenLineCap.Square,
                Thickness = penSize,
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square,
            };

            penLines.Brush = this.GridLinesColor;

            this.DrawSnappedLinesBetweenPoints(drawingContext, penSize, this.GridLinesColor, points.ToArray());
        }

        private void DrawSnappedLineBetweenPoints(DrawingContext dc, Point pointStart, Point pointEnd, Brush color)
        {
            var pen = new Pen
            {
                DashCap = PenLineCap.Square,
                Brush = color,
                Thickness = this.GetSizeOfPen(),
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square,
            };
            var guidelineSet = new GuidelineSet();
            guidelineSet.GuidelinesX.Add(pointStart.X);
            guidelineSet.GuidelinesY.Add(pointStart.Y);
            guidelineSet.GuidelinesX.Add(pointEnd.X);
            guidelineSet.GuidelinesY.Add(pointEnd.Y);
            dc.PushGuidelineSet(guidelineSet);
            var points = new Point[2];
            points[0] = new Point(pointStart.X, pointStart.Y);
            points[1] = new Point(pointEnd.X, pointEnd.Y);
            dc.DrawLine(pen, points[0], points[1]);
            dc.Pop();
        }

        private void DrawSnappedLinesBetweenPoints(DrawingContext context, double lineThickness, Brush color, params Point[] points)
        {
            if (points == null || context == null)
            {
                return;
            }

            var guidelineSet = new GuidelineSet();
            foreach (var point in points)
            {
                guidelineSet.GuidelinesX.Add(point.X);
                guidelineSet.GuidelinesY.Add(point.Y);
            }

            var half = lineThickness / 2;
            var adjustedPoints = points.Select(p => new Point(p.X + half, p.Y + half)).ToArray();
            context.PushGuidelineSet(guidelineSet);

            for (var i = 0; i < adjustedPoints.Length - 1; i = i + 2)
            {
                this.DrawSnappedLineBetweenPoints(context, points[i], points[i + 1], color);
            }

            context.Pop();
        }

        private string GetColorFilter(IDrawableCompartment compartment, IDrawableCompartment compartmentDetails = null)
        {
            if (this.IsReadOnly == false &&
                this.SelectedColorFilterFunc != null)
            {
                if (compartmentDetails != null)
                {
                    return this.SelectedColorFilterFunc.Invoke(compartmentDetails, compartment);
                }
                else
                {
                    return this.SelectedColorFilterFunc.Invoke(compartment, this.SelectedCompartment);
                }
            }

            return this.DefaultCompartmentColor;
        }

        private double GetCorrectedMarginOffset()
        {
            var ma = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            if (ma.M11 >= 2)
            {
                return ((int)ma.M11 + HALF_MARK) - ma.M11;
            }

            return 2 - ma.M11;
        }

        private double GetSizeOfPen()
        {
            var ma = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            return 1 / ma.M11;
        }

        private void ResizeCompartment(CompartmentViewModel compartment)
        {
            if (compartment == null)
            {
                return;
            }

            if (compartment.CompartmentDetails.Width == null || compartment.CompartmentDetails.Depth == null ||
                compartment.CompartmentDetails.XPosition == null || compartment.CompartmentDetails.YPosition == null)
            {
                return;
            }

            var compartmentOrigin = new Point
            {
                X = compartment.CompartmentDetails.XPosition.Value,
                Y = compartment.CompartmentDetails.YPosition.Value,
            };

            var convertedCompartmentOrigin = ConvertWithStandardOrigin(
                compartmentOrigin,
                this.OriginHorizontal,
                this.OriginVertical,
                this.DimensionWidth,
                this.DimensionHeight,
                (double)compartment.CompartmentDetails.Width,
                (double)compartment.CompartmentDetails.Depth);

            compartment.Top = ConvertMillimetersToPixel(
                convertedCompartmentOrigin.Y,
                this.TrayHeight,
                this.DimensionHeight);

            compartment.Left = ConvertMillimetersToPixel(
                convertedCompartmentOrigin.X,
                this.TrayWidth,
                this.DimensionWidth);

            var depth = ConvertMillimetersToPixel(
                (double)compartment.CompartmentDetails.Depth,
                this.TrayHeight,
                this.DimensionHeight);

            var width = ConvertMillimetersToPixel(
                (double)compartment.CompartmentDetails.Width,
                this.TrayWidth,
                this.DimensionWidth);

            compartment.Depth = depth;
            compartment.Width = width;
        }

        private void SetSelectedItem()
        {
            if (this.SelectedCompartment == null
                || this.Items == null)
            {
                this.SelectedItem = null;
                return;
            }

            var compartment = this.Items
                .AsCompartmentViewModel()
                .FirstOrDefault(c => c.CompartmentDetails.Id == this.SelectedCompartment.Id);

            if (compartment == null)
            {
                this.SelectedItem = null;
                return;
            }

            if (this.SelectedItem != null
                && ((CompartmentViewModel)this.SelectedItem).CompartmentDetails.Id == this.SelectedCompartment.Id)
            {
                return;
            }

            this.SelectedItem = compartment;
            this.UpdateColorCompartments();
        }

        private void UpdateColorCompartments()
        {
            if (this.SelectedColorFilterFunc == null)
            {
                return;
            }

            foreach (var item in this.Items.AsCompartmentViewModel())
            {
                item.ColorFill = this.GetColorFilter(this.SelectedCompartment, item.CompartmentDetails);
            }
        }

        private void UpdateIsCompartmentSelectable()
        {
            foreach (var compartment in this.Items.AsCompartmentViewModel())
            {
                compartment.IsSelectable = this.IsCompartmentSelectable;
            }
        }

        #endregion
    }
}
