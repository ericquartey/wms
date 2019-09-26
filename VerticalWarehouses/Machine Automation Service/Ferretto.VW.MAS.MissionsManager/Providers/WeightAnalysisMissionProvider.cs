using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    internal class WeightAnalysisMissionProvider : BaseProvider, IWeightAnalysisMissionProvider
    {

        #region Fields

        private readonly IVerticalAxisDataLayer verticalAxisDataLayer;

        #endregion

        #region Constructors

        public WeightAnalysisMissionProvider(
            IEventAggregator eventAggregator,
            IVerticalAxisDataLayer verticalAxisDataLayer)
            : base(eventAggregator)
        {
            this.verticalAxisDataLayer = verticalAxisDataLayer;
        }

        #endregion



        #region Methods

        public void Start(decimal initialPosition, decimal displacement, decimal netWeight, TimeSpan inPlaceSamplingDuration)
        {
            if (initialPosition < Math.Max(this.verticalAxisDataLayer.Offset, this.verticalAxisDataLayer.LowerBound))
            {
                throw new ArgumentOutOfRangeException(); // TODO add localized string message
            }

            if (displacement <= 0)
            {
                throw new ArgumentOutOfRangeException(); // TODO add localized string message
            }

            if (netWeight < 0)
            {
                throw new ArgumentOutOfRangeException(); // TODO add localized string message
            }

            if (inPlaceSamplingDuration.TotalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.SendCommandToMissionManager(
               new WeightAcquisitionCommandMessageData
               {
                   CommandAction = CommandAction.Start,
                   InitialPosition = initialPosition,
                   Displacement = displacement,
                   InPlaceSamplingDuration = inPlaceSamplingDuration,
                   NetWeight = netWeight,
               },
               "Start Weight Acquisition Procedure",
               MessageActor.Any,
               MessageType.WeightAcquisitionCommand,
               BayNumber.None);
        }

        public void Stop()
        {
            this.SendCommandToMissionManager(
              new WeightAcquisitionCommandMessageData
              {
                  CommandAction = CommandAction.Abort,
              },
              "Abort Weight Acquisition Procedure",
              MessageActor.Any,
              MessageType.WeightAcquisitionCommand,
              BayNumber.None);
        }

        #endregion
    }
}
