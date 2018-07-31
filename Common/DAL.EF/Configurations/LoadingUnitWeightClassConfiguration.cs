using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitWeightClassConfiguration : IEntityTypeConfiguration<LoadingUnitWeightClass>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitWeightClass> builder)
    {
      builder.HasKey(l => l.Id);

      builder.Property(l => l.Description).IsRequired();
      builder.Property(l => l.MinWeight).IsRequired();
      builder.Property(l => l.MaxWeight).IsRequired();
    }
  }
}
