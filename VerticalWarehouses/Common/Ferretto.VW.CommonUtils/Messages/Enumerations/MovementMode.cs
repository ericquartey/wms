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
        /// Horizontal movements for calibration
        /// </summary>
        HorizontalCalibration,

        TorqueCurrentSampling,

        ProfileCalibration,

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
        /// Test in carousel bay (for calibration)
        /// </summary>
        BayTest,

        HorizontalForwardBackward,

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
        FindZero
    }
}
