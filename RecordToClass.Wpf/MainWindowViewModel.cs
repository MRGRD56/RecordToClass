using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RecordToClass.Models;
using RecordToClass.Wpf.Annotations;

namespace RecordToClass.Wpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _result;
        private string _recordText;
        public event PropertyChangedEventHandler PropertyChanged;

        public string RecordText
        {
            get => _recordText;
            set
            {
                _recordText = value;
                UpdateResult();
            }
        }

        private void UpdateResult()
        {
            try
            {
                Result = Record.Parse(RecordText).ToClassString();
            }
            catch (Exception exception)
            {
                Result = exception.ToString();
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                if (value == _result) return;
                _result = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}