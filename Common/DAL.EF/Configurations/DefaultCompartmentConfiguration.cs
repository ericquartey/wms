using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class DefaultCompartmentConfiguration : IEntityTypeConfiguration<DefaultCompartment>
    {
        public void Configure(EntityTypeBuilder<DefaultCompartment> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Note)
                .HasColumnType("text");

            builder.HasOne(d => d.DefaultLoadingUnit)
                .WithMany(d => d.DefaultCompartments)
                .HasForeignKey(d => d.DefaultLoadingUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(d => d.CompartmentType)
                .WithMany(c => c.DefaultCompartments)
                .HasForeignKey(d => d.CompartmentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
