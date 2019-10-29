using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<User> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasIndex(u => u.Name).IsUnique();

            builder.Property(u => u.PasswordHash).IsRequired();

            builder.Property(u => u.PasswordSalt).IsRequired();

            builder.HasData(new User
            {
                Id = -1,
                Name = "installer",
                AccessLevel = 0,
                PasswordHash = "DsWpG30CTZweMD4Q+LlgzrsGOWM/jx6enmP8O7RIrvU=",
                PasswordSalt = "2xw+hMIYBtLCoUqQGXSL0A==",
            });

            builder.HasData(new User
            {
                Id = -2,
                Name = "operator",
                AccessLevel = 2,
                PasswordHash = "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A==",
            });
        }

        #endregion
    }
}
