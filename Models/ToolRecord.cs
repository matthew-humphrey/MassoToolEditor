using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MassoToolEditor.Models
{
    public class ToolRecord : INotifyPropertyChanged
    {
        private string _toolName = string.Empty;
        private float _zOffset;
        private float _toolDiameter;
        private float _toolDiaWear;
        private uint _crc;

        public int ToolNumber { get; set; }

        public string ToolName
        {
            get => _toolName;
            set
            {
                if (_toolName != value)
                {
                    _toolName = value;
                    OnPropertyChanged();
                }
            }
        }

        public float ZOffset
        {
            get => _zOffset;
            set
            {
                if (_zOffset != value)
                {
                    _zOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        public float ToolDiameter
        {
            get => _toolDiameter;
            set
            {
                if (_toolDiameter != value)
                {
                    _toolDiameter = value;
                    OnPropertyChanged();
                }
            }
        }

        public float ToolDiaWear
        {
            get => _toolDiaWear;
            set
            {
                if (_toolDiaWear != value)
                {
                    _toolDiaWear = value;
                    OnPropertyChanged();
                }
            }
        }

        public uint CRC
        {
            get => _crc;
            set
            {
                if (_crc != value)
                {
                    _crc = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsReadOnly => ToolNumber == 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
