using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class PackageTypeConfiguration : IEntityTypeConfiguration<PackageType>
  {
    public void Configure(EntityTypeBuilder<PackageType> builder)
    {
      builder.HasKey(p => p.Id);

      builder.Property(p => p.Description).IsRequired();
    }
  }
}
