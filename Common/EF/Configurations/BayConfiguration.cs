using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class BayConfiguration : IEntityTypeConfiguration<Bay>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Bay> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Description).IsRequired();

            builder.HasOne(b => b.BayType)
                .WithMany(b => b.Bays)
                .HasForeignKey(b => b.BayTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion Methods
    }
}
