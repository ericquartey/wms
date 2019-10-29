using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ShuttersConfiguration : IEntityTypeConfiguration<Shutter>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Shutter> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.Property(s => s.Type)
             .HasColumnType("text")
             .HasConversion(
                 enumValue => enumValue.ToString(),
                 stringValue => System.Enum.Parse<ShutterType>(stringValue));
        }

        #endregion
    }
}
