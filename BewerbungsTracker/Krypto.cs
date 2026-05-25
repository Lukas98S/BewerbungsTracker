using System;
using System.Security.Cryptography;
using System.Text;

namespace BewerbungsTracker
{
    public static class Krypto
    {
        public static string HashEn(string enInput)
        {
            if (string.IsNullOrEmpty(enInput)) return enInput;

            byte[] bytes = Encoding.UTF8.GetBytes(enInput);
            byte[] enResult = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(enResult);
        }

        public static string HashDe(string deInput)
        {
            if (string.IsNullOrEmpty(deInput)) return deInput;

            try
            {
                byte[] bytes = Convert.FromBase64String(deInput);
                byte[] deResult = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(deResult);
            }
            catch
            {
                return "Fehler beim Entschlüsseln";
            }
               
        }
    }
}
