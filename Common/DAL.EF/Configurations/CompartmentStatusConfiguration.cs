using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class CompartmentStatusConfiguration : IEntityTypeConfiguration<CompartmentStatus>
  {
    public void Configure(EntityTypeBuilder<CompartmentStatus> builder)
    {
      builder.HasKey(c => c.Id);

      builder.Property(c => c.Description).IsRequired();
    }
  }
}
