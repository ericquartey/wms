using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class InvertersConfiguration : IEntityTypeConfiguration<Inverter>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Inverter> builder)
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

            builder.Property(i => i.Type)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<InverterType>(stringValue));
        }

        #endregion
    }
}
