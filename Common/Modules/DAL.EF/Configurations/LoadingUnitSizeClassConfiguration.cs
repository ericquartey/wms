using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
    public class LoadingUnitSizeClassConfiguration : IEntityTypeConfiguration<LoadingUnitSizeClass>
    {
        public void Configure(EntityTypeBuilder<LoadingUnitSizeClass> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Description).IsRequired();
            builder.Property(l => l.Length).IsRequired();
            builder.Property(l => l.Width).IsRequired();
        }
    }
}
