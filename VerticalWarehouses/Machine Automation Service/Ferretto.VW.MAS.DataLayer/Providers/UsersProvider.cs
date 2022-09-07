using System;
using System.Collections.Generic;
using System.IO;
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

            this.AreSetUsers();

            //var salt = Convert.ToBase64String(GeneratePasswordSalt());
            //var hash = Encrypt("password", salt);
            //var psw = Decrypt(hash, salt);
        }

        #endregion

        #region Methods

        public static string Decrypt(string cipherText, string salt)
        {
            var iv = new byte[16];
            Buffer.BlockCopy(Convert.FromBase64String(cipherText), 0, iv, 0, iv.Length);
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                using (var decryptor = aesAlg.CreateDecryptor(Convert.FromBase64String(salt), iv))
                {
                    byte[] encrypted = Convert.FromBase64String(cipherText);
                    using (var msDecrypt = new MemoryStream(encrypted, iv.Length, encrypted.Length - iv.Length))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var resultStream = new MemoryStream())
                            {
                                csDecrypt.CopyTo(resultStream);
                                return System.Text.Encoding.UTF8.GetString(resultStream.ToArray());
                            }
                        }
                    }
                }
            }
        }

        public static string Encrypt(string clearText, string salt)
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                using (var encryptor = aesAlg.CreateEncryptor(Convert.FromBase64String(salt), aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            byte[] data = System.Text.Encoding.UTF8.GetBytes(clearText);
                            csEncrypt.Write(data, 0, data.Length);
                        }
                        var ret = msEncrypt.ToArray();
                        return Convert.ToBase64String(ret);
                    }
                }
            }
        }

        public void AreSetUsers()
        {
            lock (this.dataContext)
            {
                if (!this.dataContext.Users.Any())
                {
                    this.dataContext.Users.Add(UserParameters.Values.Operator);
                    this.dataContext.Users.Add(UserParameters.Values.Movement);
                    this.dataContext.Users.Add(UserParameters.Values.Guest);
                    this.dataContext.Users.Add(UserParameters.Values.Installer);
                    this.dataContext.Users.Add(UserParameters.Values.Service);
                    this.dataContext.Users.Add(UserParameters.Values.Admin);
                }
                else
                {
                    if (!this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Admin.ToString().ToLower()))
                    {
                        this.dataContext.Users.Add(UserParameters.Values.Admin);
                    }

                    if (!this.dataContext.Users.Any(s => s.Name == "service"))
                    {
                        this.dataContext.Users.Add(UserParameters.Values.Service);
                    }

                    if (!this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Installer.ToString().ToLower()))
                    {
                        this.dataContext.Users.Add(UserParameters.Values.Installer);
                    }

                    if (!this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Movement.ToString().ToLower()))
                    {
                        this.dataContext.Users.Add(UserParameters.Values.Movement);
                    }

                    if (!this.dataContext.Users.Any(s => s.Name == UserAccessLevel.Operator.ToString().ToLower()))
                    {
                        this.dataContext.Users.Add(UserParameters.Values.Operator);
                    }

                    if (!this.dataContext.Users.Any(s => s.Name == UserParameters.Values.Guest.Name))
                    {
                        this.dataContext.Users.Add(UserParameters.Values.Guest);
                    }
                }
                foreach (var item in this.dataContext.Users)
                {
                    switch (item.Name)
                    {
                        case "admin":
                            item.AccessLevel = ((int)UserAccessLevel.Admin);
                            break;

                        case "service":
                            item.AccessLevel = ((int)UserAccessLevel.Admin);
                            break;

                        case "installer":
                            item.AccessLevel = ((int)UserAccessLevel.Installer);
                            break;

                        case "movement":
                            item.AccessLevel = ((int)UserAccessLevel.Movement);
                            break;

                        case "operator":
                        case "guest":
                            item.AccessLevel = ((int)UserAccessLevel.Operator);
                            break;

                        default:
                            break;
                    }
                }

                this.dataContext.SaveChanges();
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

        public UserParameters Authenticate(string cardToken)
        {
            if (string.IsNullOrWhiteSpace(cardToken))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(cardToken));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Token == cardToken);

                if (user != null
                    &&
                    (UserAccessLevel)user.AccessLevel != UserAccessLevel.NoAccess)
                {
                    return user;
                }
            }

            return null;
        }

        public void ChangePassword(string userName, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(newPassword));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                if (user != null)
                {
                    var newPasswordHash = GeneratePasswordHash(newPassword, user.PasswordSalt);
                    user.PasswordHash = newPasswordHash;

                    this.dataContext.Update(user);
                    this.dataContext.SaveChanges();
                    return;
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
                var result = this.dataContext.Users.AsNoTracking().Where(u => !u.IsDisabled && !u.IsDisabledWithWMS && string.IsNullOrEmpty(u.Token))
                    .OrderBy(o => o.AccessLevel).ThenByDescending(o => o.Id);
                return result;
            }
        }

        public bool GetIsDisabled(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                if (user != null)
                {
                    return user.IsDisabled;
                }
            }

            throw new EntityNotFoundException(userName);
        }

        public bool GetIsLimited(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                return user?.IsLimited is true;
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

        public bool IsOperatorEnabledWithWMS()
        {
            lock (this.dataContext)
            {
                if(!this.dataContext.WmsSettings.First().IsEnabled)
                {
                    return true;
                }
                var user = this.dataContext.Users.FirstOrDefault(u => u.AccessLevel == (int)UserAccessLevel.Operator && u.IsDisabledWithWMS);

                return (user is null);
            }
        }

        public void SetIsDisabled(string userName, bool isDisabled)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                if (user != null)
                {
                    user.IsDisabled = isDisabled;
                    this.dataContext.SaveChanges();
                    return;
                }
            }

            throw new EntityNotFoundException(userName);
        }

        public void SetIsLimited(string userName, bool isLimited)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException(Resources.General.ResourceManager.GetString("ValueCannotBeNullOrWhiteSpace", CommonUtils.Culture.Actual), nameof(userName));
            }

            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == userName);

                if (user != null)
                {
                    user.IsLimited = isLimited;
                    this.dataContext.SaveChanges();
                    return;
                }
            }

            throw new EntityNotFoundException(userName);
        }

        public void SetOperatorEnabledWithWMS(bool isEnabled)
        {
            lock (this.dataContext)
            {
                var user = this.dataContext.Users.SingleOrDefault(u => u.Name == UserParameters.Values.Operator.Name);

                if (user != null)
                {
                    user.IsDisabledWithWMS = !isEnabled;

                    this.dataContext.Update(user);
                    this.dataContext.SaveChanges();
                    return;
                }
            }
            throw new EntityNotFoundException(UserParameters.Values.Operator.Name);
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

        public void AddUser(UserParameters user)
        {
            lock (this.dataContext)
            {
                if (!this.dataContext.Users.Any(s => s.Name == user.Name))
                {
                    this.dataContext.Users.Add(user);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void DeleteUser(UserParameters user)
        {
            lock (this.dataContext)
            {
                if (this.dataContext.Users.Any(s => s.Name == user.Name))
                {
                    this.dataContext.Users.Remove(user);
                    this.dataContext.SaveChanges();
                }
            }
        }
        #endregion
    }
}
