using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MgMvvmTools;
using RecordToClass.Extensions;
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
        private PropertyAccessor _selectedPropertyAccessor = PropertyAccessor.Get;
        public event PropertyChangedEventHandler PropertyChanged;

        public string RecordText
        {
            get => _recordText;
            set
            {
                _recordText = value;
                OnPropertyChanged(nameof(RecordText));
                UpdateResult();
            }
        }

        private async void UpdateResult()
        {
            if (string.IsNullOrWhiteSpace(RecordText))
            {
                Result = RecordText;
                return;
            }
            var syncContext = SynchronizationContext.Current;
            await Task.Run(() =>
            {
                syncContext.Send(() =>
                {
                    try
                    {
                        Result = Record.Parse(RecordText)
                            .ToClass(
                                SelectedPropertyAccessor,
                                DoUseThisKeyword,
                                DoCreateDeconstructor)
                            .ToString();
                    }
                    catch (Exception exception)
                    {
                        Result = exception.ToString();
                    }
                });
            });
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

        public PropertyAccessor[] PropertyAccessors { get; } = Enum.GetValues<PropertyAccessor>();

        public PropertyAccessor SelectedPropertyAccessor
        {
            get => _selectedPropertyAccessor;
            set
            {
                if (value == _selectedPropertyAccessor) return;
                _selectedPropertyAccessor = value;
                UpdateResult();
            }
        }

        private async Task FormatRecordTextAsync()
        {
            try
            {
                RecordText = await SyntaxNodeExtensions.FormatCodeAsync(RecordText);
            }
            catch
            {
            }
        }
        
        public ICommand FormatCommand => new Command(async () =>
        {
            await FormatRecordTextAsync();
        });

        private async Task PasteRecordAsync()
        {
            var text = Clipboard.GetText();
            if (text == "") return;
            RecordText = text;
            await FormatRecordTextAsync();
        }

        private void CopyClass()
        {
            Clipboard.SetText(Result);
        }

        public ICommand PasteRecordCommand => new Command(async () =>
        {
            await PasteRecordAsync();
        });
        public ICommand CopyClassCommand => new Command(CopyClass);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}