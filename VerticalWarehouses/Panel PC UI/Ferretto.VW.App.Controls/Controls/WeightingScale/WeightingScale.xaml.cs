using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.Devices.WeightingScale;

namespace Ferretto.VW.App.Controls
{
    public partial class WeightingScale : UserControl
    {
        #region Fields

        public static readonly DependencyProperty AverageUnitWeightProperty = DependencyProperty.Register(
                                nameof(AverageUnitWeight),
                                typeof(float?),
                                typeof(WeightingScale),
                                new FrameworkPropertyMetadata
                                           (null,
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                            new PropertyChangedCallback(WeightingScale.OnAverageUnitWeightPropertyChanged)));

        public static readonly DependencyProperty IsWeightingScaleEnabledProperty = DependencyProperty.Register(
                                nameof(IsWeightingScaleEnabled),
                                typeof(bool),
                                typeof(WeightingScale),
                                new PropertyMetadata(false));

        public static readonly DependencyProperty QualityProperty = DependencyProperty.Register(
                                nameof(Quality),
                                typeof(SampleQuality),
                                typeof(WeightingScale),
                                new PropertyMetadata(SampleQuality.Unknown));

        public static readonly DependencyProperty ResetCommandProperty = DependencyProperty.Register(
                                nameof(ResetCommand),
                                typeof(ICommand),
                                typeof(WeightingScale),
                                new UIPropertyMetadata(null, Reset));

        public static readonly DependencyProperty ScaleNumberProperty = DependencyProperty.Register(
                                nameof(ScaleNumber),
                                typeof(int),
                                typeof(WeightingScale),
                                new PropertyMetadata(0));

        public static readonly DependencyProperty SetAverageUnitaryWeightCommandProperty = DependencyProperty.Register(
                                        nameof(SetAverageUnitaryWeightCommand),
                                typeof(ICommand),
                                typeof(WeightingScale),
                                new UIPropertyMetadata(null, SetAverageUnitaryWeight));

        public static readonly DependencyProperty TareInfoProperty = DependencyProperty.Register(
                                nameof(TareInfo),
                                typeof(string),
                                typeof(WeightingScale),
                                new PropertyMetadata(null));

        public static readonly DependencyProperty TareProperty = DependencyProperty.Register(
                                nameof(Tare),
                                typeof(float),
                                typeof(WeightingScale),
                                new PropertyMetadata(0.0f));

        public static readonly DependencyProperty UnitOfMeasureProperty = DependencyProperty.Register(
                                nameof(UnitOfMeasure),
                                typeof(string),
                                typeof(WeightingScale),
                                new PropertyMetadata(null));

        public static readonly DependencyProperty UnitsCountProperty = DependencyProperty.Register(
                                nameof(UnitsCount),
                                typeof(int?),
                                typeof(WeightingScale),
                                new PropertyMetadata(null));

        public static readonly DependencyProperty WeightInfoProperty = DependencyProperty.Register(
                                nameof(WeightInfo),
                                typeof(string),
                                typeof(WeightingScale),
                                new PropertyMetadata(null));

        public static readonly DependencyProperty WeightProperty = DependencyProperty.Register(
                                        nameof(Weight),
                                typeof(float),
                                typeof(WeightingScale),
                                new PropertyMetadata(0.0f));

        private readonly IWeightingScaleService weightingScaleService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IWeightingScaleService>();

        #endregion

        #region Constructors

        public WeightingScale()
        {
            this.InitializeComponent();
            this.weightingScaleService.ClearMessageAsync();
            this.weightingScaleService.ResetAverageUnitaryWeightAsync();
            this.IsVisibleChanged += this.WeightingScale_IsVisibleChanged;
            this.InitializeTimerSample();
        }

        #endregion

        #region Properties

        public float? AverageUnitWeight
        {
            get => (float?)this.GetValue(AverageUnitWeightProperty);
            set => this.SetValue(AverageUnitWeightProperty, value);
        }

        public bool IsWeightingScaleEnabled
        {
            get => (bool)this.GetValue(IsWeightingScaleEnabledProperty);
            set => this.SetValue(IsWeightingScaleEnabledProperty, value);
        }

        public SampleQuality Quality
        {
            get => (SampleQuality)this.GetValue(QualityProperty);
            set => this.SetValue(QualityProperty, value);
        }

        public ICommand ResetCommand
        {
            get => (ICommand)this.GetValue(ResetCommandProperty);
            set => this.SetValue(ResetCommandProperty, value);
        }

