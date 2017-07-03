using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Core;


// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x416

namespace TesteReconhecimento
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private VideoCapture _capture;
        private bool _CaptureEnabled;

        public MainPage()
        {
            this.InitializeComponent();
            FillClassifiers();
            FillImages();
            Window.Current.VisibilityChanged += (sender, args) =>
            {
                CvInvoke.WinrtOnVisibilityChanged(args.Visible);
            };
            
            CvInvoke.WinrtSetFrameContainer(this.imgFinal);

            CvInvoke.WinrtStartMessageLoop(Process);

        }

        private async void FillImages()
        {
            cbImages = (ComboBox)FindName("cbImages");
            cbImages.SelectedValuePath = "Name";
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Images");
            var files = await assets.GetFilesAsync();
            cbImages.ItemsSource = files;
        }

        private async void FillClassifiers()
        {
            cbClassifier = (ComboBox)FindName("cbClassifier");
            cbClassifier.SelectedValuePath = "Name";
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Classifiers");
            var files = await assets.GetFilesAsync();
            cbClassifier.ItemsSource = files;
        }

        private async void btnReconhecer_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                HaarClassifier c = new HaarClassifier();
                c.HaarFile = "Classifiers/" + cbClassifier.SelectedValue.ToString();
                c.Scale = Convert.ToSingle(txtScale.Text);
                c.Neighboors = Convert.ToInt32(txtNeighboors.Text);
                BitmapImage i = new BitmapImage();
                var ms = await c.GetImageWithMatches("Images/" + cbImages.SelectedValue.ToString());
                await i.SetSourceAsync(ms.AsRandomAccessStream());
                imgFinal.Source = i;
            }
            catch (Exception except)
            {
                MessageDialog showDialog = new MessageDialog(except.Message);
                await showDialog.ShowAsync();
            }
        }

        private void Process()
        {
            Mat m = new Mat();
            Mat mProcessed = new Mat();
            while (true)
            {
                if (_CaptureEnabled)
                {
                    try
                    {
                        if (_capture == null)
                            _capture = new VideoCapture();

                        _capture.Read(m); //WHY EMPTY

                        if (!m.IsEmpty)
                        {
                            //render the processed image on the top imageview
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                async () =>
                                {
                                    var wb = mProcessed.ToWritableBitmap();
                                    imgFinal.Source = wb;
                                });

                            //The data in the mat that is read from the camera will 
                            //be drawn to the Image control
                            CvInvoke.WinrtImshow();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    _CaptureEnabled = false;
                }
                else
                {
                    if (_capture != null)
                    {
                        _capture.Dispose();
                        _capture = null;
                    }

                    Task t = Task.Delay(100);
                    t.Wait();
                }
            }
        }

        private void btnGetCamera_ClickAsync(object sender, RoutedEventArgs e)
        {

            _CaptureEnabled = true;
        }
    }
}
