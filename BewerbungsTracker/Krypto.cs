using System;
using System.Security.Cryptography;
using System.Text;

namespace BewerbungsTracker
{
    /// <summary>
    /// Statische Hilfsklasse für Verschlüsselung und Entschlüsselung von sensiblen Daten.
    /// Nutzt die DPAPI (Data Protection API) von Windows für sichere Datenverschlüsselung
    /// auf Basis des aktuellen Benutzers.
    /// </summary>
    public static class Krypto
    {
        /// <summary>
        /// Verschlüsselt einen String mit DPAPI unter dem aktuellen Benutzer.
        /// Die verschlüsselten Daten können nur vom gleichen Benutzer auf dem gleichen Computer entschlüsselt werden.
        /// </summary>
        /// <param name="enInput">Der zu verschlüsselnde String</param>
        /// <returns>Base64-kodierter, verschlüsselter String. Gibt den Input zurück, falls dieser leer/null ist.</returns>
        public static string HashEn(string enInput)
        {
            // Wenn der Input leer oder null ist, wird er unverändert zurückgegeben
            if (string.IsNullOrEmpty(enInput)) return enInput;

            // Der String wird zu UTF8-Bytes konvertiert für die Verschlüsselung
            byte[] bytes = Encoding.UTF8.GetBytes(enInput);

            // DPAPI verschlüsselt die Bytes mit dem aktuellen Benutzer als Schlüssel
            byte[] enResult = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);

            // Das Ergebnis wird zu Base64 konvertiert für einfache Speicherung/Übertragung
            return Convert.ToBase64String(enResult);
        }

        /// <summary>
        /// Entschlüsselt einen mit HashEn verschlüsselten String.
        /// </summary>
        /// <param name="deInput">Der zu entschlüsselnde Base64-kodierte String</param>
        /// <returns>Der Originaltext oder eine Fehlermeldung, falls Entschlüsselung fehlschlägt.</returns>
        public static string HashDe(string deInput)
        {
            // Wenn der Input leer oder null ist, wird er unverändert zurückgegeben
            if (string.IsNullOrEmpty(deInput)) return deInput;

            try
            {
                // Der Base64-String wird zurück zu Bytes konvertiert
                byte[] bytes = Convert.FromBase64String(deInput);

                // DPAPI entschlüsselt die Bytes mit dem aktuellen Benutzer
                byte[] deResult = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);

                // Die entschlüsselten Bytes werden zu UTF8-String konvertiert
                return Encoding.UTF8.GetString(deResult);
            }
            catch
            {
                // Falls die Entschlüsselung fehlschlägt (z.B. wegen ungültigen Base64-Strings
                // oder wenn der Benutzer gewechselt hat), wird eine Fehlermeldung zurückgegeben
                return "Fehler beim Entschlüsseln";
            }

        }
    }
}