        public int ScaleNumber
        {
            get => (int)this.GetValue(ScaleNumberProperty);
            set => this.SetValue(ScaleNumberProperty, value);
        }

        public ICommand SetAverageUnitaryWeightCommand
        {
            get => (ICommand)this.GetValue(SetAverageUnitaryWeightCommandProperty);
            set => this.SetValue(SetAverageUnitaryWeightCommandProperty, value);
        }

        public float Tare
        {
            get => (float)this.GetValue(TareProperty);
            set => this.SetValue(TareProperty, value);
        }

        public string TareInfo
        {
            get => (string)this.GetValue(TareInfoProperty);
            set => this.SetValue(TareInfoProperty, value);
        }

        public string UnitOfMeasure
        {
            get => (string)this.GetValue(UnitOfMeasureProperty);
            set => this.SetValue(UnitOfMeasureProperty, value);
        }

        public int? UnitsCount
        {
            get => (int?)this.GetValue(UnitsCountProperty);
            set => this.SetValue(UnitsCountProperty, value);
        }

        public float Weight
        {
            get => (float)this.GetValue(WeightProperty);
            set => this.SetValue(WeightProperty, value);
        }

        public string WeightInfo
        {
            get => (string)this.GetValue(WeightInfoProperty);
            set => this.SetValue(WeightInfoProperty, value);
        }

        #endregion

        #region Methods

        public void SetAverageUnitaryWeight()
        {
            if (this.AverageUnitWeight.HasValue)
            {
                this.weightingScaleService?.SetAverageUnitaryWeightAsync(this.AverageUnitWeight.Value);
            }
        }

        private static void OnAverageUnitWeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WeightingScale weightingScale)
            {
                weightingScale.Reset();
                weightingScale.SetAverageUnitaryWeight();
            }
        }

        private static void Reset(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WeightingScale weightingScale)
            {
                weightingScale.Reset();
            }
        }

        private static void SetAverageUnitaryWeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WeightingScale weightingScale)
            {
                weightingScale.SetAverageUnitaryWeight();
            }
        }

        // TEST
        private void InitializeTimerSample()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += this.timer_Tick;
            timer.Start();
        }

        private void Reset()
        {
            this.weightingScaleService?.ResetAverageUnitaryWeightAsync();
        }

        // TEST
        private void timer_Tick(object sender, EventArgs e)
        {
            var weightSample = new TestWeight()
            {
                //AverageUnitWeight = 0.3f,
                Quality = (SampleQuality)new Random().Next(3, 4),
                ScaleNumber = 1,
                Tare = 10,
                UnitOfMeasure = "kg",
                UnitsCount = new Random().Next(10, 15),
                Weight = new Random().Next(100, 105)
            };

            var weightAcquired = new Accessories.Interfaces.WeightingScale.WeightAcquiredEventArgs(weightSample);

            this.Update(weightAcquired);
        }

        private void Update(Accessories.Interfaces.WeightingScale.WeightAcquiredEventArgs e)
        {
            var currWeightSample = e.WeightSample;
            //this.AverageUnitWeight = currWeightSample.AverageUnitWeight;
            this.Quality = currWeightSample.Quality;
            this.ScaleNumber = currWeightSample.ScaleNumber;
            this.Tare = currWeightSample.Tare;
            this.UnitOfMeasure = currWeightSample.UnitOfMeasure;
            this.UnitsCount = currWeightSample.UnitsCount;
            this.Weight = currWeightSample.Weight;
            this.WeightInfo = this.Weight.ToString("#.#");
            this.TareInfo = this.Tare.ToString("#.#");
        }

        private void WeightingScale_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.weightingScaleService.WeighAcquired += this.WeightingScaleService_WeighAcquired;
                try
                {
                    this.weightingScaleService.StartWeightAcquisition();
                }
                catch
                {
                }
            }
            else
            {
                this.weightingScaleService.StopWeightAcquisition();
                this.weightingScaleService.ResetAverageUnitaryWeightAsync();
            }
        }

        private void WeightingScaleService_WeighAcquired(object sender, Accessories.Interfaces.WeightingScale.WeightAcquiredEventArgs e)
        {
            this.Update(e);
        }

        #endregion

        #region Classes

        public class TestWeight : IWeightSample
        {
            #region Properties

            public float? AverageUnitWeight { get; set; }

            public SampleQuality Quality { get; set; }

            public int ScaleNumber { get; set; }

            public float Tare { get; set; }

            public string UnitOfMeasure { get; set; }

            public int? UnitsCount { get; set; }

            public float Weight { get; set; }

            #endregion
        }

        #endregion
    }
}
