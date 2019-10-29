using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class SetupStatusConfiguration : IEntityTypeConfiguration<SetupStatus>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<SetupStatus> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(s => s.Id);

            builder.HasData(new SetupStatus { Id = 1 });
        }

        #endregion
    }
}
