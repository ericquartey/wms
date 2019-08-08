using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.Common.EF.Configurations
{
    public class MissionOperationConfiguration : IEntityTypeConfiguration<MissionOperation>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<MissionOperation> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Status)
                .HasColumnType("char(1)")
                .HasConversion(
                    enumValue => (char)enumValue,
                    charValue => (Enums.MissionOperationStatus)System.Enum.ToObject(typeof(Enums.MissionOperationStatus), charValue))
                .HasDefaultValueSql($"'{(char)Enums.MissionOperationStatus.New}'");

            builder.Property(o => o.Type)
                .HasColumnType("char(1)")
                .HasConversion(
                    enumValue => (char)enumValue,
                    charValue => (Enums.MissionOperationType)System.Enum.ToObject(typeof(Enums.MissionOperationType), charValue));

            builder.Property(o => o.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(o => o.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(o => o.Mission)
                .WithMany(m => m.Operations)
                .HasForeignKey(o => o.MissionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(o => o.Compartment)
                .WithMany(c => c.MissionOperations)
                .HasForeignKey(o => o.CompartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(o => o.ItemList)
                .WithMany(i => i.MissionOperations)
                .HasForeignKey(o => o.ItemListId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(o => o.ItemListRow)
                .WithMany(i => i.MissionOperations)
                .HasForeignKey(o => o.ItemListRowId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(o => o.Item)
                .WithMany(i => i.MissionOperations)
                .HasForeignKey(o => o.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(o => o.MaterialStatus)
                .WithMany(o => o.MissionOperations)
                .HasForeignKey(o => o.MaterialStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(o => o.PackageType)
                .WithMany(p => p.MissionOperations)
                .HasForeignKey(o => o.PackageTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
