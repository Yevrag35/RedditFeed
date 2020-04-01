using System;
using System.Windows;

namespace RedditFeed
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            var mw = new MainWindow();
            mw.Show();
        }
    }
}
