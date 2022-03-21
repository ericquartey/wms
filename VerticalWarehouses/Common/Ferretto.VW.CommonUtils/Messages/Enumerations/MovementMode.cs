namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum MovementMode
    {
        NotSpecified,

        /// <summary>
        /// Movements in horizontal, vertical
        /// </summary>
        Position,

        /// <summary>
        /// Movements in vertical and measure weight
        /// </summary>
        PositionAndMeasureWeight,

        /// <summary>
        /// Movements in horizontal and measure height
        /// </summary>
        PositionAndMeasureProfile,

        /// <summary>
        /// Repetitive vertical movements
        /// </summary>
        BeltBurnishing,

        /// <summary>
        /// Horizontal movements for calibration (measure zero plate)
        /// </summary>
        HorizontalCalibration,

        /// <summary>
        /// Elevator Horizontal axis movement (automatic resolution change)
        /// </summary>
        HorizontalResolution,

        TorqueCurrentSampling,

        /// <summary>
        /// Horizontal movements for profile barrier positioning
        /// </summary>
        ProfileCalibration,

        /// <summary>
        /// Elevator Vertical axis movement and measure profile (automatic resolution change)
        /// </summary>
        ProfileResolution,

        /// <summary>
        /// Movements for shutter
        /// </summary>
        ShutterPosition,

        /// <summary>
        /// Repetitive movements for shutter
        /// </summary>
        ShutterTest,

        /// <summary>
        /// Movements in carousel bay
        /// </summary>
        BayChain,

        /// <summary>
        /// Manual movements in carousel bay
        /// </summary>
        BayChainManual,

        /// <summary>
        /// Test in carousel bay (automatic resolution change)
        /// </summary>
        BayTest,

        /// <summary>
        /// Movements in external bay
        /// </summary>
        ExtBayChain,

        /// <summary>
        /// Manual movements in external bay
        /// </summary>
        ExtBayChainManual,

        /// <summary>
        /// Test in external bay (for calibration)
        /// </summary>
        ExtBayTest,

        /// <summary>
        /// Test in external bay
        /// </summary>
        DoubleExtBayTest,

        /// <summary>
        /// Find lost zero
        /// </summary>
        FindZero,

        /// <summary>
        /// Find lost zero bay chain
        /// </summary>
        BayChainFindZero
    }
}
