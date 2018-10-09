using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Prism.Mvvm;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class BusinessObject<TId> : BindableBase, IBusinessObject<TId>
    {
        #region Fields

        private TId id;

        #endregion Fields

        #region Constructors

        protected BusinessObject()
        { }

        #endregion Constructors

        #region Properties

        public virtual TId Id { get => this.id; set => this.SetProperty(ref this.id, value); }

        #endregion Properties

        #region Methods

        protected static void SetIfPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    member = value;
                }
            }
        }

        protected static void SetIfPositive(ref int member, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
            }

            member = value;
        }

        protected static void SetIfStrictlyPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    member = value;
                }
            }
        }

        protected static void SetIfStrictlyPositive(ref int member, int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
            }

            member = value;
        }

        #endregion Methods
    }
}
