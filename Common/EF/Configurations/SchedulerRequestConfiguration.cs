using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class SchedulerRequestConfiguration : IEntityTypeConfiguration<SchedulerRequest>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SchedulerRequest> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(a => a.Id);

            builder.Property(a => a.OperationType)
                .HasColumnType("char(1)");

            builder.Property(i => i.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(s => s.Area)
                .WithMany(a => a.SchedulerRequests)
                .HasForeignKey(s => s.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(s => s.Bay)
               .WithMany(b => b.SchedulerRequests)
               .HasForeignKey(s => s.BayId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(s => s.Item)
               .WithMany(b => b.SchedulerRequests)
               .HasForeignKey(s => s.ItemId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(s => s.List)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(s => s.ListId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(s => s.ListRow)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(s => s.ListRowId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(s => s.LoadingUnit)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(s => s.LoadingUnitId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(s => s.LoadingUnitType)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(s => s.LoadingUnitTypeId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(c => c.MaterialStatus)
               .WithMany(m => m.SchedulerRequests)
               .HasForeignKey(c => c.MaterialStatusId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(c => c.PackageType)
               .WithMany(m => m.SchedulerRequests)
               .HasForeignKey(c => c.PackageTypeId)
               .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion Methods
    }
}
