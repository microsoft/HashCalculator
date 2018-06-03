using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TPMPCRCalculator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PermutateHash : Page
    {
        private ApplicationDataContainer m_RoamingSettings = null;

        private readonly NavigationHelper m_NavigationHelper;
        private int m_CurrentAlgorithmIndex = 0;
        private const string m_SettingSelectedHashAlgorithm = "permuterSelectedHash";
        private const string m_SettingExpectedHash = "permuterExpectedHash";

        private CancellationTokenSource cts;

        public PermutateHash()
        {
            this.InitializeComponent();

            this.m_RoamingSettings = ApplicationData.Current.RoamingSettings;

            this.m_NavigationHelper = new NavigationHelper(this);
            this.m_NavigationHelper.LoadState += LoadState;
            this.m_NavigationHelper.SaveState += SaveState;

            string[] algorithms = Worker.GetHashingAlgorithms();
            ListOfAlgorithms.Items.Clear();
            for (uint i = 0; i < algorithms.Length; i++)
            {
                ListOfAlgorithms.Items.Add(algorithms[i]);
            }
            ListOfAlgorithms.SelectedIndex = m_CurrentAlgorithmIndex;

            Hashes.Items.Clear();
            ResultHashes.Items.Clear();
        }

        private void MainPage_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    PermutateHashes_Click(sender, e);
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
                        m_CurrentAlgorithmIndex = index;
                    }
                }
            }

            if (SuspensionManager.SessionState.ContainsKey(m_SettingExpectedHash))
            {
                ExpectedResultHash.Text = (string)SuspensionManager.SessionState[m_SettingExpectedHash];
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
            SuspensionManager.SessionState[m_SettingExpectedHash] = ExpectedResultHash.Text;
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

        #region UI action handlers

        private void AddHash_Click(object sender, RoutedEventArgs e)
        {
            string hash = HashToAdd.Text.Trim();
            try
            {
                Worker.ValidateIsHash((string)ListOfAlgorithms.SelectedItem, hash);
                Hashes.Items.Add(hash);
                HashToAdd.Text = "";
            }
            catch (Exception ex)
            {
                ResultHashes.Items.Clear();
                ResultHashes.Items.Add(ex.Message);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Hashes.Items.Clear();
            ResultHashes.Items.Clear();
            ExpectedResultHash.Text = "";
            HashToAdd.Text = "";
        }

        private async void PermutateHashes_Click(object sender, RoutedEventArgs e)
        {
            bool found = false;
            try
            {
                cts = new CancellationTokenSource();

                ResultHashes.Items.Clear();
                found = await CheckAndUpdateAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                ResultHashes.Items.Add("Computation cancelled.");
            }
            catch (Exception)
            {
                ResultHashes.Items.Add("Computation failed.");
            }
            finally
            {
                // Set the CancellationTokenSource to null when the computation is complete.  
                cts = null;

                if (!found)
                    ResultHashes.Items.Add("No order of the provided hashes resulted in the expected hash value.");
            }
        }

        private void ListOfAlgorithms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfAlgorithms.SelectedIndex != m_CurrentAlgorithmIndex)
            {
                m_CurrentAlgorithmIndex = ListOfAlgorithms.SelectedIndex;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        #endregion

        private async Task<List<string>> CheckAndUpdateAsync(string algorithm, int initialValue, string[] hashes, int[] permutation, string expectedResult)
        {
            if (hashes.Length != permutation.Length)
                throw new ArgumentException("Hash array and permutation array have different size.");

            return await Task<List<string>>.Run(() =>
            {
                // resort hashes into order of permutation
                string[] permutatedHashes = new string[hashes.Length];
                for (int i = 0; i < hashes.Length; i++)
                {
                    permutatedHashes[i] = hashes[permutation[i]];
                }
                int index = Worker.CheckIfRightHashOrder(algorithm, initialValue, permutatedHashes, expectedResult);
                if (index >= 0)
                {
                    List<string> result = new List<string>(index + 2);
                    result.Add(Worker.GetZeroDigestForAlgorithm(algorithm, initialValue));
                    for (int i = 0; i <= index; i++)
                    {
                        result.Add((string)hashes[i]);
                    }
                    return result;
                }
                return new List<string>();
            });
        }

        private async Task<bool> CheckAndUpdateAsync(CancellationToken ct)
        {
            int[] singlePermutation = new int[Hashes.Items.Count];
            for (int i = 0; i < Hashes.Items.Count; i++)
                singlePermutation[i] = i;

            IEnumerable<IEnumerable<int>> permutations;
            if (DontChangeHashOrder.IsChecked.Value)
            {
                permutations = new List<IEnumerable<int>>(1);
                List<IEnumerable<int>> single = permutations as List<IEnumerable<int>>;
                single.Add(singlePermutation);
            }
            else
            {
                permutations = PermutationHelper.GetPermutations<int>(singlePermutation, singlePermutation.Length);
            }
            // split the permuations into chunks that we run sequenctially to avoid out of memory exception when trying to
            // run too many permutations in parallel
            var chunks = PermutationHelper.Split<IEnumerable<int>>(permutations, 1024);

            bool foundMatch = false;
            List<string> results = new List<string>();
            // check localities (0..4) and Locality Indicator (locality as bitfield) (1, 2, 4, 8, 16)
            int[] startValues = new int[] { 0, 1, 2, 3, 4, 8, 16 };
            foreach (int startValue in startValues)
            {
                foreach (var chunk in chunks)
                {
                    IEnumerable<Task<List<string>>> computePermutatioQuery =
                        from permutation in chunk select CheckAndUpdateAsync((string)ListOfAlgorithms.SelectedItem, startValue, Hashes.Items.Cast<string>().ToArray<string>(), permutation.ToArray<int>(), ExpectedResultHash.Text.Trim());

                    List<Task<List<string>>> computePermutation = computePermutatioQuery.ToList();

                    while (computePermutation.Count > 0)
                    {
                        Task<List<string>> finishedPermutation = await Task.WhenAny(computePermutation);
                        computePermutation.Remove(finishedPermutation);
                        results = await finishedPermutation;
                        foundMatch = results.Count > 0;
                        if (foundMatch || ct.IsCancellationRequested)
                        {
                            // cancel the rest
                            cts.Cancel();
                            break;
                        }
                    }
                    if (foundMatch || ct.IsCancellationRequested)
                        break;
                }
                if (foundMatch || ct.IsCancellationRequested)
                    break;
            }

            if (foundMatch)
            {
                foreach (string result in results)
                {
                    ResultHashes.Items.Add(result);
                }
            }
            return foundMatch;
        }
    }
}
