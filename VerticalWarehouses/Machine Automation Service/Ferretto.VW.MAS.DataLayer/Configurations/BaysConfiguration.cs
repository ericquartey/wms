using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class BaysConfiguration : IEntityTypeConfiguration<Bay>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Bay> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasIndex(b => b.Number)
                .IsUnique();

            builder.Property(c => c.ShutterType)
             .HasColumnType("text")
             .HasConversion(
                 enumValue => enumValue.ToString(),
                 stringValue => System.Enum.Parse<ShutterType>(stringValue));

            builder
                .Ignore(b => b.Status);
        }

        #endregion
    }
}
