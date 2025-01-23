using System;
using System.Collections.Generic;
using Vegraris.Tracing;

namespace Vegraris
{
    public class LockedPiece : Piece, ITrace
    {
        internal LockedPiece(in Tetromino tetromino) : base(tetromino)
        {
        }

        private bool clearing;
        private bool lockedEffect;
        private TimeSpan timeStamp;

        public bool Clearing { get => clearing; internal set => SetProperty(ref clearing, value); }
        public bool LockedEffect { get => lockedEffect; private set => SetProperty(ref lockedEffect, value); }
        TimeSpan ITrace.TimeStamp => timeStamp;

        internal void UpdatePath(IEnumerable<IEnumerable<PiecePoint>> path) => Path = path;
        internal void BeginLockedTrace(TimeSpan timeStamp)
        {
            if (!clearing)
            {
                this.timeStamp = timeStamp;
                LockedEffect = true;
            }
        }

        void ITrace.Complete() => LockedEffect = false;
    }
}