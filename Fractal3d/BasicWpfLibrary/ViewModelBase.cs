namespace BasicWpfLibrary;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

/*
 public string FirstName
 {
     get { return _firstName; }
     set { SetProperty(ref _firstName, value); }
 }
 */

// derived from https://stackoverflow.com/questions/36149863/how-to-write-a-viewmodelbase-in-mvvm
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

