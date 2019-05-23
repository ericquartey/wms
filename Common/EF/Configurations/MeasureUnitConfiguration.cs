﻿using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class MeasureUnitConfiguration : IEntityTypeConfiguration<MeasureUnit>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<MeasureUnit> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Id);

            builder.Property(a => a.Id)
                .HasColumnType("char(2)");

            builder.Property(m => m.Description)
                .IsRequired();
        }

        #endregion
    }
}
