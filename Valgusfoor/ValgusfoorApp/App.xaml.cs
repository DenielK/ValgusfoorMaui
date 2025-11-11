namespace ValgusfoorApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Открываем нашу страницу ValgusfoorPage вместо AppShell
            return new Window(new ValgusfoorPage());
        }
    }
}
