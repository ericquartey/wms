using System;
using System.Linq;
using System.Security.Cryptography;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class UsersProvider : IUsersProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public UsersProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public int? Authenticate(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ValueCannotBeNullOrWhiteSpace, nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(Resources.General.ValueCannotBeNullOrWhiteSpace, nameof(password));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                if (user != null
                    &&
                    IsPasswordValid(password, user))
                {
                    return user.AccessLevel;
                }
            }

            throw new EntityNotFoundException(userName);
        }

        public User Create(string userName, string password, int accessLevel)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ValueCannotBeNullOrWhiteSpace, nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(Resources.General.ValueCannotBeNullOrWhiteSpace, nameof(password));
            }

            lock (this.dataContext)
            {
                var existingUser = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);
                if (existingUser != null)
                {
                    return null;
                }

                var salt = Convert.ToBase64String(GeneratePasswordSalt());

                var passwordHash = GeneratePasswordHash(password, salt);

                var user = new User
                {
                    AccessLevel = accessLevel,
                    Name = userName,
                    PasswordSalt = salt,
                    PasswordHash = passwordHash,
                };

                this.dataContext.Users.Add(user);

                this.dataContext.SaveChanges();

                return user;
            }
        }

        private static string GeneratePasswordHash(string password, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashed;
        }

        private static byte[] GeneratePasswordSalt()
        {
            var salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        private static bool IsPasswordValid(string password, User user)
        {
            var providedPasswordHash = GeneratePasswordHash(password, user.PasswordSalt);

            return providedPasswordHash == user.PasswordHash;
        }

        #endregion
    }
}
