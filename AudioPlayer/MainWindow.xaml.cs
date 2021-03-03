using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace AudioPlayer
{
    public enum Categories
    {
        Performer,
        Album,
        Title,
        Absent
    }
    public partial class MainWindow : Window
    {
        MediaElement AudioPlayer = new MediaElement();
        DispatcherTimer Timer = new DispatcherTimer();
        DispatcherTimer TimerReplay = new DispatcherTimer();

        bool playing = true;
        bool replay = false;

        public MainWindow()
        {
            InitializeComponent();
            Playlists.Add(new Tuple<Categories, string>(Categories.Title, "Default"), new List<string>());
            DisplayPlaylists();
        }
        void Play(object sender, RoutedEventArgs e)
        {
            if (playing)
            {
                AudioPlayer.Pause();
                play.Style = FindResource("RoundedButtonPlay") as Style;
                Timer.Stop();
                playing = false;
            }
            else
            {
                AudioPlayer.Play();
                play.Style = FindResource("RoundedButtonPause") as Style;
                Timer.Start();
                playing = true;
            }

        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            replay = !replay;
        }

        string[] Formats = new string[] { ".aif", ".m3u", ".m4a", ".mid", ".mp3", ".mpa", ".wav", ".wma" };
        private void AddWithMenu(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = false
            };
            bool? dialogOk = fileDialog.ShowDialog();

            if (dialogOk == true)
            {
                Playlists[SelectedPlaylist].Add(fileDialog.FileName);
                DisplaySelectedPlaylist();
            }
        }

        /* Получить расположение выбранной песни песни */
        string GetPathByTrack(string name)
        {
            var result = from n in Playlists[SelectedPlaylist]
                         where n.IndexOf(name) != -1
                         select n;

            return result.First();
        }

        private void media1_MediaOpened(object sender, RoutedEventArgs e)
        {
            playingTime.Maximum = AudioPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            Timer.Interval = TimeSpan.FromSeconds(0.1);
            Timer.Tick += TickSlider;
        }
        private void TickSlider(object sender, EventArgs e)
        {
            time.Content = AudioPlayer.Position.ToString(@"mm\:ss");
            time.Visibility = Visibility.Visible;
            playingTime.Value = AudioPlayer.Position.TotalSeconds;

            if (AudioPlayer.Position.TotalSeconds == AudioPlayer.NaturalDuration.TimeSpan.TotalSeconds && replay)
            {
                AudioPlayer.Stop();             
                playing = false;
                Play(new object(), new RoutedEventArgs());
            }
        }
        private void TickReplay(object sender, EventArgs e)
        {
            this.Play(this.play, new RoutedEventArgs());
        }
        private void _time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AudioPlayer.Source != null)
            {
                AudioPlayer.Pause();
                AudioPlayer.Position = TimeSpan.FromSeconds(playingTime.Value);
                AudioPlayer.Play();
            }
        }
        private void Delete(object sender, RoutedEventArgs e)
        {
            if (downWrapPanel != null && e.Source is Button)
            {
                var lbl = ((Label)downWrapPanel.Children[0]).Content.ToString();
                var remote = Playlists[SelectedPlaylist].Where(k => System.IO.Path.GetFileName(k) == lbl).First();
                Playlists[SelectedPlaylist].Remove(remote);
                DisplaySelectedPlaylist();
            }
        }

        private void RewindNext(object sender, RoutedEventArgs e)
        {
            AudioPlayer.Position = AudioPlayer.Position.Add(new TimeSpan(0, 0, 5));
        }

        private void RewindPrev(object sender, RoutedEventArgs e)
        {
            var sdf = new TimeSpan(0, 0, 5);
            AudioPlayer.Position = AudioPlayer.Position.Add(new TimeSpan(0, 0, -5));
        }

        private void ChangeVolume(object sender, RoutedEventArgs e)
        {
            VolumeСontrol.IsOpen = !VolumeСontrol.IsOpen;
        }

        private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioPlayer.Volume = ((Slider)sender).Value / 100;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AudioPlayer.Balance = ((Slider)sender).Value / 100;
        }

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {
            PlaylistCreation creationPlaylistWindow = new PlaylistCreation(this);
            creationPlaylistWindow.Owner = this;
            creationPlaylistWindow.Show();
        }

        /////////////////////////////////////////////////////////////////////

        public Dictionary<Tuple<Categories, string>, List<string>> Playlists = new Dictionary<Tuple<Categories, string>, List<string>>();   // содержит все плейлисты
        public Tuple<Categories, string> SelectedPlaylist = new Tuple<Categories, string>(Categories.Title, "Default");                     // string - название выбранного плейлиста, Сategories - категория выбранного плейлиста

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

        public void AddPlaylist(Categories category, string name)
        {
            Playlists.Add(new Tuple<Categories, string>(category, name), new List<string>());
        }
        public void DisplayPlaylists()
        {
            if (playlists != null)
                playlists.Children.Clear();

            foreach (var playlist in Playlists)
            {
                bool displaying = false;
                foreach (CheckBox condition in filtration.Children)
                {
                    if (condition.IsChecked == true && DefineСategory(condition.Content.ToString()) == playlist.Key.Item1)
                    {
                        Label textOfItem = null;

                        if (SelectedPlaylist.Item2 == playlist.Key.Item2)
                            textOfItem = new Label() { FontWeight = FontWeights.UltraBold, Content = playlist.Key.Item2 };
                        else
                            textOfItem = new Label() { Content = playlist.Key.Item2 };

                        playlists.Children.Add(textOfItem);
                        textOfItem.MouseDown += SelectPlaylist;
                    }
                }       
            }
        }
        private void SelectPlaylist(object sender, MouseButtonEventArgs e)
        {
            Label selectedPlaylist = (Label)sender;

            foreach (var playlist in Playlists)
            {
                if (selectedPlaylist.Content == playlist.Key.Item2)
                {
                    SelectedPlaylist = playlist.Key;
                }
            }

            DisplaySelectedPlaylist();
            DisplayPlaylists();
        }
        public void DisplaySelectedPlaylist()
        {
            Music.Children.Clear();

            foreach (var music in Playlists[SelectedPlaylist])
            {
                var textOfItem = new Label() { Content = System.IO.Path.GetFileName(music) };

                textOfItem.PreviewMouseDown += ScrollViewer_MouseDown;
                textOfItem.MouseDoubleClick += delegate (object a, MouseButtonEventArgs b)
                {
                    var track = GetPathByTrack(((Label)a).Content.ToString());
                    Opened.Text = System.IO.Path.GetFileName(track);
                    AudioPlayer.Source = new Uri(track);
                    AudioPlayer.MediaOpened += media1_MediaOpened;
                    AudioPlayer.LoadedBehavior = MediaState.Manual;
                    AudioPlayer.UnloadedBehavior = MediaState.Manual;
                    AudioPlayer.Play();
                    Timer.Start();
                };

                var delete = new Button() { Style = FindResource("RoundedButtonStyleMinus") as Style, Width = 20, Height = 20 };
                delete.PreviewMouseDown += Delete;

                var item = new WrapPanel() { AllowDrop = true };
                item.Children.Add(textOfItem);
                item.Children.Add(delete);
                item.PreviewMouseDown += ScrollViewer_MouseDown;
                item.Drop += ScrollViewer_Drop;

                Music.Children.Add(item);
            }
        }

        private void Drop(object sender, DragEventArgs e)
        {
            var file = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (file != null && !Playlists[SelectedPlaylist].Contains(file[0]))
                Playlists[SelectedPlaylist].Add(file[0]);

                DisplaySelectedPlaylist();
        }

        private void Performer_Checked(object sender, RoutedEventArgs e)
        {
            DisplayPlaylists();
        }

        private void ScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as WrapPanel) != null)
                downWrapPanel = (WrapPanel)sender;

            if (e.Source as Button == null)
            {
                DragDrop.DoDragDrop(downWrapPanel.Children[0], ((Label)downWrapPanel.Children[0]).Content, DragDropEffects.Copy);
            }
        }

        WrapPanel downWrapPanel;

        private void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            var lbl = e.Source as Label;
            if (lbl != null) 
            {
                var path1 = Playlists[SelectedPlaylist].Where(k => System.IO.Path.GetFileName(k) == lbl.Content.ToString()).First();
                var path2 = Playlists[SelectedPlaylist].Where(k => System.IO.Path.GetFileName(k) == ((Label)downWrapPanel.Children[0]).Content.ToString()).First();

                var swap1 = Playlists[SelectedPlaylist].IndexOf(path1);
                var swap2 = Playlists[SelectedPlaylist].IndexOf(path2);

                var temp = Playlists[SelectedPlaylist][swap1];
                Playlists[SelectedPlaylist][swap1] = Playlists[SelectedPlaylist][swap2];
                Playlists[SelectedPlaylist][swap2] = temp;
            }
        }

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {
            var fbd = new WinForms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var dir = new DirectoryInfo(fbd.SelectedPath);

                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo f in files)
                    {
                        if (Formats.Contains(f.Extension))
                        {
                            Playlists[SelectedPlaylist].Add(f.FullName);
                        }
                    }
                }
                catch
                {
                }
            }

            DisplaySelectedPlaylist();
        }

        private void ClearPlaylist(object sender, RoutedEventArgs e)
        {
            Playlists[SelectedPlaylist].Clear();
            DisplaySelectedPlaylist();
        }

        private void SortSelectedPlaylist(object sender, RoutedEventArgs e)
        {
            var sortingPlayList = (from music in Playlists[SelectedPlaylist]
                                  orderby music descending
                                  select music).ToArray();

            Playlists[SelectedPlaylist].Clear();
            Playlists[SelectedPlaylist] = new List<string>(sortingPlayList);
            DisplaySelectedPlaylist();
        }



        /////////////////////////////////////////////////////////////////////


    }
}
