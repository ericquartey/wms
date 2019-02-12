using System;
using System.Collections.Generic;
using System.Threading;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.RemoteIODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver
{
    public class NewRemoteIODriver : INewRemoteIODriver
    {
        #region Fields
        private const int ENCODER_CRADLE = 2;
        private const int ENCODER_ELEVATOR = 1;
        private const int N_DIGITAL_OUTPUT_LINES = 5;
        private const int DELAY_TIME = 150;

        private readonly IEventAggregator eventAggregator;
        private readonly IRemoteIO remoteIO;

        #endregion

        #region Constructors

        public NewRemoteIODriver(IEventAggregator eventAggregator, IRemoteIO remoteIO)
        {
            this.eventAggregator = eventAggregator;
            this.remoteIO = remoteIO;

            try
            {
                this.remoteIO.Connect();
            }
            catch (Exception ex)
            {
                throw new Exception("RemoteIO does not instantiate.", ex);
            }
        }

        #endregion

        #region Properties

        public List<bool> Inputs => this.remoteIO.Inputs;

        public List<bool> Outputs
        {
            set { this.remoteIO.Outputs = value; }
        }

        public string IPAddress
        {
            get { return this.remoteIO.IPAddress; }
            set { this.remoteIO.IPAddress = value; }
        }

        public int Port
        {
            get { return this.remoteIO.Port; }
            set { this.remoteIO.Port = value; }
        }

        #endregion

        #region Methods

        public void Disconnect()
        {
            try
            {
                this.remoteIO.Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception("RemoteIO does not instantiate.", ex);
            }
        }

        public void SwitchVerticalToHorizontal()
        {
            var digitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                digitalOutput.Add(false);
            }

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            digitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                digitalOutput.Add((i == ENCODER_CRADLE) ? true : false);
            }

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            // The calibrate horizontal axis routine is ended
            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Publish(new Notification_EventParameter(OperationType.SwitchVerticalToHorizontal, OperationStatus.End, "Switch Vertical to Horizontal Ended", Verbosity.Info));
        }

        public void SwitchHorizontalToVertical()
        {
            var digitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                digitalOutput.Add(false);
            }

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            digitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                digitalOutput.Add((i == ENCODER_ELEVATOR) ? true : false);
            }

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            // The calibrate horizontal axis routine is ended
            this.eventAggregator.GetEvent<RemoteIODriver_NotificationEvent>().Publish(new Notification_EventParameter(OperationType.SwitchHorizontalToVertical, OperationStatus.End, "Switch Horizontal to Vertical Ended", Verbosity.Info));
        }

        #endregion
    }
}
