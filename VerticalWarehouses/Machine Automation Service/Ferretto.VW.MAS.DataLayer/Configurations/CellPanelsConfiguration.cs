using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class PanelsConfiguration : IEntityTypeConfiguration<CellPanel>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<CellPanel> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(p => p.Id);
        }

        #endregion
    }
}
