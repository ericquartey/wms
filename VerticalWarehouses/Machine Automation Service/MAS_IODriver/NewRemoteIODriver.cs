using System;
using System.Collections.Generic;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.RemoteIODriver;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver
{
    public class NewRemoteIODriver : INewRemoteIODriver
    {
        #region Fields

        private const int DELAY_TIME = 75;

        private const int ENCODER_CRADLE = 2;

        private const int ENCODER_ELEVATOR = 1;

        private const int N_DIGITAL_OUTPUT_LINES = 5;

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

        public string IPAddress
        {
            get => this.remoteIO.IPAddress;
            set => this.remoteIO.IPAddress = value;
        }

        public List<bool> Outputs
        {
            set => this.remoteIO.Outputs = value;
        }

        public int Port
        {
            get => this.remoteIO.Port;
            set => this.remoteIO.Port = value;
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

        public void SwitchHorizontalToVertical()
        {
            var digitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++) digitalOutput.Add(false);

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            digitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++) digitalOutput.Add(i == ENCODER_ELEVATOR ? true : false);

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Switch Horizontal to Vertical Ended", MessageActor.Any, MessageActor.IODriver,
                MessageType.SwitchHorizontalToVertical, MessageStatus.OperationEnd));
        }

        public void SwitchVerticalToHorizontal()
        {
            var digitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++) digitalOutput.Add(false);

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            digitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++) digitalOutput.Add(i == ENCODER_CRADLE ? true : false);

            this.remoteIO.Outputs = digitalOutput;
            Thread.Sleep(DELAY_TIME);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null,
                "Switch Vertical to Horizontal Ended", MessageActor.Any, MessageActor.IODriver,
                MessageType.SwitchVerticalToHorizontal, MessageStatus.OperationEnd));
        }

        #endregion
    }
}
