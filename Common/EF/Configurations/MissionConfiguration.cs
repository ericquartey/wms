using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class MissionConfiguration : IEntityTypeConfiguration<Mission>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Mission> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Status)
                .HasColumnType("char(1)")
                .HasConversion(x => (char)x, x => (MissionStatus)System.Enum.ToObject(typeof(MissionStatus), x))
                .HasDefaultValueSql($"'{(char)MissionStatus.New}'");

            builder.Property(m => m.Type)
                .HasColumnType("char(1)")
                .HasConversion(x => (char)x, x => (MissionType)System.Enum.ToObject(typeof(MissionType), x));

            builder.Property(m => m.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Cell)
                .WithMany(s => s.Missions)
                .HasForeignKey(m => m.CellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.Bay)
                .WithMany(b => b.Missions)
                .HasForeignKey(m => m.BayId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.LoadingUnit)
                .WithMany(l => l.Missions)
                .HasForeignKey(m => m.LoadingUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.Compartment)
                .WithMany(c => c.Missions)
                .HasForeignKey(m => m.CompartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.ItemList)
                .WithMany(i => i.Missions)
                .HasForeignKey(m => m.ItemListId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.ItemListRow)
                .WithMany(i => i.Missions)
                .HasForeignKey(m => m.ItemListRowId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.Item)
                .WithMany(i => i.Missions)
                .HasForeignKey(m => m.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.MaterialStatus)
                .WithMany(m => m.Missions)
                .HasForeignKey(m => m.MaterialStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.PackageType)
                .WithMany(p => p.Missions)
                .HasForeignKey(m => m.PackageTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
