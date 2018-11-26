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
                .HasDefaultValueSql(((char)MissionStatus.New).ToString());

            builder.Property(m => m.Type)
                .HasColumnType("char(1)")
                .HasConversion(x => (char)x, x => (MissionType)System.Enum.ToObject(typeof(MissionType), x));

            builder.HasOne(m => m.SourceCell)
                .WithMany(s => s.SourceMissions)
                .HasForeignKey(m => m.SourceCellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.DestinationCell)
                .WithMany(s => s.DestinationMissions)
                .HasForeignKey(m => m.DestinationCellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.SourceBay)
                .WithMany(b => b.SourceMissions)
                .HasForeignKey(m => m.SourceBayId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.DestinationBay)
                .WithMany(b => b.DestinationMissions)
                .HasForeignKey(m => m.DestinationBayId)
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

        #endregion Methods
    }
}
