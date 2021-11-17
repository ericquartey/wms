using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class LogoutSettingsConfiguration : IEntityTypeConfiguration<LogoutSettings>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LogoutSettings> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.Property(u => u.Timeout).IsRequired();

            builder.Property(u => u.BeginTime).IsRequired();

            builder.Property(u => u.EndTime).IsRequired();

            builder.Property(u => u.IsActive).IsRequired();
        }

        #endregion
    }
}
