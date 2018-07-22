using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMocker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Go();
        }
        private FClipper clipper = null;
        private void Go()
        {
            new Thread(new ThreadStart(() =>
            {
                TextBoxStreamWriter t = new TextBoxStreamWriter(this.Dispatcher,OutputBox);
                clipper = new FClipper(5000);
                clipper.StartRecording();

            })).Start();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "RecordButton":
                    FClip clip = clipper.AddLastXSecondsToList();
                    ListBoxObject.Items.Add(clip.fileName);
                    break;
                case "SaveButton":

                    break;
                case "ListenButton":
                    string SelectedName = (string)(ListBoxObject.SelectedValue);
                    SoundPlayer simpleSound = new SoundPlayer(clipper.GetPath(SelectedName));
                    simpleSound.Play();
                    break;
                case "PlayButton":

                    break;
            }

        }



       
    }
}
