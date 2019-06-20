using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class GlobalSettingsConfiguration : IEntityTypeConfiguration<GlobalSettings>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<GlobalSettings> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(a => a.Id);

            builder.Property(g => g.MinStepCompartment)
                .HasDefaultValue(5);
        }

        #endregion
    }
}
