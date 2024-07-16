

namespace FSMViewAvalonia2;

public class App : Application
{
    public static MainWindow mainWindow;
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
