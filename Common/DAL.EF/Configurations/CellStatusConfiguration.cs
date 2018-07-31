using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class CellStatusConfiguration : IEntityTypeConfiguration<CellStatus>
  {
    public void Configure(EntityTypeBuilder<CellStatus> builder)
    {
      builder.HasKey(c => c.Id);

      builder.Property(c => c.Description).IsRequired();
    }
  }
}
