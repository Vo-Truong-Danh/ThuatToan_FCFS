using FCFS.Library;
using System.Windows.Input;

namespace FCFS.UI.Views
{
    /// <summary>
    /// Interaction logic for EditBurst.xaml
    /// </summary>
    public partial class EditBurst : System.Windows.Window
    {
        public EditBurst()
        {
            InitializeComponent();
            MouseDown += (sender, e) => FocusManager.SetFocusedElement(this, this);
            uint count = (uint)Math.Round((double)(Stores.DataLake.Count / 2)) + 1;
            Stores.DataLake.CollectionChanged += (sender, e) =>
            {
                BurstControl.ItemsSource = Stores.DataLake;
                count = (uint)Math.Round((double)(Stores.DataLake.Count / 2)) + 1;
            };
            if (Stores.DataLake.Count == 0)
            {
                Stores.DataLake.Add(new Data(BurstType.CPU, count, 1));
                BurstControl.ItemsSource = Stores.DataLake;
            }
            else
                BurstControl.ItemsSource = Stores.DataLake;

            AddButton.Click += (sender, e) =>
            {
                Stores.DataLake.Add(new Data(BurstType.IO, count, 1));
                Stores.DataLake.Add(new Data(BurstType.CPU, count, 1));
            };

            ClearButton.Click += (sender, e) =>
            {
                Stores.DataLake.Clear();
                Stores.DataLake.Add(new Data(BurstType.CPU, count, 1));
            };
        }

        private void EditBurst_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
