using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vegraris
{
    public abstract class Piece : INotifyPropertyChanged
    {
        protected Piece(in Tetromino tetromino)
        {
            Tetromino = tetromino;
        }

        private IEnumerable<IEnumerable<PiecePoint>> path;
        private object tag;

        protected bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(target, value))
            {
                target = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Tetromino Tetromino { get; }
        public IEnumerable<IEnumerable<PiecePoint>> Path { get => path; protected set => SetProperty(ref path, value); }
        public object Tag { get => tag; set => SetProperty(ref tag, value); }
    }
}