using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoClickKey.Models;

public class Job : INotifyPropertyChanged
{
    private string _name = "New Job";
    private bool _isEnabled = true;
    private int _delayBetweenProfiles;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    public List<string> ProfileNames { get; set; } = new();

    public int DelayBetweenProfiles
    {
        get => _delayBetweenProfiles;
        set
        {
            if (_delayBetweenProfiles != value)
            {
                _delayBetweenProfiles = Math.Max(0, value);
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
