using System;
using System.Windows;
using System.Windows.Threading;

namespace AutoClickKey.Views;

public partial class ShutdownCountdownWindow : Window
{
    private readonly DispatcherTimer _countdownTimer;
    private int _remainingSeconds;

    public ShutdownCountdownWindow(int countdownSeconds = 60)
    {
        InitializeComponent();
        _remainingSeconds = countdownSeconds;
        CountdownText.Text = _remainingSeconds.ToString();

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += CountdownTimer_Tick;
    }

    public bool WasCancelled { get; private set; }

    public void StartCountdown()
    {
        _countdownTimer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        _countdownTimer.Stop();
        base.OnClosed(e);
    }

    private void CountdownTimer_Tick(object? sender, EventArgs e)
    {
        _remainingSeconds--;
        CountdownText.Text = _remainingSeconds.ToString();

        if (_remainingSeconds <= 0)
        {
            _countdownTimer.Stop();
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        WasCancelled = true;
        _countdownTimer.Stop();
        DialogResult = false;
        Close();
    }
}
