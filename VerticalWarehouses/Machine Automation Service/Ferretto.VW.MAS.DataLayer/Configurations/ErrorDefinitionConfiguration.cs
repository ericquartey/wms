using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ErrorDefinitionConfiguration : IEntityTypeConfiguration<ErrorDefinition>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ErrorDefinition> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasIndex(d => d.Code)
                .IsUnique();

            builder
                .Property(d => d.Description)
                .IsRequired();

            builder
                .HasOne(d => d.Statistics)
                .WithOne(s => s.Error)
                .HasForeignKey<ErrorStatistic>(s => s.Code)
                .OnDelete(DeleteBehavior.ClientSetNull);

            foreach (var enumValue in typeof(MachineErrorCode).GetFields())
            {
                var attribute = enumValue.GetCustomAttributes(typeof(ErrorDescriptionAttribute), false).FirstOrDefault()
                    as ErrorDescriptionAttribute;

                if (attribute != null)
                {
                    builder.HasData(new ErrorDefinition
                    {
                        Id = (int)enumValue.GetValue(null),
                        Code = (int)enumValue.GetValue(null),
                        Description = attribute.Description,
                        Reason = attribute.Reason,
                        Severity = attribute.Severity,
                    });
                }
            }
        }

        #endregion
    }
}
