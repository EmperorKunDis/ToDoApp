using System;
using System.ComponentModel;
using System.Windows.Media;

public class TaskItem : INotifyPropertyChanged
{
    private bool _isCompleted;
    private DateTime _deadline;
    private string _description;
    private Brush _foregroundColor;
    private string _remainingTime;

    public event EventHandler TaskDeleted;

    public void DeleteTask()
    {
        TaskDeleted?.Invoke(this, EventArgs.Empty);
    }

    public string Description
    {
        get { return _description; }
        set
        {
            _description = value;
            OnPropertyChanged("Description");
        }
    }

    public DateTime Deadline
    {
        get { return _deadline; }
        set
        {
            _deadline = value;
            OnPropertyChanged("Deadline");
            OnPropertyChanged("RemainingTime");
            UpdateForegroundColor();
        }
    }

    public bool IsCompleted
    {
        get { return _isCompleted; }
        set
        {
            _isCompleted = value;
            OnPropertyChanged("IsCompleted");
            OnPropertyChanged("RemainingTime");
            UpdateForegroundColor();

            if (_isCompleted)
                TaskCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    public string RemainingTime
    {
        get
        {
            if (IsCompleted)
                return "Completed";

            var remainingTime = Deadline - DateTime.Now;
            if (remainingTime.TotalSeconds <= 0)
            {
                ForegroundColor = Brushes.Red;
                if (!IsCompleted)
                {
                    IsCompleted = true;
                    _remainingTime = "You Fucked It Up";
                    return _remainingTime;
                }
            }
            return _remainingTime ?? $"{(int)remainingTime.TotalDays}d {(int)remainingTime.TotalHours % 24}h {(int)remainingTime.TotalMinutes % 60}m";
        }
    }


    public Brush ForegroundColor
    {
        get { return _foregroundColor; }
        set
        {
            _foregroundColor = value;
            OnPropertyChanged("ForegroundColor");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler TaskCompleted;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateForegroundColor()
    {
        var remainingTime = Deadline - DateTime.Now;
        if (remainingTime.TotalSeconds <= 1000 && !IsCompleted)
        {
            ForegroundColor = Brushes.Red;
        }
        else
        {
            ForegroundColor = Brushes.Black;
        }
    }
}
