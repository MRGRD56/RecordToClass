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
        private bool _doUseThisKeyword;
        private bool _doCreateDeconstructor = true;
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
                Result = Record.Parse(RecordText)
                    .ToClassString(
                        useThisKeyword: DoUseThisKeyword, 
                        createDeconstructor: DoCreateDeconstructor);
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

        public bool DoUseThisKeyword
        {
            get => _doUseThisKeyword;
            set
            {
                if (value == _doUseThisKeyword) return;
                _doUseThisKeyword = value;
                UpdateResult();
            }
        }

        public bool DoCreateDeconstructor
        {
            get => _doCreateDeconstructor;
            set
            {
                if (value == _doCreateDeconstructor) return;
                _doCreateDeconstructor = value;
                UpdateResult();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}