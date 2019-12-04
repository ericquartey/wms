using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class MissionsConfiguration : IEntityTypeConfiguration<Mission>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Mission> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Id);

            builder.Property(m => m.LoadingUnitSource)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<LoadingUnitLocation>(stringValue));

            builder.Property(m => m.MissionType)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<MissionType>(stringValue));

            builder.Property(m => m.Status)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<MissionStatus>(stringValue));

            builder.Property(m => m.TargetBay)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<BayNumber>(stringValue));
        }

        #endregion
    }
}
