using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using System.Threading.Tasks;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Kindle2OneNote
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var systemColor = (Windows.UI.Color)this.Resources["SystemAccentColor"];
            var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar != null)
            {
                titleBar.ButtonBackgroundColor = systemColor;
                titleBar.BackgroundColor = systemColor;
                titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonHoverBackgroundColor = LightenDarkenColor(systemColor, 0.1);
                titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
                titleBar.ButtonPressedBackgroundColor = LightenDarkenColor(systemColor, -0.1);
            }

            Size windowSize = new Size(480, 260);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = windowSize;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(windowSize);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;

            this.DataContext = Presenter.Instance;
        }

        private void selectFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Presenter.Instance.IsReady)
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        
        private static Windows.UI.Color LightenDarkenColor(Windows.UI.Color color, double correctionFactor)
        {
            double red = (255 - color.R) * correctionFactor + color.R;
            double green = (255 - color.G) * correctionFactor + color.G;
            double blue = (255 - color.B) * correctionFactor + color.B;
            return Windows.UI.Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }
    }

    class UserStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if ((bool)value)
            {
                return loader.GetString("SignedIn");
            }
            else
            {
                return loader.GetString("NotSet"); ;
            }
        }
        
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class ButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, string language)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if ((bool)value)
            {
                return loader.GetString("SignOut");
            }
            else
            {
                return loader.GetString("SignIn");
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
