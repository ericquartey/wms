using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
    public class BayConfiguration : IEntityTypeConfiguration<Bay>
    {
        public void Configure(EntityTypeBuilder<Bay> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Description).IsRequired();

            builder.HasOne(b => b.BayType)
                .WithMany(b => b.Bays)
                .HasForeignKey(b => b.BayTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
