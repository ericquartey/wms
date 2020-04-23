using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class TcpIpAccessoryConfiguration : IEntityTypeConfiguration<TcpIpAccessory>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TcpIpAccessory> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Property(i => i.IpAddress)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Net.IPAddress.Parse(stringValue));
        }

        #endregion
    }
}
