using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ServicingInfoConfiguration : IEntityTypeConfiguration<ServicingInfo>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ServicingInfo> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(s => s.Id);

            var installationDate = System.DateTime.Now.AddMonths(-34);
            builder.HasData(new ServicingInfo
            {
                Id = 1,
                InstallationDate = installationDate,
                LastServiceDate = null,
                NextServiceDate = null,
            });
        }

        #endregion
    }
}
