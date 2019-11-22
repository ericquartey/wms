using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class LoadingUnitPartsConfiguration : IEntityTypeConfiguration<LoadingUnitPart>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LoadingUnitPart> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(p => p.Id);

            builder
                .HasOne(c => c.LoadingUnit)
                .WithMany(p => p.LoadingUnitParts)
                .HasForeignKey(c => c.LoadingUnitId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
