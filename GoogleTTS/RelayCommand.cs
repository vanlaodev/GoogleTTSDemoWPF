using System;
using System.Windows.Input;

namespace GoogleTTS
{
    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _canExec;
        private readonly Action<object> _exec;

        public RelayCommand(Action<object> exec, Func<object, bool> canExec)
        {
            _exec = exec;
            _canExec = canExec;
        }

        public bool CanExecute(object parameter)
        {
            return _canExec != null && _canExec(parameter);
        }

        public void Execute(object parameter)
        {
            if (_exec != null)
            {
                _exec(parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}