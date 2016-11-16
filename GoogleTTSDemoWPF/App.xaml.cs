using System.Windows;

namespace GoogleTTS
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainViewModel = new MainViewModel();
            var mainView = new MainWindow(mainViewModel);
            mainView.Show();
        }
    }
}