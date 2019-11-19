using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ElevatorsConfoguration : IEntityTypeConfiguration<Elevator>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Elevator> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasOne(e => e.LoadingUnit)
                .WithOne()
                        //.HasForeignKey<Elevator>(e => e.LoadingUnitId)
                            .OnDelete(DeleteBehavior.Cascade);

            builder
               .HasOne(e => e.StructuralProperties)
                    .WithOne()
                      .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
