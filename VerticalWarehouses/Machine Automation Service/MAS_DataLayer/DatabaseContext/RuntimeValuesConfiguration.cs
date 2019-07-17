
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer
{
    public class RuntimeValuesConfiguration : IEntityTypeConfiguration<RuntimeValue>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<RuntimeValue> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(cv => new { cv.CategoryName, cv.VarName });
        }

        #endregion
    }
}
