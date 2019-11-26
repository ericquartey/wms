using System;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.Utils.Source.Filters.Interfaces;

namespace Ferretto.VW.Utils.Source.Filters
{
#pragma warning disable SA1311 // Static readonly fields must begin with upper-case letter
#pragma warning disable SA1130 // Use lambda syntax
#pragma warning disable SA1008 // Opening parenthesis must be spaced correctly
#pragma warning disable S3257 // Declarations and initializations should be as concise as possible
#pragma warning disable S3353 // Unchanged local variables should be "const"

    public class EditFilter : IFilter
    {
        #region Fields

        private static readonly Func<IDrawableCompartment, IDrawableCompartment, string> colorFunc =

            delegate (IDrawableCompartment compartment, IDrawableCompartment selected)
            {
                var color = "#5A5A5A";
                return color;
            };

        #endregion

        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> ColorFunc => colorFunc;

        public string Description => "Compartment";

        public int Id => 2;

        public IDrawableCompartment Selected { get; set; }

        #endregion
    }

#pragma warning restore S3353 // Unchanged local variables should be "const"
#pragma warning restore SA1311 // Static readonly fields must begin with upper-case letter
#pragma warning restore S3257 // Declarations and initializations should be as concise as possible
#pragma warning restore SA1008 // Opening parenthesis must be spaced correctly
#pragma warning restore SA1130 // Use lambda syntax
}
