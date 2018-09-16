using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;

namespace ReligionChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string pathToStates;
        private ObservableCollection<State> states;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PathToStates = "Path to history/states/";
            versionLabel.Text = $"Version: {Assembly.GetExecutingAssembly().GetName().Version}";
            states = new ObservableCollection<State>();               
            table.ItemsSource = states;    
        }

        public string PathToStates { get => pathToStates; set { pathToStates = value; RaisePropertyChanged("PathToStates"); } }

        public void RaisePropertyChanged(string propertyName)
        {          
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void pathSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", "Hearts of Iron IV", "mod");

                dialog.Description = "Please set the directory to history/states/";
                dialog.SelectedPath = defaultPath;
                dialog.ShowDialog();
                PathToStates = dialog.SelectedPath;
            }
        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadData();
            }
            catch(Exception)
            {
                System.Windows.MessageBox.Show(this, "Can not parse the religion data in the files, please check the path is correct. \nThe path must be ended with \'/history/states\'", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }           
        }         

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {           
            try
            {
                SaveData();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show(this, "The list of states is empty", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {          
            var files = Directory.GetFiles(PathToStates, "*.txt", SearchOption.TopDirectoryOnly);

            int count = 0;
            foreach (var file in files)
            {
                states.Add(new State { Name = Path.GetFileNameWithoutExtension(file), Religion = State.GetReligion(File.ReadAllLines(file).ToList()) });
                count++;
                progressBar.Dispatcher.Invoke(() => progressBar.Value = GetPercentage(count, files.Length));
            }

            if (states.Count > 0)
                statusText.Text = "All data successfully loaded";
            else
                throw new Exception();               
        }

        private void SaveData()
        {           
            Task.Run(() =>
            {
                statusText.Dispatcher.Invoke(() => { statusText.Text = "Saving..."; });
                var files = Directory.GetFiles(PathToStates, "*.txt", SearchOption.TopDirectoryOnly);              

                int count = 0;
                foreach (var state in states)
                {
                    List<string> buffer = File.ReadAllLines(pathToStates + "\\" + state.Name + ".txt").ToList();
                    bool hasReligionFlag = false;

                    for (int i = 0; i < buffer.Count; i++)
                    {
                        if (buffer[i].Contains("set_state_flag"))
                        {
                            buffer[i] = $"\t\tset_state_flag = {state.Religion.ToString()}";
                            hasReligionFlag = true;
                            File.WriteAllLines(pathToStates + "\\" + state.Name + ".txt", buffer.ToArray());
                            count++;
                            break;
                        }
                    }

                    if (!hasReligionFlag)
                    {
                        for (int i = 0; i < buffer.Count; i++)
                        {
                            if (buffer[i].Contains("history"))
                            {
                                buffer.Insert(i + 2, $"\t\tset_state_flag = {state.Religion.ToString()}");
                                File.WriteAllLines(pathToStates + "\\" + state.Name + ".txt", buffer.ToArray());
                                count++;
                                break;
                            }
                        }
                    }

                    progressBar.Dispatcher.Invoke(() => progressBar.Value = GetPercentage(count, states.Count));
                }
                progressBar.Dispatcher.Invoke(() => { progressBar.Value = 100; });
                statusText.Dispatcher.Invoke(() => { statusText.Text = "All data successfully saved"; });
            });            
        }

        private void table_AutoGeneratingColumn(object sender, System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "Name")
            {
                // e.Cancel = true;   // For not to include               
                e.Column.IsReadOnly = true; // Makes the column as read only
            }
        }

        private int GetPercentage(int value, int maxValue)
        {
            return value * 100 / maxValue;
        }        
    }
}
