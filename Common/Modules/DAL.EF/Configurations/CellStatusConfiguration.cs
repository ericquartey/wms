using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class CellStatusConfiguration : IEntityTypeConfiguration<CellStatus>
  {
    public void Configure(EntityTypeBuilder<CellStatus> builder)
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
