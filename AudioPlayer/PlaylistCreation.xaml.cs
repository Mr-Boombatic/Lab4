using System;
using System.Collections.Generic;
using System.Linq;
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

namespace AudioPlayer
{
    /// <summary>
    /// Логика взаимодействия для PlaylistCreation.xaml
    /// </summary>
    /// 

    public partial class PlaylistCreation : Window
    {
        private MainWindow parent;

        public PlaylistCreation(MainWindow mainWindow)
        {
            InitializeComponent();
            parent = mainWindow;
        }

        public PlaylistCreation()
        {
            InitializeComponent();
        }

        public Categories DefineСategory(string category) 
        {
            switch (category)
            {
                case "Название":
                    return Categories.Title;
                case "Альбом":
                    return Categories.Album;
                case "Исполнитель":
                    return Categories.Performer;
            }

            throw new Exception("Нету такой категории");
        }

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {
            if (Сategory.Text != null && PlaylistName.Text != null)
            {
                parent.AddPlaylist(this.DefineСategory(Сategory.Text), PlaylistName.Text);
                parent.DisplayPlaylists();
                this.Close();
            }
        }
    }
}
