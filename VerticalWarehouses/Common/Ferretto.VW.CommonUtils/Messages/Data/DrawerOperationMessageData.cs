﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class DrawerOperationMessageData : IDrawerOperationMessageData
    {
        #region Constructors

        public DrawerOperationMessageData(
            DrawerOperation operation,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Operation = operation;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public LoadingUnitDestination Destination { get; set; }

        public double DestinationHorizontalPosition { get; set; }

        public double DestinationVerticalPosition { get; set; }

        public bool IsDestinationPositive { get; set; }

        public bool IsSourcePositive { get; set; }

        public DrawerOperation Operation { get; set; }

        public LoadingUnitDestination Source { get; set; }

        public double SourceHorizontalPosition { get; set; }

        public double SourceVerticalPosition { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
