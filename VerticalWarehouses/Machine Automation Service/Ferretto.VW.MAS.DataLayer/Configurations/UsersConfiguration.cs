using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class UsersConfiguration : IEntityTypeConfiguration<UserParameters>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<UserParameters> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasIndex(u => u.Name).IsUnique();

            builder.Property(u => u.PasswordHash).IsRequired();

            builder.Property(u => u.PasswordSalt).IsRequired();

            builder.Ignore(u => u.Validity);
        }

        #endregion
    }
}
