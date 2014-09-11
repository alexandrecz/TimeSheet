using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TimeSheet.Resources;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace TimeSheet.Views
{
    public partial class InvoiceView : PhoneApplicationPage
    {
        #region attributes

        private DataTransferManager dataTransferManager;
        private ApplicationBarIconButton share = new ApplicationBarIconButton();

        #endregion


        public InvoiceView()
        {
            InitializeComponent();
            DataContext = App.AppInvoiceViewModel;

            BuildAppBar();
            this.Loaded += (s, e) =>
                  {
                      Save();
                  };
        }

        #region methods

        private void BuildAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.BackgroundColor = new SolidColorBrush((App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush).Color).Color;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = false;

            share.IconUri = new Uri("/Toolkit.Content/share.png", UriKind.Relative);
            share.Text = AppResources.AppBarShare;

            ApplicationBar.Buttons.Add(share);

            share.Click += (s, e) =>
            {
                DataTransferManager.ShowShareUI();
            };
        }


        private async void Save()
        {
            LayoutRoot.UpdateLayout();
            var wb = new WriteableBitmap((int)LayoutRoot.ActualWidth, (int)LayoutRoot.ActualHeight);
            wb.Render(LayoutRoot, null);
            wb.Invalidate();
            using (var stream = new MemoryStream())
            {
                string fileName = "1Invoice.jpg";
                //1240 x 1754 
                wb.SaveJpeg(stream, 4960, 7016, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);

                await SaveToLocalFolderAsync(stream, fileName);
            }
        }


        private async Task SaveToLocalFolderAsync(Stream file, string fileName)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (Stream outputStream = await storageFile.OpenStreamForWriteAsync())
            {
                await file.CopyToAsync(outputStream);
            }
        }


        private async void InvoiceView_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var dp = args.Request.Data;
            var deferral = args.Request.GetDeferral();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            var photoFile = await localFolder.GetFileAsync("1Invoice.jpg");
            dp.Properties.Title = AppResources.ShareInvoiceTitle;
            dp.Properties.Description = AppResources.ShareInvoiceDesc;
            dp.SetStorageItems(new List<StorageFile> { photoFile });
            deferral.Complete();
        }

        /// <summary>
        /// Next version it'll rocks
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        [Obsolete]
        private bool SaveImageToPhotoHub(WriteableBitmap bmp)
        {
            using (var mediaLibrary = new MediaLibrary())
            {
                using (var stream = new MemoryStream())
                {
                    var fileName = string.Format("Gs{0}.jpg", Guid.NewGuid());
                    bmp.SaveJpeg(stream, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                    stream.Seek(0, SeekOrigin.Begin);
                    var picture = mediaLibrary.SavePicture(fileName, stream);
                    if (picture.Name.Contains(fileName)) return true;
                }
            }
            return false;
        }


        #endregion

        #region events

        //share register
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += InvoiceView_DataRequested;
        }


        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            dataTransferManager.DataRequested -= InvoiceView_DataRequested;
        }

        #endregion

    }
}