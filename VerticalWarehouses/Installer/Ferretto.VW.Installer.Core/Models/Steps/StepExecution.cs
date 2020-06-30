using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace Ferretto.VW.Installer.Core.Models.Steps
{
    public class StepExecution : BindableBase
    {
        #region Fields

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private TimeSpan? duration;

        private DateTime? endTime;

        private DateTime? startTime;

        private StepStatus status;

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

        #endregion

        #region Methods

        public void LogChar(char message)
        {
            if (message == '\0')
            {
                return;
            }

            this.stringBuilder.Append(message);
            this.RaisePropertyChanged(nameof(this.Log));
        }

        public void LogError(string message)
        {
            if (!this.stringBuilder.ToString().EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                this.stringBuilder.Append(Environment.NewLine);
            }

            this.stringBuilder
                .Append(message)
                .Append(Environment.NewLine);

            this.logger.Error(message);

            this.RaisePropertyChanged(nameof(this.Log));
        }

        public void LogInformation(string message)
        {
            if (!this.stringBuilder.ToString().EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase))
            {
                this.stringBuilder.Append(Environment.NewLine);
            }

            this.stringBuilder
              .Append(message)
              .Append(Environment.NewLine);

            this.logger.Debug(message);

            this.RaisePropertyChanged(nameof(this.Log));
        }

        #endregion
    }
}
