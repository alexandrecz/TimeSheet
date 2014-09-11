using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Threading;
using System.Xml.Serialization;
using TimeSheet.Resources;


namespace TimeSheet.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public enum TimerStatus
        {
            Paused,
            Running,
            Stopped
        }

        #region attributes

        private int startTime = 0;
        private DispatcherTimer newTimer;
        private int tic = 0;
        private int ticM = 0;
        private int ticH = 0;
        private TimerStatus timerStatus = TimerStatus.Stopped;
        private bool saveEnable = false;
        private bool beginEnable = false;
        private string useProjectSwitch = AppResources.AskN;

        private MyTask myTaskModel = new MyTask();

        private string hour = "00";
        private string min = "00";
        private string sec = "00";

        #region tasks

        private readonly object _sync = new object();

        private List<MyTask> mytaskList = new List<MyTask>();

        #endregion

        #endregion

        #region constructor

        public MainViewModel()
        {
            this.MyTaskModel.PropertyChanged += (s, e) =>
            {
                BeginEnable = (!string.IsNullOrEmpty(this.MyTaskModel.Description));
            };
        }

        #endregion

        #region properties


        public List<MyTask> MyTaskList
        {
            get { return mytaskList; }
            set
            {
                if (mytaskList != null)
                {
                    mytaskList = value;
                    NotifyPropertyChanged("MyTaskList");
                }
            }
        }


        public MyTask MyTaskModel
        {
            get { return myTaskModel; }
            set { myTaskModel = value; NotifyPropertyChanged("MyTaskModel"); }
        }

        public bool BeginEnable
        {
            get
            {
                return beginEnable;
            }
            set { beginEnable = value; NotifyPropertyChanged("BeginEnable"); }
        }

        public bool SaveEnable
        {
            get { return saveEnable; }
            set { saveEnable = value; NotifyPropertyChanged("SaveEnable"); }
        }

        public string UseProjectSwitch
        {
            get { return useProjectSwitch; }
            set { useProjectSwitch = value; NotifyPropertyChanged("UseProjectSwitch"); }
        }

        public string Min
        {
            get { return min; }
            set { min = value; NotifyPropertyChanged("Min"); }
        }

        public string Sec
        {
            get { return sec; }
            set { sec = value; NotifyPropertyChanged("Sec"); }
        }


        public string Hour
        {
            get { return hour; }
            set { hour = value; NotifyPropertyChanged("Hour"); }
        }

        public TimerStatus TimerStatusTask
        {
            get { return timerStatus; }
            set { timerStatus = value; NotifyPropertyChanged("TimerStatusTask"); }
        }


        #endregion

        #region methods

        public void Start()
        {
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            if (startTime == 0)
            {
                startTime = DateTime.Now.Minute;
            }

            if (TimerStatusTask == TimerStatus.Stopped)
            {
                this.newTimer = new DispatcherTimer();
                this.newTimer.Interval = TimeSpan.FromSeconds(1);
                this.newTimer.Tick += (o, s) =>
                {
                    this.tic += ((DispatcherTimer)(o)).Interval.Seconds;

                    if (this.tic == 60)
                    {
                        this.tic = 0;
                        Sec = "00";
                        Min = (ticM == 60 ? ticM = 0 : ticM += 1).ToString();

                        if (this.ticM == 60)
                        {

                            Sec = "0";
                            this.ticM = 0;
                            Min = "00";
                            Hour = (ticH == 60 ? ticH = 0 : ticH += 1).ToString();
                        }

                        Min = Min.Length == 1 ? "0" + Min : Min;
                        Hour = Hour.Length == 1 ? "0" + Hour : Hour;
                    }
                    else
                    {
                        Sec = this.tic.ToString();
                    }
                    Sec = Sec.Length == 1 ? "0" + Sec : Sec;

                    SaveEnable = false;
                };

                newTimer.Start();
                TimerStatusTask = TimerStatus.Running;
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            if (TimerStatusTask == TimerStatus.Running)
            {
                newTimer.Stop();
                TimerStatusTask = TimerStatus.Paused;

                SaveEnable = true;
            }
            else if (TimerStatusTask == TimerStatus.Paused)
            {
                TimerStatusTask = TimerStatus.Stopped;
                Start();
            }
            else
            {
                TimerStatusTask = TimerStatus.Running;
                Start();
            }
        }

        public void Reset()
        {
            newTimer.Stop();
            Hour = "00";
            Min = "00";
            Sec = "00";
            tic = 0;
            ticM = 0;
            ticH = 0;
            TimerStatusTask = TimerStatus.Stopped;
            SaveEnable = false;
        }

        public void SaveTask()
        {
            MyTaskModel.ID = (MyTaskList.Count > 0) ? MyTaskList.FindLast(x => x.TaskDate < MyTaskModel.TaskDate).ID + 1 : MyTaskList.Count + 1;

            WorkedHourValue();

            MyTaskList.Add(MyTaskModel);
            SaveViewModelToIsolatedStorage();
            RenewTaskModel();
        }

        public void WorkedHourValue()
        {
            string dec = MyTaskModel.Duration.Split(':')[1];
            string inte = MyTaskModel.Duration.Split(':')[0];

            if (Int32.Parse(dec) >= 30 || inte == "00")
            {
                dec = "5";
            }
            else
            {
                dec = "0";
            }

            MyTaskModel.Total = (Double.Parse(MyTaskModel.ValueBy) * Double.Parse(string.Format("{0}.{1}", inte, dec))).ToString();
        }

        public void DeleteTask(MyTask task)
        {
            MyTaskList.Remove(task);
            SaveViewModelToIsolatedStorage();
        }

        #region Isolated

        private void SaveViewModelToIsolatedStorage()
        {
            DeleteStorage();
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream =
                   new IsolatedStorageFileStream("ViewModel.xml", FileMode.Create, FileAccess.Write, store))
                {
                    var serializer = new XmlSerializer(typeof(List<MyTask>));
                    serializer.Serialize(stream, MyTaskList);
                }
            }
        }

        public void LoadViewModelFromIsolatedStorage()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream =
                   new IsolatedStorageFileStream("ViewModel.xml", FileMode.OpenOrCreate, FileAccess.Read, store))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        if (!reader.EndOfStream)
                        {
                            var serializer = new XmlSerializer(typeof(List<MyTask>));
                            MyTaskList = (List<MyTask>)serializer.Deserialize(reader);
                        }
                    }
                }
            }
        }

        private void DeleteStorage()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("ViewModel.xml"))
                {
                    store.DeleteFile("ViewModel.xml");
                }

                using (var f = new StreamWriter(new IsolatedStorageFileStream("ViewModel.xml", FileMode.CreateNew, store)))
                {
                    f.Flush();
                    f.Close();
                    if (store.FileExists("ViewModel.xml"))
                    {
                        store.DeleteFile("ViewModel.xml");
                    }
                }
            }
        }

        #endregion

        public void RenewTaskModel()
        {
            MyTaskModel.ID = 0;

            MyTaskModel.Description = string.Empty;
            MyTaskModel.Duration = string.Empty;
            MyTaskModel.TaskDate = DateTime.Now;
        }

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

    #region wrapper

    public class MyTask : INotifyPropertyChanged
    {
        public MyTask()
        {

        }

        private int id;
        private string description;
        private string duration;
        private string valueBy;
        private string total;
        private DateTime taskDate = DateTime.Now;

        public int ID
        {
            get { return id; }
            set { id = value; NotifyPropertyChanged("ID"); }
        }

        public string Description
        {
            get { return description; }
            set { description = value; NotifyPropertyChanged("Description"); }
        }

        public string ValueBy
        {
            get { return valueBy; }
            set { valueBy = value; NotifyPropertyChanged("ValueBy"); }
        }

        public string Total
        {
            get { return total; }
            set { total = value; NotifyPropertyChanged("Total"); }
        }

        public string Duration
        {
            get { return duration; }
            set { duration = value; NotifyPropertyChanged("Duration"); }
        }

        public DateTime TaskDate
        {
            get { return taskDate; }
            set { taskDate = value; NotifyPropertyChanged("TaskDate"); }
        }

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

    #endregion
}