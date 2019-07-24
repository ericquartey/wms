using Ferretto.VW.MAS.DataModels.Error;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class ErrorStatisticConfiguration : IEntityTypeConfiguration<ErrorStatistic>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ErrorStatistic> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(s => s.Code);

            builder.HasData(
                new ErrorStatistic { Code = 1001, TotalErrors = 11 },
                new ErrorStatistic { Code = 1002, TotalErrors = 7 },
                new ErrorStatistic { Code = 1003, TotalErrors = 5 },
                new ErrorStatistic { Code = 1004, TotalErrors = 3 },
                new ErrorStatistic { Code = 1005, TotalErrors = 2 },
                new ErrorStatistic { Code = 1006, TotalErrors = 1 },
                new ErrorStatistic { Code = 1007, TotalErrors = 1 },
                new ErrorStatistic { Code = 1008, TotalErrors = 1 },
                new ErrorStatistic { Code = 1009, TotalErrors = 1 },
                new ErrorStatistic { Code = 1010, TotalErrors = 1 },
                new ErrorStatistic { Code = 1011, TotalErrors = 1 },
                new ErrorStatistic { Code = 1012, TotalErrors = 1 },
                new ErrorStatistic { Code = 1013, TotalErrors = 1 },
                new ErrorStatistic { Code = 1014, TotalErrors = 1 },
                new ErrorStatistic { Code = 1015, TotalErrors = 1 },
                new ErrorStatistic { Code = 1016, TotalErrors = 0 });
        }

        #endregion
    }
}
