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

            builder.HasOne(i => i.Area)
                .WithMany(a => a.SchedulerRequests)
                .HasForeignKey(i => i.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.Bay)
               .WithMany(b => b.SchedulerRequests)
               .HasForeignKey(i => i.BayId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.Item)
               .WithMany(b => b.SchedulerRequests)
               .HasForeignKey(i => i.ItemId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.List)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(i => i.ListId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.ListRow)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(i => i.ListRowId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.LoadingUnit)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(i => i.LoadingUnitId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(i => i.LoadingUnitType)
               .WithMany(l => l.SchedulerRequests)
               .HasForeignKey(i => i.LoadingUnitTypeId)
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
