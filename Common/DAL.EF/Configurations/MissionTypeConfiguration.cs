using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class MissionTypeConfiguration : IEntityTypeConfiguration<MissionType>
  {
    public void Configure(EntityTypeBuilder<MissionType> builder)
    {
      builder.HasKey(m => m.Id);

      builder.Property(m => m.Id).HasColumnType("char(2)");
      builder.Property(m => m.Description).IsRequired();
    }
  }
}
