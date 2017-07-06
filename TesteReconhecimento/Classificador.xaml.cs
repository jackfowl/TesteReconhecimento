using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Página em Branco está documentado em https://go.microsoft.com/fwlink/?LinkId=234238

namespace TesteReconhecimento
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class Classificador : Page
    {
        BitmapImage _bitmap;

        public Classificador()
        {
            this.InitializeComponent();
        }

        private async void btnGetCamera_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            var file = await openPicker.PickSingleFileAsync() ;
            if (file != null)
            {
                if (_bitmap == null)
                    _bitmap = new BitmapImage();

                using (IRandomAccessStream stream = await GetBitmapStreamAsync(file))
                {
                    _bitmap.SetSource(stream);
                    imgSource.Source = _bitmap;

                    HaarClassifier hc = new HaarClassifier();
                    hc.HaarFile = @"Classifiers/face.xml";
                    hc.Neighboors = 3;
                    hc.Scale = 1.1;

                    using (var ms = await hc.GetImageWithMatches(await GetAsyncMemoryStream(stream)))
                    {
                        BitmapImage bitmap2 = new BitmapImage();
                        await bitmap2.SetSourceAsync(ms.AsRandomAccessStream());
                        imgDetected.Source = bitmap2;
                    }
                }
            }
        }

        private async Task<MemoryStream> GetAsyncMemoryStream(IRandomAccessStream stream)
        {
            var dummy = new MemoryStream();
            stream.Seek(0);
            await stream.AsStreamForRead().CopyToAsync(dummy);
            return dummy;
        }

        private async Task<IRandomAccessStream> GetBitmapStreamAsync(StorageFile file)
        {
            IRandomAccessStream stream = await file.OpenReadAsync().AsTask().ConfigureAwait(false);
            return stream;
        }

        private  void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }

}
