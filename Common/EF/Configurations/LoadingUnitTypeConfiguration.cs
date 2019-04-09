﻿using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class LoadingUnitTypeConfiguration : IEntityTypeConfiguration<LoadingUnitType>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LoadingUnitType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.Property(c => c.Description).IsRequired();
            builder.Property(c => c.EmptyWeight).HasDefaultValue(0);

            builder.HasOne(l => l.LoadingUnitHeightClass)
                .WithMany(l => l.LoadingUnitTypes)
                .HasForeignKey(l => l.LoadingUnitHeightClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(l => l.LoadingUnitWeightClass)
                .WithMany(l => l.LoadingUnitTypes)
                .HasForeignKey(l => l.LoadingUnitWeightClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(l => l.LoadingUnitSizeClass)
                .WithMany(l => l.LoadingUnitTypes)
                .HasForeignKey(l => l.LoadingUnitSizeClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
