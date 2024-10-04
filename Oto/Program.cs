
namespace Oto
{
    public class Program
    {
        private readonly Adw.Application _application;
        private MainWindow? _window;

        public static int Main(string[] args) => new Program().Run(args);

        public Program()
        {
            _application = Adw.Application.New("io.github.ravener.Oto", Gio.ApplicationFlags.FlagsNone);
            _application.OnActivate += OnActivate;
            _application.OnStartup += OnStartup;
        }

        public void OnActivate(Gio.Application app, EventArgs args)
        {
            _window ??= new MainWindow(_application);
            _window.Present();
        }

        private void OnStartup(Gio.Application application, EventArgs args)
        {
            CreateAction("quit", (_, _) => { _application.Quit(); }, ["<Ctrl>Q"]);
            CreateAction("about", (_, _) => { OnAbout(); });
        }

        public int Run(string[] args)
        {
            return _application.RunWithSynchronizationContext(args);
        }

        public void OnAbout()
        {
            var about = Adw.AboutWindow.New();

            about.TransientFor = _window;
            about.ApplicationName = "Oto";
            about.ApplicationIcon = "io.github.ravener.Oto";
            about.DeveloperName = "Ravener";
            about.Version = "1.0.0";
            about.Developers = ["Ravener"];
            about.Copyright = "© 2024 Ravener";
            about.IssueUrl = "https://github.com/ravener/Oto/issues";
            about.Website = "https://github.com/ravener/Oto";
            about.LicenseType = Gtk.License.Gpl30;

            about.Present();
        }

        private void CreateAction(string name, GObject.SignalHandler<Gio.SimpleAction, Gio.SimpleAction.ActivateSignalArgs> callback, string[]? shortcuts = null)
        {
            var action = Gio.SimpleAction.New(name, null);
            action.OnActivate += callback;
            _application.AddAction(action);

            if (shortcuts?.Length > 0)
            {
                _application.SetAccelsForAction($"app.{name}", shortcuts);
            }
        }
    }
}