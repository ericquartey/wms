using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ErrorConfiguration : IEntityTypeConfiguration<Error>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Error> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasIndex(e => e.Code)
                .IsUnique();

            builder
                .Property(m => m.OccurrenceDate)
                .IsRequired();

            builder
                .HasOne(e => e.Definition)
                .WithMany(d => d.Occurrences)
                .HasForeignKey(e => e.Code)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
