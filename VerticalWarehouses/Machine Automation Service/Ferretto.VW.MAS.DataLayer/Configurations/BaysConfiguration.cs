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
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasIndex(b => b.Number)
                .IsUnique();

            builder
                .Ignore(b => b.Status)
                .Ignore(b => b.IsDouble);
        }

        #endregion
    }
}
