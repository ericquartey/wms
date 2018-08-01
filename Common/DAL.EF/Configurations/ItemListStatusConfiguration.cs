using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class ItemListStatusConfiguration : IEntityTypeConfiguration<ItemListStatus>
  {
    public void Configure(EntityTypeBuilder<ItemListStatus> builder)
    {
      builder.HasKey(i => i.Id);

      builder.Property(i => i.Description).IsRequired();
    }
  }
}
