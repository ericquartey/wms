using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class DefaultLoadingUnitConfiguration : IEntityTypeConfiguration<DefaultLoadingUnit>
  {
    public void Configure(EntityTypeBuilder<DefaultLoadingUnit> builder)
    {
      builder.HasKey(d => d.Id);

      builder.HasOne(d => d.LoadingUnitType)
          .WithMany(l => l.DefaultLoadingUnits)
          .HasForeignKey(d => d.LoadingUnitTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
