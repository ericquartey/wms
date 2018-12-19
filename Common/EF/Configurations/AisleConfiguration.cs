using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class AisleConfiguration : IEntityTypeConfiguration<Aisle>
    {
        public void Configure(EntityTypeBuilder<Aisle> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name).IsRequired();

            builder.HasOne(a => a.Area)
                .WithMany(a => a.Aisles)
                .HasForeignKey(a => a.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
