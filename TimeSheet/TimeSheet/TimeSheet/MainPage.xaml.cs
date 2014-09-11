using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using TimeSheet.Resources;
using TimeSheet.ViewModels;
using Windows.ApplicationModel.Email;
using Windows.Phone.Speech.VoiceCommands;

namespace TimeSheet
{
    public partial class MainPage : PhoneApplicationPage
    {

        #region attributes

        private ApplicationBar barMain;
        private ApplicationBar barTask;

        private ApplicationBarIconButton starTaskBtn = new ApplicationBarIconButton();
        private ApplicationBarIconButton compTaskBtn = new ApplicationBarIconButton();

        private ApplicationBarIconButton deleteTaskBtn = new ApplicationBarIconButton();
        private ApplicationBarIconButton selectTaskBtn = new ApplicationBarIconButton();

        private ApplicationBarIconButton invoiceTaskBtn = new ApplicationBarIconButton();

        private MultiselectList target;


        #endregion

        #region Constructor

        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;

            RefreshTasks();
            BuildAppBarMain();
            BuildAppBarTasks();
        }

        #endregion

        #region Tasks

        private void BuildAppBarMain()
        {
            barMain = new ApplicationBar();
            barMain.BackgroundColor = new SolidColorBrush((App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush).Color).Color;
            barMain.Mode = ApplicationBarMode.Default;
            barMain.IsVisible = true;
            barMain.IsMenuEnabled = false;

            starTaskBtn.Text = AppResources.ButtonActionStartText;
            starTaskBtn.IconUri = new Uri("/Toolkit.Content/play.png", UriKind.Relative);

            starTaskBtn.Click += (s, e) =>
            {
                SetStatus();
            };

            compTaskBtn.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.Relative);
            compTaskBtn.Text = AppResources.ButtonComplete;
            compTaskBtn.IsEnabled = false;

            compTaskBtn.Click += (s, e) =>
            {
                starTaskBtn.Text = AppResources.ButtonActionStartText;
                starTaskBtn.IconUri = new Uri("/Toolkit.Content/play.png", UriKind.Relative);
                compTaskBtn.IsEnabled = false;

                IAsyncResult result = Microsoft.Xna.Framework.GamerServices.Guide.BeginShowMessageBox("Question", AppResources.QuestionTaskComp, new string[] { AppResources.AskY, AppResources.AskN }, 0, Microsoft.Xna.Framework.GamerServices.MessageBoxIcon.None, null, null);
                result.AsyncWaitHandle.WaitOne();

                int? choice = Microsoft.Xna.Framework.GamerServices.Guide.EndShowMessageBox(result);
                if (choice.HasValue)
                {
                    if (choice.Value == 0)
                    {
                        App.ViewModel.MyTaskModel.Duration = string.Format("{0}:{1}:{2}", App.ViewModel.Hour, App.ViewModel.Min, App.ViewModel.Sec);
                        NavigationService.Navigate(new Uri("/Views/TaskView.xaml", UriKind.Relative));
                    }
                    else
                    {
                        App.ViewModel.RenewTaskModel();
                    }
                }

                App.ViewModel.Reset();
            };

