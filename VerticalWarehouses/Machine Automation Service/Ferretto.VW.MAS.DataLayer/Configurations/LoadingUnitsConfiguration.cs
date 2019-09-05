﻿using System;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class LoadingUnitsConfiguration : IEntityTypeConfiguration<LoadingUnit>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LoadingUnit> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.HasIndex(l => l.Code).IsUnique();
        }

        #endregion
    }
}
