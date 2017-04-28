using System;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TPMPCRCalculator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalculateHMACHash : Page
    {
        private readonly NavigationHelper m_NavigationHelper;
        private const string m_SettingSelectedHashAlgorithm = "hmacHashSelectedHash";
        private const string m_SettingInput = "hmacHashInput";
        private const string m_SettingKey = "hmacHashKey";
        private const string m_SettingSha1 = "hmacHashSha1";
        private const string m_SettingSha256 = "hmacHashSha256";
        private const string m_SettingSha384 = "hmacHashSha384";
        private const string m_SettingSha512 = "hmacHashSha512";
        private const string m_SettingRawBytes = "hmacHashRawBytes";

        public CalculateHMACHash()
        {
            this.InitializeComponent();
            this.m_NavigationHelper = new NavigationHelper(this);
            this.m_NavigationHelper.LoadState += LoadState;
            this.m_NavigationHelper.SaveState += SaveState;
        }

        private void GenerateHash_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Sha1.Text = Worker.ComputeHmacHash(MacAlgorithmNames.HmacSha1, Input.Text, Key.Text, RawBytes.IsChecked.Value);
                Sha256.Text = Worker.ComputeHmacHash(MacAlgorithmNames.HmacSha256, Input.Text, Key.Text, RawBytes.IsChecked.Value);
                Sha384.Text = Worker.ComputeHmacHash(MacAlgorithmNames.HmacSha384, Input.Text, Key.Text, RawBytes.IsChecked.Value);
                Sha512.Text = Worker.ComputeHmacHash(MacAlgorithmNames.HmacSha512, Input.Text, Key.Text, RawBytes.IsChecked.Value);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                    Sha1.Text = ex.Message;
            }
        }

        private void MainPage_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    GenerateHash_Click(sender, e);
                    break;
            }
        }

        #region Save and Restore state

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void LoadState(object sender, LoadStateEventArgs e)
        {
            if (SuspensionManager.SessionState.ContainsKey(m_SettingInput))
            {
                Input.Text = (string)SuspensionManager.SessionState[m_SettingInput];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingKey))
            {
                Key.Text = (string)SuspensionManager.SessionState[m_SettingKey];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingSha1))
            {
                Sha1.Text = (string)SuspensionManager.SessionState[m_SettingSha1];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingSha256))
            {
                Sha256.Text = (string)SuspensionManager.SessionState[m_SettingSha256];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingSha384))
            {
                Sha384.Text = (string)SuspensionManager.SessionState[m_SettingSha384];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingSha512))
            {
                Sha512.Text = (string)SuspensionManager.SessionState[m_SettingSha512];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingRawBytes))
            {
                bool rawBytes;
                if (Boolean.TryParse((string)SuspensionManager.SessionState[m_SettingRawBytes], out rawBytes))
                {
                    RawBytes.IsChecked = rawBytes;
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void SaveState(object sender, SaveStateEventArgs e)
        {
            SuspensionManager.SessionState[m_SettingInput] = Input.Text;
            SuspensionManager.SessionState[m_SettingKey] = Key.Text;
            SuspensionManager.SessionState[m_SettingSha1] = Sha1.Text;
            SuspensionManager.SessionState[m_SettingSha256] = Sha256.Text;
            SuspensionManager.SessionState[m_SettingSha384] = Sha384.Text;
            SuspensionManager.SessionState[m_SettingSha512] = Sha512.Text;
            SuspensionManager.SessionState[m_SettingRawBytes] = RawBytes.IsChecked.ToString();
        }

        #endregion

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        /// 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.m_NavigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.m_NavigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
