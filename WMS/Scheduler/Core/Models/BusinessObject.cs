using System;

namespace Ferretto.WMS.Scheduler.Core
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Critical Code Smell",
        "S3874:\"out\" and \"ref\" parameters should not be used",
        Justification = "This code nned to be refactored in the scope of data service implementation")]
    public class BusinessObject
    {
        #region Constructors

        protected BusinessObject()
        {
        }

        #endregion Constructors

        #region Properties

        public int Id { get; set; }

        #endregion Properties

        #region Methods

        protected static bool SetIfPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be positive.");
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    member = value;
                    return true;
                }
            }

            return false;
        }

        protected static bool SetIfPositive(ref int member, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be positive.");
            }

            if (member != value)
            {
                member = value;
                return true;
            }

            return false;
        }

        protected static bool SetIfStrictlyPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    member = value;
                    return true;
                }
            }

            return false;
        }

        protected static bool SetIfStrictlyPositive(ref int member, int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
            }

            if (member != value)
            {
                member = value;
                return true;
            }

            return false;
        }

        #endregion Methods
    }
}
