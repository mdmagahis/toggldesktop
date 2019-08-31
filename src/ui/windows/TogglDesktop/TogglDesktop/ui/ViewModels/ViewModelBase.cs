using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TogglDesktop.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool RaiseAndSetIfChanged<TRet>(ref TRet backingField, TRet newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
            {
                return false;
            }

            backingField = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}