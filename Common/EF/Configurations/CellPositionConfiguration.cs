﻿using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellPositionConfiguration : IEntityTypeConfiguration<CellPosition>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<CellPosition> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Description)
                .IsRequired();
        }

        #endregion
    }
}
