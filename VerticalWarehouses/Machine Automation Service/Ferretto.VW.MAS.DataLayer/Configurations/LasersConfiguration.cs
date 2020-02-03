using System.Net;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class LasersConfiguration : IEntityTypeConfiguration<Laser>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Laser> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.Property(i => i.IpAddress)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => IPAddress.Parse(stringValue));

            //builder.HasOne(a => a.Bay)
            //       .WithOne(b => b.Laser)
            //       .HasForeignKey<Bay>(b => b.Id);
        }

        #endregion
    }
}
