using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Installer.Core
{
    public abstract class Step : BindableBase
    {
        #region Fields

        private const string EmbeddedInstallationFilePath = "EmbeddedInstallationFilePath";

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private TimeSpan? duration;

        private DateTime? endTime;

        private DateTime? startTime;

        private StepStatus status;

        #endregion

        #region Constructors

        public Step(int number, string title, string description, bool skipOnResume)
        {
            this.Title = title;
            this.Number = number;
            this.Description = description;
            this.SkipOnResume = skipOnResume;
        }

        #endregion

        #region Properties

        public string Description { get; }
        public bool SkipOnResume { get; }

        public TimeSpan? Duration
        {
            get => this.duration;
            set => this.SetProperty(ref this.duration, value);
        }

        public DateTime? EndTime
        {
            get => this.endTime;
            set
            {
                if (this.SetProperty(ref this.endTime, value))
                {
                    this.duration = this.endTime.HasValue && this.startTime.HasValue ? this.endTime - this.startTime : null;
                }
            }
        }

        public string Log
        {
            get => this.stringBuilder.ToString();
            private set
            {
                this.stringBuilder.Clear();
                this.stringBuilder.Append(value);
                this.RaisePropertyChanged();
            }
        }

        public int Number { get; }

        public bool SkipRollback { get; set; }

        public DateTime? StartTime
        {
            get => this.startTime;
            set => this.SetProperty(ref this.startTime, value);
        }

        public StepStatus Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        public string Title { get; }

        #endregion

        #region Methods

        public static string InterpolateVariables(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var regex = new Regex(@"\$\((?<var_name>[^\)]+)\)");

            var match = regex.Match(value);

            var interpolatedValue = value;
            while (match.Success)
            {
                var varName = match.Groups["var_name"].Value;

                string varValue;
                if (varName == EmbeddedInstallationFilePath)
                {
                    varValue = Process.GetCurrentProcess().MainModule.FileName;
                }
                else
                {
                    varValue = ConfigurationManager.AppSettings.Get(varName);
                    if (varValue is null)
                    {
                        throw new Exception($"La chiave di configurazione '{varName}' non esiste.");
                    }
                }

                interpolatedValue = interpolatedValue.Replace(match.Value, varValue);
                match = match.NextMatch();
            }

            return interpolatedValue;
        }

        public async Task ApplyAsync()
        {
            if (this.Status is StepStatus.Done || this.Status is StepStatus.InProgress)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be executed because its status is {this.Status}.");
            }

            this.LogInformation($"Avvio dello step '{this.Title}'.");

            this.StartTime = DateTime.Now;
            var timer = new Timer(this.OnTimerTick, null, 0, 500);

            this.Status = StepStatus.InProgress;

            try
            {
                this.Status = await this.OnApplyAsync();
                timer.Dispose();
                this.EndTime = DateTime.Now;
                this.LogInformation($"Step completato con stato '{this.Status}'.");
            }
            catch (Exception ex)
            {
                this.Status = StepStatus.Failed;
                this.LogError($"Step fallito inaspettatamente: {ex.Message}");
                timer.Dispose();
            }
        }

        public async Task RollbackAsync()
        {
            if (this.Status != StepStatus.Failed && this.Status != StepStatus.Done)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be rolled back because its status is {this.Status}.");
            }

            this.LogInformation("Avvio rollback.");

            this.Status = StepStatus.RollingBack;

            try
            {
                this.Status = await this.OnRollbackAsync();
                this.LogInformation($"Rollback of step completed with status {this.Status}.");
            }
            catch
            {
                this.LogInformation("Rollback dello step fallito inaspettatamente.");

                this.Status = StepStatus.RollbackFailed;
            }
        }

        public override string ToString()
        {
            return $"{this.Number}: {this.Title} ({this.Status})" ?? base.ToString();
        }

        protected void LogChar(char message)
        {
            if (message == '\0')
            {
                return;
            }

            this.stringBuilder.Append(message);
            this.RaisePropertyChanged(nameof(this.Log));
        }

        protected void LogError(string message)
        {
            if (!this.stringBuilder.ToString().EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                this.stringBuilder.Append(Environment.NewLine);
            }

            this.stringBuilder
                .Append($@"{message}")

                .Append(Environment.NewLine);

            this.logger.Error(message);

            this.RaisePropertyChanged(nameof(this.Log));
        }

        protected void LogInformation(string message)
        {
            if (!this.stringBuilder.ToString().EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                this.stringBuilder.Append(Environment.NewLine);
            }

            this.stringBuilder
              .Append($"{message}")
              .Append(Environment.NewLine);

            this.logger.Debug(message);

            this.RaisePropertyChanged(nameof(this.Log));
        }

        protected abstract Task<StepStatus> OnApplyAsync();

        protected abstract Task<StepStatus> OnRollbackAsync();

        private void OnTimerTick(object state)
        {
            this.Duration = DateTime.Now - this.StartTime;
        }

        #endregion
    }
}
