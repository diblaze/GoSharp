using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GoSharp.Data;
using Path = System.IO.Path;

namespace GoSharp
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for LoadWindow.xaml
    /// </summary>
    public partial class LoadWindow : Window
    {
        public LoadWindow(JsonSerialization jsonSerialization)
        {
            InitializeComponent();
            LoadSaveFiles(jsonSerialization);

        }

        /// <summary>
        /// Loads all save files, ordered by date.
        /// </summary>
        /// <param name="jsonSerialization"><c>JsonSerialization</c></param>
        private void LoadSaveFiles(JsonSerialization jsonSerialization)
        {
 
            string pathToLoadFrom = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Get all saves
            List<BoardData> saves = jsonSerialization.GetAllSavesInFolder<BoardData>(pathToLoadFrom);

            //Order them 
            saves = jsonSerialization.OrderSavesByLatestDate(saves);

            //Show them
            ListViewGames.ItemsSource = saves;


        }

        private void BtnSelectGame_OnClick(object sender, RoutedEventArgs e)
        {
            var save = ListViewGames.SelectedItem as BoardData;

            ((MainWindow) Application.Current.Windows[0])?.LoadGame(save);

            Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ListViewGames_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnSelectGame.IsEnabled = true;
        }
    }
}
