using Ferretto.VW.MAS.DataModels.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ErrorStatisticConfiguration : IEntityTypeConfiguration<ErrorStatistic>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ErrorStatistic> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(s => s.Code);

            foreach (var enumValue in System.Enum.GetValues(typeof(MachineErrors)))
            {
                builder.HasData(new ErrorStatistic { Code = (int)enumValue });
            }
        }

        #endregion
    }
}
