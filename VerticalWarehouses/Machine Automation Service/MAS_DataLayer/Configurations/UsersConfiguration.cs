using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<User> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(u => u.Name);

            builder.Property(u => u.PasswordHash).IsRequired();

            builder.Property(u => u.PasswordSalt).IsRequired();

            builder.HasData(new User
            {
                Name = "installer",
                AccessLevel = 0,
                PasswordHash = "DsWpG30CTZweMD4Q+LlgzrsGOWM/jx6enmP8O7RIrvU=",
                PasswordSalt = "2xw+hMIYBtLCoUqQGXSL0A=="
            });

            builder.HasData(new User
            {
                Name = "operator",
                AccessLevel = 2,
                PasswordHash = "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A=="
            });
        }

        #endregion
    }
}
