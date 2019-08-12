using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Enums = Ferretto.Common.Resources.Enums;

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
                .HasConversion(
                    enumValue => (char)enumValue,
                    charValue => (Enums.MissionStatus)System.Enum.ToObject(typeof(Enums.MissionStatus), charValue))
                .HasDefaultValueSql($"'{(char)Enums.MissionStatus.New}'");

            builder.Property(m => m.CreationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(m => m.LastModificationDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(m => m.Bay)
                .WithMany(b => b.Missions)
                .HasForeignKey(m => m.BayId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(m => m.LoadingUnit)
                .WithMany(l => l.Missions)
                .HasForeignKey(m => m.LoadingUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
