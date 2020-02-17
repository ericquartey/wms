using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Prism.Events;

namespace Ferretto.VW.MAS.LaserDriver
{
    internal class LaserProvider : ILaserProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public LaserProvider(IEventAggregator eventAggregator, IBaysDataProvider baysDataProvider)
        {
            this.eventAggregator = eventAggregator ?? throw new System.ArgumentNullException(nameof(eventAggregator));
            this.baysDataProvider = baysDataProvider ?? throw new System.ArgumentNullException(nameof(baysDataProvider));
        }

        #endregion

        #region Methods

        public void MoveToPositionAndSwitchOn(BayNumber bayNumber, double x, double y)
        {
            this.eventAggregator
                .GetEvent<FieldCommandEvent>()
                .Publish(new Utils.Messages.FieldCommandMessage(
                    new LaserFieldMessageData { X = (int)x, Y = (int)y },
                    $"Move laser to position ({x},{y}) and switch on for bay {bayNumber}",
                     FieldMessageActor.LaserDriver,
                    FieldMessageActor.Any,
                    FieldMessageType.LaserMoveAndSwitchOn,
                    (byte)bayNumber));
        }

        public void SwitchOff(BayNumber bayNumber)
        {
            this.eventAggregator
                .GetEvent<FieldCommandEvent>()
                .Publish(new Utils.Messages.FieldCommandMessage(
                   null,
                    $"Switch off laser on bay {bayNumber}",
                     FieldMessageActor.LaserDriver,
                    FieldMessageActor.Any,
                    FieldMessageType.LaserOff,
                    (byte)bayNumber));
        }

        public void SwitchOn(BayNumber bayNumber)
        {
            this.eventAggregator
                .GetEvent<FieldCommandEvent>()
                .Publish(new Utils.Messages.FieldCommandMessage(
                   null,
                    $"Switch on laser on bay {bayNumber}",
                     FieldMessageActor.LaserDriver,
                    FieldMessageActor.Any,
                    FieldMessageType.LaserOn,
                    (byte)bayNumber));
        }

        #endregion
    }
}
