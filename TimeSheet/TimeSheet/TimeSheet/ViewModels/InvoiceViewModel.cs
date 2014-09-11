
using System;
using System.ComponentModel;
namespace TimeSheet.ViewModels
{
    public class InvoiceViewModel : INotifyPropertyChanged
    {
        #region attributes

        private MyTask invoiceTask;
    
               
        #endregion

        #region constructors

        public InvoiceViewModel()
        {

        }

        #endregion


        #region properties

        public MyTask InvoiceTask
        {
            get { return invoiceTask; }
            set { invoiceTask = value; NotifyPropertyChanged("InvoiceTask"); }
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
}
