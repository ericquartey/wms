using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Ferretto.VW.Installer
{
    internal abstract class Step : BindableBase
    {
        #region Fields

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private TimeSpan? duration;

        private DateTime? endTime;

        private DateTime? startTime;

        private StepStatus status;

        #endregion

        #region Constructors

        public Step(int number, string title)
        {
            this.Title = title;
            this.Number = number;
        }

        #endregion

        #region Properties

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

        public bool MustRollback { get; set; }

        public int Number { get; }

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

        public async Task ApplyAsync()
        {
            if (this.Status is StepStatus.Done || this.Status is StepStatus.InProgress)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be executed because its status is {this.Status}.");
            }

            this.LogWriteLine($"Avvio dello step '{this.Title}'.");

            this.StartTime = DateTime.Now;
            var timer = new Timer(this.OnTimerTick, null, 0, 500);

            this.Status = StepStatus.InProgress;

            try
            {
                this.Status = await this.OnApplyAsync();
                timer.Dispose();
                this.EndTime = DateTime.Now;
                this.LogWriteLine($"Step completato con stato '{this.Status}'.");
            }
            catch
            {
                this.Status = StepStatus.Failed;
                this.LogWriteLine("Step fallito inaspettatamente.");
                timer.Dispose();
            }
        }

        public string InterpolateVariables(string value)
        {
            var regex = new Regex(@"\$\((?<var_name>[^\)]+)\)");

            var match = regex.Match(value);

            while (match.Success)
            {
                var varName = match.Groups["var_name"].Value;
                var varValue = ConfigurationManager.AppSettings.Get(varName);
                value = value.Replace(match.Value, varValue);
                match = match.NextMatch();
            }

            return value;
        }

        public async Task RollbackAsync()
        {
            if (this.Status != StepStatus.Failed && this.Status != StepStatus.Done)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be rolled back because its status is {this.Status}.");
            }

            this.LogWriteLine("Avvio rollback.");

            this.Status = StepStatus.RollingBack;

            try
            {
                this.Status = await this.OnRollbackAsync();
                this.LogWriteLine($"Rollback of step completed with status {this.Status}.");
            }
            catch
            {
                this.LogWriteLine("Rollback dello step fallito inaspettatamente.");

                this.Status = StepStatus.RollbackFailed;
            }
        }

        public override string ToString()
        {
            return $"{this.Number}: {this.Title} ({this.Status})" ?? base.ToString();
        }

        protected void LogWrite(char message)
        {
            if (message == '\0')
            {
                return;
            }

            this.stringBuilder.Append(message);
            this.RaisePropertyChanged(nameof(this.Log));
        }

        protected void LogWriteLine(string message)
        {
            if (!this.stringBuilder.ToString().EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                this.stringBuilder.Append(Environment.NewLine);
            }

            this.stringBuilder.Append(message).Append(Environment.NewLine);

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
