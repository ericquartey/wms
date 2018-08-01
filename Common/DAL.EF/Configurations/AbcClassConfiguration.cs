using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class AbcClassConfiguration : IEntityTypeConfiguration<AbcClass>
  {
    public void Configure(EntityTypeBuilder<AbcClass> builder)
    {
      builder.HasKey(a => a.Id);

      builder.Property(a => a.Id).HasColumnType("char(1)");
      builder.Property(a => a.Description).IsRequired();
    }
  }
}
