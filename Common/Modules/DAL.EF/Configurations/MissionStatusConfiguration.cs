using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class MissionStatusConfiguration : IEntityTypeConfiguration<MissionStatus>
  {
    public void Configure(EntityTypeBuilder<MissionStatus> builder)
    {
      builder.HasKey(m => m.Id);

      builder.Property(m => m.Id).HasColumnType("char(1)");
      builder.Property(m => m.Description).IsRequired();
    }
  }
}
