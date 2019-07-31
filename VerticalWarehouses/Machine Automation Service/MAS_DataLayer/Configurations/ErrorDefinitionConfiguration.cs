using System.Linq;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DataModels.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class ErrorDefinitionConfiguration : IEntityTypeConfiguration<ErrorDefinition>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ErrorDefinition> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Code);

            builder.Property(m => m.Description)
                .IsRequired();

            builder
                .HasOne(e => e.Statistics)
                .WithOne(s => s.Error)
                .HasForeignKey<ErrorStatistic>(s => s.Code)
                .OnDelete(DeleteBehavior.ClientSetNull);

            foreach (var enumValue in typeof(MachineErrors).GetFields())
            {
                var attribute = enumValue.GetCustomAttributes(typeof(ErrorDescriptionAttribute), false).FirstOrDefault()
                    as ErrorDescriptionAttribute;

                if (attribute != null)
                {
                    builder.HasData(new ErrorDefinition
                    {
                        Code = (int)enumValue.GetValue(null),
                        Description = attribute.Description,
                        Reason = attribute.Reason,
                        Severity = attribute.Severity
                    });
                }
            }
        }

        #endregion
    }
}
