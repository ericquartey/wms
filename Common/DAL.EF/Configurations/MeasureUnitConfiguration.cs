using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class MeasureUnitConfiguration : IEntityTypeConfiguration<MeasureUnit>
  {
    public void Configure(EntityTypeBuilder<MeasureUnit> builder)
    {
      builder.HasKey(m => m.Id);

      builder.Property(m => m.Description).IsRequired();
    }
  }
}
