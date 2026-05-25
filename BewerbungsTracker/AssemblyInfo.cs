using System.Windows;

/// <summary>
/// AssemblyInfo konfiguriert globale Einstellungen für WPF-Theme-Ressourcen.
/// Dies ist standardmäßige Konfiguration für WPF-Anwendungen.
/// </summary>

/// Konfiguriert, wo WPF nach Theme-spezifischen Ressourcen-Dictionaries sucht.
/// - ResourceDictionaryLocation.None: Keine theme-spezifischen Ressourcen definiert
/// - ResourceDictionaryLocation.SourceAssembly: Generische Ressourcen sind im Assembly selbst definiert
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // Wo theme-spezifische Resource Dictionaries sind
                                                // (wird verwendet, wenn eine Ressource auf der Seite,
                                                // oder in den Anwendungs-Ressourcen nicht gefunden wird)
    ResourceDictionaryLocation.SourceAssembly   // Wo das generische Resource Dictionary ist
                                                // (wird verwendet, wenn eine Ressource auf der Seite,
                                                // in der App, oder in theme-spezifischen Ressourcen nicht gefunden wird)
)]
