using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class MachineConfiguration : IEntityTypeConfiguration<Machine>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Machine> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasOne(m => m.Elevator)
                    .WithOne(e => e.Machine)
                        .HasForeignKey<Elevator>(e => e.MachineId)
                            .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasMany(m => m.Bays)
                    .WithOne(b => b.Machine)
                        .HasForeignKey<Bay>(e => e.MachineId)
                            .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasMany(m => m.Panels)
                     .WithOne(p => p.Machine)
                        .HasForeignKey<Bay>(e => e.MachineId)
                            .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
