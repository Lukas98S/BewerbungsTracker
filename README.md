# Bewerbungs-Tracker

Ein modernes, in C# und WPF geschriebenes Tool zur effizienten Verwaltung von Job-Bewerbungen.
Das Highlight dieses Projekts: Die Anwendung prüft im Hintergrund automatisch das E-Mail-Postfach nach eingehenden Abbsagen und aktualisiert den Status der jeweiligen Bewerbung selbstständig in der Datenbank.

## Porgramm-Screenshots
<img width="786" height="443" alt="BewerbungsTracker" src="https://github.com/user-attachments/assets/5d796ddd-cbde-4ba7-837f-7131a4a47178" />
<img width="786" height="443" alt="BewerbungsTracker_2" src="https://github.com/user-attachments/assets/3f558948-ca87-42b7-b171-985ceefe0b6c" />

## Features
* **Automatischer E-Mail-Scan (IMAP):** Verbindet sich sicher mit dem E-Mail-Postfach (automatische Erkennung gängiger Anbieter wie GMX, GMail etc.) und gleicht eingehende E-Mails mit offenen Bewerbungen ab.
* **Smartes Auto-Save** Änderungen am Bewerbungsstatus (z.B. über das Dropdown-Menü) werden in Echtzeit in der Datenbank gespeichert - ganz ohne störende Speicher-Buttons.
* **PDF-Automatisierung:** Liest Positions- und Firmendaten automatisch aus PDF-Anschreiben aus und legt neue Datensätze an.
* **Hohe Sicherheit:** E-Mail-Passwörter werden nicht im Klartext gespeichert. Die App nutzt die **Windows DPAPI** (Data Protection API), um sensible Daten sicher und an das Windows-Benutzerkonto gebunden zu verschlüsseln.
* **Archiv-System:** Abgelehnte Bewerbungen verschwinden nahtlos aus der Hauptansicht und werden übersichtlich im Archiv abgelegt.

## Verwendete Technologien und Architektur

* **Sprache:** C# (.NET Core)
* **UI-Framework:** WPF (Windows Presentation Foundation)
* **Architektur:** Strickte Trennung von Logik und Disgn nach dem **MVVM-Pazzern** (Model-View-ViewModel).
* **Datenbbank:** SQLite in Kombination mit dem **Entity Framework Core**.
* **Wichtige NuGet-Pakete:**
  * `MailKit` (für die IMAP-Verbindung)
  * `UglyToad.PdfPig` (für das Parsen der PDF-Dokumente)
  * `System.Secruity.Cryptography.ProtectedData` (für die DPAPI-Verschlüsselung)
 
## Installation & Start

1. Lade dir das Repository herunter oder klone es via Git.
2. Öffne die Projektmappe (`.sln`) in Visual Studio.
3. Stelle sicher, dass alle NuGet-Pakete wiederhergestellt wurden.
4. Starte das Projekt. Beim allerersten Start führt dich das integrierte Setup-Fenter durch die Einrichtung deiner E-Mail-Daten.

## Hinweis zur Sicherheit
Für den automatischen E-Mail-Scan wird die Nutzung eines **App-Passwortes** (über die Kontoeinstellungen deines E-Mail-Anbieters) benötigt.
Die Zugangsdaten werden rein lokal auf deinem PC verschlüsselt in einer `secrets.json` gespeichert und verlassen niemals dein System.

##

# Application tracker

A modern tool written in C# and WPF for efficient management of job applications.
The highlight of this project: The application automatically checks the email inbox in the background for incoming rejections and automatically updates the status of the respective application in the database.

## Program screenshots
<img width="786" height="443" alt="ApplicationTracker" src="https://github.com/user-attachments/assets/5d796ddd-cbde-4ba7-837f-7131a4a47178" />
<img width="786" height="443" alt="BewerbungsTracker_2" src="https://github.com/user-attachments/assets/3f558948-ca87-42b7-b171-985ceefe0b6c" />

## Features
* **Automatic email scan (IMAP):** Connects securely to the email inbox (automatically detects common providers such as GMX, GMail, etc.) and compares incoming emails with open applications.
* **Smart Auto-Save** Changes to the application status (e.g. via the drop-down menu) are saved in the database in real time - without any annoying save buttons.
* **PDF automation:** Automatically reads position and company data from PDF cover letters and creates new data records.
* **High Security:** Email passwords are not stored in plain text. The app uses the **Windows DPAPI** (Data Protection API) to encrypt sensitive data securely and tied to the Windows user account.
* **Archive system:** Rejected applications disappear seamlessly from the main view and are clearly stored in the archive.

## Technologies and architecture used

* **Language:** C# (.NET Core)
* **UI framework:** WPF (Windows Presentation Foundation)
* **Architecture:** Strict separation of logic and disgn according to the **MVVM pazzing** (Model-View-ViewModel).
* **Database:** SQLite in combination with the **Entity Framework Core**.
* **Important NuGet packages:**
* `MailKit` (for IMAP connection)
* `UglyToad.PdfPig` (for parsing the PDF documents)
* `System.Secruity.Cryptography.ProtectedData` (for DPAPI encryption)

## Installation & Start

1. Download the repository or clone it via Git.
2. Open the solution (`.sln`) in Visual Studio.
3. Make sure all NuGet packages have been saved as is.
4. Start the project. When you first start it, the integrated setup window will guide you through setting up your email data.

## Note on security
Automatic email scanning requires the use of an **app password** (via your email provider's account settings).
The access data is stored encrypted locally on your PC in a “secrets.json” and never leaves your system.
