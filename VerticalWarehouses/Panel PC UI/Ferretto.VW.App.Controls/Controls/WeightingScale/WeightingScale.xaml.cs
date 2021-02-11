using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.Devices.WeightingScale;
using NLog;

namespace Ferretto.VW.App.Controls
{
    public partial class WeightingScale : UserControl
    {
        #region Fields

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

        public static readonly DependencyProperty ScaleNumberProperty = DependencyProperty.Register(
                                nameof(ScaleNumber),
                                typeof(int),
                                typeof(WeightingScale),
                                new PropertyMetadata(0));

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

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IWeightingScaleService weightingScaleService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IWeightingScaleService>();

        #endregion

        #region Constructors

        public WeightingScale()
        {
            this.InitializeComponent();

            this.Loaded += async (s, e) => await this.OnLoadedAsync(s, e);
            this.Unloaded += this.OnUnloaded;
        }

        #endregion

        #region Properties

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

        public int ScaleNumber
        {
            get => (int)this.GetValue(ScaleNumberProperty);
            set => this.SetValue(ScaleNumberProperty, value);
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

        private async Task OnLoadedAsync(object s, RoutedEventArgs e)
        {
            this.weightingScaleService.WeighAcquired += this.WeightingScaleService_WeighAcquired;

            this.logger.Debug("Loaded weighting scale component. Starting service ...");
            try
            {
                await this.weightingScaleService.StartAsync();
                await this.weightingScaleService.ClearMessageAsync();
                await this.weightingScaleService.ResetAverageUnitaryWeightAsync();

                this.weightingScaleService.StartWeightAcquisition();

                this.logger.Debug("Weighting scale component initialized.");
            }
            catch(InvalidOperationException ex)
            {
                this.logger.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private void OnUnloaded(object s, RoutedEventArgs e)
        {
            try
            {
                this.logger.Debug("Unloading weighting scale component ...");

                this.weightingScaleService.WeighAcquired -= this.WeightingScaleService_WeighAcquired;

                this.weightingScaleService.StopWeightAcquisition();

                this.logger.Debug("Weighting scale component unloaded.");

                this.weightingScaleService.StopAsync();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private async Task ResetAsync()
        {
            try
            {
                await this.weightingScaleService?.ResetAverageUnitaryWeightAsync();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private void Update(Accessories.Interfaces.WeightingScale.WeightAcquiredEventArgs e)
        {
            var currWeightSample = e.WeightSample;
            this.Quality = currWeightSample.Quality;
            this.ScaleNumber = currWeightSample.ScaleNumber;
            this.Tare = currWeightSample.Tare;
            this.UnitOfMeasure = currWeightSample.UnitOfMeasure;
            this.UnitsCount = currWeightSample.UnitsCount;
            this.Weight = currWeightSample.Weight;
            this.WeightInfo = this.Quality == SampleQuality.Stable || this.Quality == SampleQuality.Unstable
                ? this.Weight.ToString("0.0")
                : "-----.-";
            this.TareInfo = this.Tare.ToString("0.0 g");
        }

        private void WeightingScaleService_WeighAcquired(object sender, Accessories.Interfaces.WeightingScale.WeightAcquiredEventArgs e)
        {
            this.Update(e);
        }

        #endregion
    }
}
