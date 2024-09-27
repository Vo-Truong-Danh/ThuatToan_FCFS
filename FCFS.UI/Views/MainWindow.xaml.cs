using FCFS.Library;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace FCFS.UI.Views
{
    /// <summary>
    /// Interaction logic for Window.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Processes Data = new();
        public MainWindow()
        {
            InitializeComponent();

            /*Helper.AllocConsole();*/

            MouseDown += (sender, e) => FocusManager.SetFocusedElement(this, this);
            Loaded += (sender, e) => Accent.ApplySystemAccent();
            Closing += (sender, e) => Application.Current.Shutdown();
            Stores.DataLake.CollectionChanged += (sender, e) => BurstViewer.ItemsSource = Stores.DataLake;
            Data.Notification += () =>
            {
                if (Data.Count > 0)
                    TotalBlock.Text = $"Total processes: {Data.Count}";
                else
                    TotalBlock.Text = null;
                DataTable.ItemsSource = Data.GetProcesses;
                InfoBar.IsOpen = false;
            };
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Data.Add((uint)ArrivalTimeBox.Value, Stores.DataLake.ToList());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                InfoBar.Title = "Error";
                InfoBar.Message = ex.Message;
                InfoBar.Severity = InfoBarSeverity.Error;
                InfoBar.IsOpen = true;
                return;
            }
            ArrivalTimeBox.Value = 0;
            Stores.DataLake.Clear();
        }

        private void ExampleButton_Click(object sender, RoutedEventArgs e)
        {
            new ExampleDialog()
            {
                Owner = this,
                ShowInTaskbar = false
            }.ShowDialog();

        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (Data.Count == 0)
                return;
            MessageBoxResult result = System.Windows.MessageBox.Show(this, "Do you want to clear all data?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None);
            if (result == MessageBoxResult.Yes)
            {
                Data.Clear();
                ArrivalTimeBox.Value = 0;
                Stores.DataLake.Clear();
                GC.Collect();
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Data.Calculate();
            }
            catch (Exception ex)
            {
                InfoBar.Title = "Error";
                InfoBar.Message = ex.Message;
                InfoBar.Severity = InfoBarSeverity.Error;
                InfoBar.IsOpen = true;
                return;
            }
            new Calculate().Show();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            new EditBurst()
            {
                ShowInTaskbar = false,
                Owner = this,
            }.ShowDialog();
        }

        private void Scroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scroll = sender as ScrollViewer;
            if (e.Delta > 0)
                scroll.LineLeft();
            else
                scroll.LineRight();
            e.Handled = true;
        }

        void ModifyMode()
        {
            MessageBoxResult result;
            if (ArrivalTimeBox.Value != 0 || Stores.DataLake.Count > 0)
                result = System.Windows.MessageBox.Show(this, "Do you want to replace current arrival time and burst times?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None);
            else
                result = MessageBoxResult.Yes;
            if (result == MessageBoxResult.Yes)
            {
                int index = DataTable.SelectedIndex;
                var p = Data.GetAt(index);
                ArrivalTimeBox.Value = p.ArrivalTime;
                Stores.DataLake = new ObservableCollection<Data>(p.BurstTimes);
                BurstViewer.ItemsSource = Stores.DataLake;
                Data.RemoveAt(DataTable.SelectedIndex);
            }
        }

        private void DataTable_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataTable.IsEnabled == false)
                return;

            switch (e.Key)
            {
                case Key k when (k == Key.Delete || k == Key.Back):
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(this, "Do you want to delete selected process?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None);
                        if (result == MessageBoxResult.Yes)
                            Data.RemoveAt(DataTable.SelectedIndex);
                        break;
                    }
                case Key k when ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && k == Key.E):
                    {
                        ModifyMode();
                        break;
                    }
                case Key k when ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && k == Key.R):
                    {
                        DataTable.ItemsSource = Data.GetProcesses;
                        break;
                    }
            }
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            ModifyMode();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(this, "Do you want to delete selected process?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None);
            if (result == MessageBoxResult.Yes)
                Data.RemoveAt(DataTable.SelectedIndex);
        }
    }
}