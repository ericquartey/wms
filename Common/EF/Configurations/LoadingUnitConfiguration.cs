using System;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.Common.EF.Configurations
{
    public class LoadingUnitConfiguration : IEntityTypeConfiguration<LoadingUnit>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LoadingUnit> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.HasIndex(l => l.Code)
                .IsUnique();

            builder.Property(l => l.Code)
                .IsRequired();

            builder.Property(l => l.AbcClassId)
                .IsRequired()
                .HasColumnType("char(1)");

            builder.Property(l => l.ReferenceType)
                .IsRequired()
                .HasColumnType("char(1)")
                .HasConversion(
                    enumValue => (char)enumValue,
                    charValue => (Enums.ReferenceType)Enum.ToObject(typeof(Enums.ReferenceType), charValue));

            builder.Property(l => l.LoadingUnitStatusId)
                .IsRequired()
                .HasColumnType("char(1)");

            builder.Property(l => l.Note)
                .HasColumnType("text");

            builder.Property(l => l.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(l => l.MissionsCount)
                .HasDefaultValue(0);

            builder.HasOne(l => l.AbcClass)
                .WithMany(a => a.LoadingUnits)
                .HasForeignKey(l => l.AbcClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(l => l.Cell)
                .WithMany(c => c.LoadingUnits)
                .HasForeignKey(l => l.CellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(l => l.CellPosition)
                .WithMany(c => c.LoadingUnits)
                .HasForeignKey(l => l.CellPositionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(l => l.LoadingUnitType)
                .WithMany(l => l.LoadingUnits)
                .HasForeignKey(l => l.LoadingUnitTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(l => l.LoadingUnitStatus)
                .WithMany(l => l.LoadingUnits)
                .HasForeignKey(l => l.LoadingUnitStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
