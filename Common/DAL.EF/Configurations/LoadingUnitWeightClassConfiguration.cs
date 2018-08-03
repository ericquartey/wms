using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitWeightClassConfiguration : IEntityTypeConfiguration<LoadingUnitWeightClass>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitWeightClass> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(l => l.Id);

      builder.Property(l => l.Description).IsRequired();
      builder.Property(l => l.MinWeight).IsRequired();
      builder.Property(l => l.MaxWeight).IsRequired();
    }
  }
}
