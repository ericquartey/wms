using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitStatusConfiguration : IEntityTypeConfiguration<LoadingUnitStatus>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitStatus> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(l => l.Id);

      builder.Property(l => l.Description).IsRequired();
    }
  }
}
