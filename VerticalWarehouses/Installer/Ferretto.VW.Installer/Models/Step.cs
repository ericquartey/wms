using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer
{
    internal abstract class Step : BindableBase
    {
        #region Fields

        private TimeSpan? duration;

        private DateTime? endTime;

        private string log = string.Empty;

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
            get => this.log;
            private set => this.SetProperty(ref this.log, value);
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

            this.StartTime = DateTime.Now;
            var timer = new Timer(this.OnTimerTick, null, 0, 500);

            this.Status = StepStatus.InProgress;

            try
            {
                this.Status = await this.OnApplyAsync();
                timer.Dispose();
                this.EndTime = DateTime.Now;
            }
            catch
            {
                this.Status = StepStatus.Failed;
                timer.Dispose();
            }
        }

        public async Task RollbackAsync()
        {
            if (this.Status != StepStatus.Failed)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be rolled back because its status is {this.Status}.");
            }

            this.Status = StepStatus.RollingBack;

            try
            {
                this.Status = await this.OnRollbackAsync();
            }
            catch
            {
                this.Status = StepStatus.RollbackFailed;
            }
        }

        public override string ToString()
        {
            return $"{this.Number}: {this.Title} ({this.Status})" ?? base.ToString();
        }

        protected void LogWrite(char message)
        {
            this.Log += message;
        }

        protected void LogWriteLine(string message)
        {
            this.Log += message + Environment.NewLine;
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
