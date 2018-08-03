using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class CellSizeClassConfiguration : IEntityTypeConfiguration<CellSizeClass>
  {
    public void Configure(EntityTypeBuilder<CellSizeClass> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(c => c.Id);

      builder.Property(c => c.Description).IsRequired();
      builder.Property(c => c.Length).IsRequired();
      builder.Property(c => c.Width).IsRequired();
    }
  }
}
