using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class AreaConfiguration : IEntityTypeConfiguration<Area>
  {
    public void Configure(EntityTypeBuilder<Area> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(a => a.Id);

      builder.Property(a => a.Name).IsRequired();
    }
  }
}
