using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TPMPCRCalculator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {
        ApplicationDataContainer roamingSettings = null;
        const string DontShowLandingPageSetting = "DontShowLandingPage";

        public LandingPage()
        {
            this.InitializeComponent();

            roamingSettings = ApplicationData.Current.RoamingSettings;
        }

        private void DontShowLandingPage_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            roamingSettings.Values[DontShowLandingPageSetting] = DontShowLandingPage.IsChecked.ToString();
        }
    }
}
