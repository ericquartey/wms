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
                .HasKey(b => b.Index);

            builder
                .HasIndex(b => b.IpAddress)
                .IsUnique();

            builder
                .Ignore(b => b.Status);
        }

        #endregion
    }
}
