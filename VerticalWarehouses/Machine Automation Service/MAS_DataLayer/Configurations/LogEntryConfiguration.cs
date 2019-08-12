using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class LogEntriesConfiguration : IEntityTypeConfiguration<LogEntry>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LogEntry> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(e => e.Id);
        }

        #endregion
    }
}
