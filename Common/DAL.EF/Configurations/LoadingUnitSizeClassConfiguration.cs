using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitSizeClassConfiguration : IEntityTypeConfiguration<LoadingUnitSizeClass>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitSizeClass> builder)
    {
      builder.HasKey(l => l.Id);

      builder.Property(l => l.Description).IsRequired();
      builder.Property(l => l.Length).IsRequired();
      builder.Property(l => l.Width).IsRequired();
    }
  }
}
