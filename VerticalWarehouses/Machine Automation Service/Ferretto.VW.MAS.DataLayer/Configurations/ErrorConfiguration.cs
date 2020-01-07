using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ErrorConfiguration : IEntityTypeConfiguration<MachineError>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<MachineError> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .Property(m => m.OccurrenceDate)
                .IsRequired();

            builder
                .Ignore(l => l.Description)
                .Ignore(l => l.Reason);
        }

        #endregion
    }
}
