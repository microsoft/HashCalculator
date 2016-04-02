using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HashCalculator.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TpmPcrs : Page
    {
        public TpmPcrs()
        {
            this.InitializeComponent();

            string[] algorithms = Worker.GetHashingAlgorithms(true);
            ListOfAlgorithms.Items.Clear();
            for (uint i = 0; i < algorithms.Length; i++)
            {
                ListOfAlgorithms.Items.Add(algorithms[i]);
            }
            ListOfAlgorithms.SelectedIndex = 0;
        }

        private void ResetPcr_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ListOfAlgorithms.SelectedIndex != -1)
            {
                PCR.Text = Worker.GetZeroDigestForAlgorithm((string)ListOfAlgorithms.SelectedItem);
            }
        }
    }
}
