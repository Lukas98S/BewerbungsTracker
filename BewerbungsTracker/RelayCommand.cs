using System;

using System.Windows.Input;

namespace BewerbungsTracker
{
    /// <summary>
    /// Implementierung des RelayCommand-Patterns für WPF Command Binding.
    /// Ermöglicht die Verwendung von Aktionen (Actions) als Commands in der UI.
    /// Dies ist besonders nützlich für die Kommunikation zwischen View und ViewModel im MVVM-Pattern.
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Die Aktion, die ausgeführt wird, wenn das Command aufgerufen wird.
        /// </summary>
        private readonly Action _execute;

        /// <summary>
        /// Initialisiert ein neues RelayCommand mit der auszuführenden Aktion.
        /// </summary>
        /// <param name="execute">Die Action, die ausgeführt werden soll</param>
        public RelayCommand(Action execute) => _execute = execute;

        /// <summary>
        /// Bestimmt, ob das Command ausgeführt werden kann.
        /// In dieser Implementierung ist das Command immer ausführbar.
        /// </summary>
        /// <param name="parameter">Command-Parameter (wird nicht verwendet)</param>
        /// <returns>Gibt immer true zurück</returns>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// Führt die dem Command zugeordnete Aktion aus.
        /// </summary>
        /// <param name="parameter">Command-Parameter (wird nicht verwendet)</param>
        public void Execute(object? parameter) => _execute();

        /// <summary>
        /// Event, das ausgelöst wird, wenn sich der Status CanExecute ändern könnte.
        /// In dieser Implementierung wird das Event nicht verwendet, da CanExecute immer true ist.
        /// </summary>
        public event EventHandler? CanExecuteChanged;
    }


}