            barMain.Buttons.Add(starTaskBtn);
            barMain.Buttons.Add(compTaskBtn);
        }

        private void BuildAppBarTasks()
        {
            barTask = new ApplicationBar();
            barTask.BackgroundColor = new SolidColorBrush((App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush).Color).Color;
            barTask.Mode = ApplicationBarMode.Default;
            barTask.IsVisible = true;
            barTask.IsMenuEnabled = false;

            selectTaskBtn.IsEnabled = false;
            selectTaskBtn.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.Relative);
            selectTaskBtn.Text = AppResources.AppBarSelect;
            barTask.Buttons.Add(selectTaskBtn);

            selectTaskBtn.Click += (s, e) =>
            {
                LongTasks.IsSelectionEnabled = !LongTasks.IsSelectionEnabled;
            };

            deleteTaskBtn.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.Relative);
            deleteTaskBtn.Text = AppResources.AppBarDelete;
            deleteTaskBtn.IsEnabled = false;
            barTask.Buttons.Add(deleteTaskBtn);

            deleteTaskBtn.Click += (s, e) =>
            {
                DeleteTask();
            };


            invoiceTaskBtn.IconUri = new Uri("/Toolkit.Content/invoice.png", UriKind.Relative);
            invoiceTaskBtn.Text = AppResources.AppBarInvoice;
            invoiceTaskBtn.IsEnabled = false;
            barTask.Buttons.Add(invoiceTaskBtn);

            invoiceTaskBtn.Click += (s, e) =>
            {
                NavigationService.Navigate(new Uri("/Views/InvoiceView.xaml", UriKind.Relative));
            };
        }

        private void RefreshTasks()
        {
            App.ViewModel.LoadViewModelFromIsolatedStorage();
            LongTasks.ItemsSource = null;
            LongTasks.ItemsSource = App.ViewModel.MyTaskList;
            selectTaskBtn.IsEnabled = App.ViewModel.MyTaskList.Count > 0;
        }

        private void SetStatus()
        {
            App.ViewModel.Start();
            if (App.ViewModel.TimerStatusTask == MainViewModel.TimerStatus.Running)
            {
                starTaskBtn.Text = AppResources.ButtonActionPauseText;
                starTaskBtn.IconUri = new Uri("/Toolkit.Content/pause.png", UriKind.Relative);
            }
            else if (App.ViewModel.TimerStatusTask == MainViewModel.TimerStatus.Paused)
            {
                starTaskBtn.Text = AppResources.ButtonActionResumeText;
                starTaskBtn.IconUri = new Uri("/Toolkit.Content/play.png", UriKind.Relative);
            }
            compTaskBtn.IsEnabled = true;
        }

        public void DeleteTask()
        {
            IList source = target.ItemsSource as IList;
            if (target.SelectedItems.Count == 1)
            {
                source.Remove((MyTask)target.SelectedItems[0]);
                if (MessageBox.Show(AppResources.QuestionTask, "Confirmation", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    App.ViewModel.DeleteTask((MyTask)target.SelectedItems[0]);
                    MessageBox.Show(AppResources.ConfirmTask);
                }
            }
            else
            {
                if (MessageBox.Show(AppResources.QuestionTasks, "Confirmation", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    foreach (MyTask item in target.SelectedItems)
                    {
                        source.Remove(item);
                        App.ViewModel.DeleteTask(item);
                    }

                    MessageBox.Show(AppResources.ConfirmTasks);
                }
            }

            RefreshTasks();

            if (App.ViewModel.MyTaskList.Count == 0)
            {
                PivotMain.SelectedIndex = 0;
            }
        }


        #endregion

        #region about

        private async Task SendEmail()
        {
            EmailMessage email = new EmailMessage();
            email.To.Add(new EmailRecipient("alexandrecz@live.com"));
            email.Subject = AppResources.EmailFeedBack;
            await EmailManager.ShowComposeNewEmailAsync(email);
        }

        private void RateMe_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceReviewTask mk = new Microsoft.Phone.Tasks.MarketplaceReviewTask();
            mk.Show();
        }

        private async void EmailMe_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await SendEmail();
        }

        #endregion

        #region events


        private void LongTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            target = (MultiselectList)sender;

            if (target.IsSelectionEnabled)
            {
                if (target.SelectedItems.Count > 1)
                {
                    selectTaskBtn.IsEnabled = true;
                    deleteTaskBtn.IsEnabled = true;
                    invoiceTaskBtn.IsEnabled = false;
                }
                else if (target.SelectedItems.Count == 1)
                {
                    selectTaskBtn.IsEnabled = true;
                    deleteTaskBtn.IsEnabled = true;
                    invoiceTaskBtn.IsEnabled = true;

                    App.AppInvoiceViewModel.InvoiceTask = target.SelectedItems[0] as MyTask;
                }
            }
            else
            {
                deleteTaskBtn.IsEnabled = false;
                invoiceTaskBtn.IsEnabled = false;
            }
        }

        private void Pivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (((Pivot)sender).SelectedIndex)
            {
                case 0:
                    {
                        ApplicationBar = barMain;
                        ApplicationBar.IsVisible = true;
                        break;
                    }
                case 1:
                    {
                        ApplicationBar = barTask;
                        ApplicationBar.IsVisible = true;
                        RefreshTasks();
                        break;
                    }
                case 2:
                    {
                        ApplicationBar.IsVisible = false;
                        break;
                    }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        #region Voice

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                string voiceCommandName;

                if (NavigationContext.QueryString.TryGetValue("voiceCommandName", out voiceCommandName))
                {
                    if (NavigationContext.QueryString["voiceCommandName"].ToString() == "TSShow")
                    {
                        string myCommand = NavigationContext.QueryString["myCommand"];
                        switch (myCommand)
                        {
                            case "tasks":
                                PivotMain.SelectedIndex = 1;
                                break;
                        }
                    }
                    else if (NavigationContext.QueryString["voiceCommandName"].ToString() == "TSAdd" || NavigationContext.QueryString["voiceCommandName"].ToString() == "TSCreate")
                    {
                        SetStatus();
                    }
                }
                else
                {
                    Task.Run(() => InstallVoiceCommands());
                }
            }
        }

        private async void InstallVoiceCommands()
        {
            const string wp81vcdPath = "ms-appx:///VoiceCommandDefinition.xml";

            try
            {
                bool using81orAbove = ((Environment.OSVersion.Version.Major >= 8)
                    && (Environment.OSVersion.Version.Minor >= 10));

                Uri vcdUri = new Uri(wp81vcdPath);

                await VoiceCommandService.InstallCommandSetsFromFileAsync(vcdUri);
            }
            catch (Exception vcdEx)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(String.Format(
                        AppResources.VoiceCommandInstallErrorTemplate, vcdEx.HResult, vcdEx.Message));
                });
            }
        }

        #endregion

        #endregion

        #region Notify

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}