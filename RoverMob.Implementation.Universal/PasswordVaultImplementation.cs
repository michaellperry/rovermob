using System;
using Windows.Security.Credentials;

namespace RoverMob.Implementation
{
    public static class PasswordVaultImplementation
    {
        public static void RemoveCredential(string resource, string userName)
        {
            PasswordVault vault = new PasswordVault();
            try
            {
                var password = vault.Retrieve(resource, userName);
                vault.Remove(password);
            }
            catch (Exception)
            {
            }
        }

        public static string RetrieveCredential(string resource, string userName)
        {
            PasswordVault vault = new PasswordVault();
            try
            {
                var password = vault.Retrieve(resource, "User").Password;
                return password;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void StoreCredential(string resource, string userName, string password)
        {
            PasswordVault vault = new PasswordVault();
            vault.Add(new PasswordCredential(resource, userName, password));
        }
    }
}
