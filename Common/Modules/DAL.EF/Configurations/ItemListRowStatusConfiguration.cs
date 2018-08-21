using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class ItemListRowStatusConfiguration : IEntityTypeConfiguration<ItemListRowStatus>
  {
    public void Configure(EntityTypeBuilder<ItemListRowStatus> builder)
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
