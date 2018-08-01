using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class AreaConfiguration : IEntityTypeConfiguration<Area>
  {
    public void Configure(EntityTypeBuilder<Area> builder)
    {
      builder.HasKey(a => a.Id);

      builder.Property(a => a.Name).IsRequired();
    }
  }
}
