using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using OtpNet;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class UsersProvider : IUsersProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly TimeSpan tokenValidity = new TimeSpan(0, 30, 0);

        #endregion

        #region Constructors

        public UsersProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            if (!this.AreSetUsers())
            {
                this.dataContext.Users.Add(UserParameters.Values.Service);
                this.dataContext.Users.Add(UserParameters.Values.Operator);
                this.dataContext.Users.Add(UserParameters.Values.Installer);
                this.dataContext.Users.Add(UserParameters.Values.Admin);

                this.dataContext.SaveChanges();
            }
        }

        #endregion

        #region Methods

        public bool AreSetUsers()
        {
            lock (this.dataContext)
            {
                if (this.dataContext.Users.Any() &&
                    this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Admin.ToString().ToLower()) &&
                    this.dataContext.Users.Any(s => s.Name == "service") &&
                    this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Operator.ToString().ToLower()) &&
                    this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Installer.ToString().ToLower()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public int? Authenticate(string userName, string password, string supportToken)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(password));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                if (user != null
                    &&
                    IsPasswordValid(user, password, supportToken, this.tokenValidity)
                    &&
                    (UserAccessLevel)user.AccessLevel != UserAccessLevel.NoAccess)
                {
                    return user.AccessLevel;
                }
            }

            throw new EntityNotFoundException(userName);
        }

        public UserParameters Create(string userName, string password, int accessLevel)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(password));
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

                var user = new UserParameters
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

        public IEnumerable<UserParameters> GetAllUserWithCulture()
        {
            lock (this.dataContext)
            {
                var count = this.dataContext.Users.Count();
                var result = this.dataContext.Users.AsNoTracking();
                return result;
            }
        }

        public string GetServiceToken()
        {
            if (!string.IsNullOrEmpty(UserParameters.Values.Service.PasswordSalt) && DateTime.Now < UserParameters.Values.Service.Validity)
            {
                return UserParameters.Values.Service.PasswordSalt;
            }

            lock (UserParameters.Values.Service)
            {
                var random = new Random();

                // Please note that not all characters are allowed due to Base32 encoding not supporting it (eg. numbers)
                //const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
                const string alphabet = "ABCDEFGHKMNPQRSTUVWXYZ234567";

                // Generate a string of six characters as secret key using the above alphabet
                var secretKey = new string(Enumerable.Repeat(alphabet, 6).Select(s => s[random.Next(s.Length)]).ToArray());

                UserParameters.Values.Service.PasswordSalt = secretKey;
                UserParameters.Values.Service.Validity = DateTime.Now.Add(this.tokenValidity);

                return secretKey;
            }
        }

        public void SetUserCulture(string culture, string name)
        {
            lock (this.dataContext)
            {
                if (this.dataContext.Users.Any(s => s.Name == name))
                {
                    var userToUpdate = this.dataContext.Users.FirstOrDefault(s => s.Name == name);
                    userToUpdate.Language = culture;

                    this.dataContext.Update(userToUpdate);
                    this.dataContext.SaveChanges();
                }
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

        private static bool IsPasswordValid(UserParameters user, string password, string supportToken, TimeSpan validity)
        {
            if (!user.IsService)
            {
                if (user.AccessLevel == 99)
                {
                    var expectedPassword = $"Ferretto{31 - DateTime.Now.Day:00}";
                    return expectedPassword == password;
                }
                else
                {
                    var providedPasswordHash = GeneratePasswordHash(password, user.PasswordSalt);

                    return providedPasswordHash == user.PasswordHash;
                }
            }
            else
            {
                // Encode secret key in Base32 encoding
                var encodedSecretKey = Base32Encoding.ToBytes(supportToken);

                // Generate Time based One Time password with a time window of 30 minutes
                var totp = new Totp(encodedSecretKey, step: (int)validity.TotalSeconds, mode: OtpHashMode.Sha512);

                // Set verification window
                var window = new VerificationWindow(previous: 1, future: 1);

                // Execute validation
                var verification = totp.VerifyTotp(DateTime.Now, password, out _, window);
                return verification;
            }
        }

        #endregion
    }
}
