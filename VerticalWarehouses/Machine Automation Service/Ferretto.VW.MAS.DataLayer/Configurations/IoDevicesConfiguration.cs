using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class IoDevicesConfiguration : IEntityTypeConfiguration<IoDevice>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<IoDevice> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasIndex(i => i.Index)
                .IsUnique();

            builder.Property(i => i.IpAddress)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Net.IPAddress.Parse(stringValue));
        }

        #endregion
    }
}
