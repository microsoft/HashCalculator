using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TPMPCRCalculator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TpmPcrs : Page
    {
        private int m_CurrentAlgorithmIndex = 0;
        private readonly NavigationHelper m_NavigationHelper;
        private const string m_SettingSelectedHashAlgorithm = "pcrSelectedHash";
        private const string m_SettingInput = "pcrInput";
        private const string m_SettingPCR = "pcrPCR";
        private const string m_ExtendDescriptionTemplate =
            "The Extend button concatenates the current PCR value with the provided input data (hash) " +
            "and then computes a hash of the concatenated value. " +
            "This computed hash is then displayed as Current PCR Value.";

        public TpmPcrs()
        {
            this.InitializeComponent();
            this.m_NavigationHelper = new NavigationHelper(this);
            this.m_NavigationHelper.LoadState += LoadState;
            this.m_NavigationHelper.SaveState += SaveState;

            string[] algorithms = Worker.GetHashingAlgorithms(true);
            ListOfAlgorithms.Items.Clear();
            for (uint i = 0; i < algorithms.Length; i++)
            {
                ListOfAlgorithms.Items.Add(algorithms[i]);
            }
            ListOfAlgorithms.SelectedIndex = m_CurrentAlgorithmIndex;

            PCR.Text = Worker.GetZeroDigestForAlgorithm((string)ListOfAlgorithms.SelectedItem);
            ExtendDescription.Text = m_ExtendDescriptionTemplate;
        }

        private void ResetPcr_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ListOfAlgorithms.SelectedIndex != -1)
            {
                PCR.Text = Worker.GetZeroDigestForAlgorithm((string)ListOfAlgorithms.SelectedItem);
            }
            ExtendDescription.Text = m_ExtendDescriptionTemplate;
        }

        private void Extend_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            string algorithmName = (string)ListOfAlgorithms.SelectedItem;
            // if input cannot be hashed, don't change PCR values
            try
            {
                string inputHash = Worker.ComputeHash(algorithmName, Input.Text, true);
                if (inputHash != null &&
                    inputHash.Length > 0)
                {
                    string oldPCR = PCR.Text;
                    PCR.Text = Worker.ComputeHash(algorithmName, PCR.Text + Input.Text, true);
                    ExtendDescription.Text = m_ExtendDescriptionTemplate + "\n\n" +
                        "Old PCR Value: " + oldPCR + "\n" +
                        "Input Hash: " + Input.Text + "\n" +
                        "Concatenated Value: " + oldPCR + Input.Text + "\n" +
                        "New PCR Value: " + PCR.Text;
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    ExtendDescription.Text = ex.Message;
                }
            }
        }

        private void TpmPcr_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    Extend_Click(sender, e);
                    break;
            }
        }

        private void ListOfAlgorithms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfAlgorithms.SelectedIndex != m_CurrentAlgorithmIndex)
            {
                m_CurrentAlgorithmIndex = ListOfAlgorithms.SelectedIndex;
                PCR.Text = Worker.GetZeroDigestForAlgorithm((string)ListOfAlgorithms.SelectedItem);
                ExtendDescription.Text = m_ExtendDescriptionTemplate;
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
                        m_CurrentAlgorithmIndex = index;
                    }
                }
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingInput))
            {
                Input.Text = (string)SuspensionManager.SessionState[m_SettingInput];
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingPCR))
            {
                PCR.Text = (string)SuspensionManager.SessionState[m_SettingPCR];
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
            SuspensionManager.SessionState[m_SettingPCR] = PCR.Text;
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
