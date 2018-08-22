using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class CellPositionConfiguration : IEntityTypeConfiguration<CellPosition>
  {
    public void Configure(EntityTypeBuilder<CellPosition> builder)
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
