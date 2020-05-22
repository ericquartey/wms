using FileHelpers;

namespace Ferretto.VW.InvertersParametersGenerator.Models
{
    [FixedLengthRecord(FixedMode.AllowLessChars)]
#pragma warning disable CA1051 // Do not declare visible instance fields
    public sealed class InverterParameterField
    {
        #region Fields

        [FieldOrder(1)]
        [FieldFixedLength(7)]
        [FieldTrim(TrimMode.Both)]
        public string Code;

        [FieldOrder(4)]
        [FieldFixedLength(3)]
        [FieldTrim(TrimMode.Both)]
        public string Comparable;

        [FieldOrder(3)]
        [FieldFixedLength(3)]
        [FieldTrim(TrimMode.Both)]
        public string Dataset;

        [FieldOrder(2)]
        [FieldFixedLength(34)]
        [FieldTrim(TrimMode.Both)]
        public string Description;

        [FieldOrder(5)]
        [FieldFixedLength(43)]
        [FieldTrim(TrimMode.Both)]
        public string Value;

        [FieldOrder(6)]
        [FieldFixedLength(3)]
        [FieldTrim(TrimMode.Both)]
        [FieldOptional]
        public string Writable;

        #endregion
    }

#pragma warning restore CA1051 // Do not declare visible instance fields
}
