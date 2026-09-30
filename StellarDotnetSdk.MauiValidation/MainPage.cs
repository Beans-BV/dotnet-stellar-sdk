namespace StellarDotnetSdk.MauiValidation;

/// <summary>
///     Runs the validation checks once on startup and again on demand. Results go to the screen and to the
///     platform log (Android logcat; on iOS expected in the device console, not verified), prefixed so they can be
///     collected from the command line.
/// </summary>
public class MainPage : ContentPage
{
    private readonly Label _output = new() { FontFamily = "monospace", FontSize = 12 };
    private readonly Button _runButton = new() { Text = "Run checks" };
    private bool _hasStarted;

    public MainPage()
    {
        Title = "Stellar SDK MAUI validation";
        _runButton.Clicked += async (_, _) => await RunAsync();
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 12,
                Children = { _runButton, _output },
            },
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_hasStarted)
        {
            return;
        }
        _hasStarted = true;
        await RunAsync();
    }

    private async Task RunAsync()
    {
        _runButton.IsEnabled = false;
        _output.Text = "";
        try
        {
            await new ValidationRunner(
                line => MainThread.BeginInvokeOnMainThread(() => _output.Text += line + Environment.NewLine),
                () => MainThread.IsMainThread).RunAllAsync();
        }
        finally
        {
            _runButton.IsEnabled = true;
        }
    }
}
