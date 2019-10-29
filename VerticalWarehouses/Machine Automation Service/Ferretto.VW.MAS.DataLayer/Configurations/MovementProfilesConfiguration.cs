using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class MovementProfilesConfiguration : IEntityTypeConfiguration<MovementProfile>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<MovementProfile> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasIndex(p => p.Name).IsUnique();

            builder.Property(c => c.Name)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<MovementProfileType>(stringValue));
        }

        #endregion
    }
}
