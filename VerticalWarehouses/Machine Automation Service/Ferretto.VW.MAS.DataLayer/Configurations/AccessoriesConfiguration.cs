using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class AccessoriesConfiguration : IEntityTypeConfiguration<Accessory>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Accessory> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
                .Property(a => a.IsConfigured)
                .HasConversion(
                    stringValue => "n/a",
                    stringValue => "n/a");

            builder
                .Property(a => a.IsEnabled)
                .HasConversion(
                    stringValue => "n/a",
                    stringValue => "n/a");
        }

        #endregion
    }
}
