using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows.Media;
using TimeSheet.Resources;


namespace TimeSheet.Views
{
    public partial class TaskView : PhoneApplicationPage
    {
        private string val = "0";
        ApplicationBarIconButton saveBtn = new ApplicationBarIconButton();
        ApplicationBarIconButton backBtn = new ApplicationBarIconButton();

        public TaskView()
        {
            InitializeComponent();

            BuildAppBar();
            DataContext = App.ViewModel;
            (DataContext as ViewModels.MainViewModel).PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "BeginEnable")
                    {
                        saveBtn.IsEnabled = App.ViewModel.BeginEnable;
                    }
                };
        }


        private void BuildAppBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.BackgroundColor = new SolidColorBrush((App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush).Color).Color;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = false;

            saveBtn.IsEnabled = App.ViewModel.BeginEnable;
            saveBtn.IconUri = new Uri("/Toolkit.Content/save.png", UriKind.Relative);
            saveBtn.Text = AppResources.AppBarSave;

            backBtn.IconUri = new Uri("/Toolkit.Content/stop.png", UriKind.Relative);
            backBtn.Text = AppResources.ButtonCancel;

            ApplicationBar.Buttons.Add(saveBtn);
            ApplicationBar.Buttons.Add(backBtn);

            saveBtn.Click += (s, e) =>
                {

                    App.ViewModel.MyTaskModel.ValueBy = val;
                    App.ViewModel.SaveTask();
                    NavigationService.GoBack();
                };

            backBtn.Click += (s, e) =>
            {
                App.ViewModel.RenewTaskModel();
                NavigationService.GoBack();
            };

        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            val = ((System.Windows.Controls.TextBox)(((System.Windows.Controls.Control)(sender)))).Text;
        }
    }
}