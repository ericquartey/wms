using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class CellConfigurationConfiguration : IEntityTypeConfiguration<Models.CellConfiguration>
  {
    public void Configure(EntityTypeBuilder<Models.CellConfiguration> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(c => c.Id);

      builder.Property(c => c.Description).IsRequired();
    }
  }
}
