using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HashCalculator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly NavigationHelper m_NavigationHelper;
        private const string m_SettingSelectedHashAlgorithm = "hashSelectedHash";
        private const string m_SettingInput = "hashInput";
        private const string m_SettingOutput = "hashOutput";
        private const string m_SettingRawBytes = "hashRawBytes";

        public MainPage()
        {
            this.InitializeComponent();
            this.m_NavigationHelper = new NavigationHelper(this);
            this.m_NavigationHelper.LoadState += LoadState;
            this.m_NavigationHelper.SaveState += SaveState;

            string[] algorithms = Worker.GetHashingAlgorithms();
            ListOfAlgorithms.Items.Clear();
            for (uint i = 0; i < algorithms.Length; i++)
            {
                ListOfAlgorithms.Items.Add(algorithms[i]);
            }
            ListOfAlgorithms.SelectedIndex = 0;
        }

        private void GenerateHash_Click(object sender, RoutedEventArgs e)
        {
            string algorithmName = (string)ListOfAlgorithms.SelectedItem;
            Output.Text = Worker.ComputeHash(algorithmName, Input.Text, RawBytes.IsChecked.Value);
        }

        private void MakeInput_Click(object sender, RoutedEventArgs e)
        {
            Input.Text = Output.Text;
            Output.Text = "";
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
            if (SuspensionManager.SessionState.ContainsKey(m_SettingSelectedHashAlgorithm))
            {
                int index;
                if (Int32.TryParse((string)SuspensionManager.SessionState[m_SettingSelectedHashAlgorithm], out index))
                {
                    if (index >= 0 && index < ListOfAlgorithms.Items.Count)
                    {
                        ListOfAlgorithms.SelectedIndex = index;
                    }
                }
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingInput))
            {
                Input.Text = (string)SuspensionManager.SessionState[m_SettingInput];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingOutput))
            {
                Output.Text = (string)SuspensionManager.SessionState[m_SettingOutput];
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
            SuspensionManager.SessionState[m_SettingSelectedHashAlgorithm] = ListOfAlgorithms.SelectedIndex.ToString();
            SuspensionManager.SessionState[m_SettingInput] = Input.Text;
            SuspensionManager.SessionState[m_SettingOutput] = Output.Text;
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
