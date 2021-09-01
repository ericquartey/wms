using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class InverterParametersConfiguration : IEntityTypeConfiguration<InverterParameter>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<InverterParameter> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder
                .HasOne(c => c.Inverter)
                .WithMany(p => p.Parameters)
                .HasForeignKey(c => c.InverterId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
