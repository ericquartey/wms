using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class ServicingInfoConfiguration : IEntityTypeConfiguration<ServicingInfo>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ServicingInfo> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(s => s.Id);

            builder.HasData(new ServicingInfo
            {
                Id = 1,
                InstallationDate = System.DateTime.Now.AddMonths(-34),
                LastServiceDate = System.DateTime.Now.AddMonths(-3),
                NextServiceDate = System.DateTime.Now.AddMonths(12),
            });
        }

        #endregion
    }
}
