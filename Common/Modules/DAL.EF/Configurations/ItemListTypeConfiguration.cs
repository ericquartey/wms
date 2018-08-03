using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class ItemListTypeConfiguration : IEntityTypeConfiguration<ItemListType>
  {
    public void Configure(EntityTypeBuilder<ItemListType> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(i => i.Id);

      builder.Property(i => i.Description).IsRequired();
    }
  }
}
