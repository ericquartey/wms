using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class MaterialStatusConfiguration : IEntityTypeConfiguration<MaterialStatus>
    {
        public void Configure(EntityTypeBuilder<MaterialStatus> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Description).IsRequired();
        }
    }
}
