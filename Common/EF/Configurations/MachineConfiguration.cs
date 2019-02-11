using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class MachineConfiguration : IEntityTypeConfiguration<Machine>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Machine> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(m => m.Id);

            builder.Property(m => m.MachineTypeId).IsRequired().HasColumnType("char(1)");
            builder.Property(m => m.Nickname).IsRequired();

            builder.HasOne(m => m.Aisle)
                .WithMany(a => a.Machines)
                .HasForeignKey(m => m.AisleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(m => m.MachineType)
                .WithMany(m => m.Machines)
                .HasForeignKey(m => m.MachineTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        #endregion
    }
}
