using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitRangeConfiguration : IEntityTypeConfiguration<LoadingUnitRange>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitRange> builder)
    {
      builder.HasKey(l => l.Id);

      builder.HasOne(l => l.Area)
          .WithMany(a => a.LoadingUnitRanges)
          .HasForeignKey(l => l.AreaId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
