using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Vegraris.Tracing;

namespace Vegraris
{
    public class Game : INotifyPropertyChanged
    {
        public Game()
        {
            grid = new Grid(10, 20);
            randomBag = new RandomBag();
            clearingLines = new Queue<int>();
            timerManager = new TimerManager();
            fallingTimer = timerManager.CreateTimer(() => Move(0, 1), 1000, 1000);
            shiftingTimer = timerManager.CreateTimer(() => Move(shifting, 0), 250, 30);
            softDropTimer = timerManager.CreateTimer(() => SoftDrop(), 0, 50);
            lockDownTimer = timerManager.CreateTimer(LockDown, 500, 0);
            clearingTimer = timerManager.CreateTimer(ClearLine, 250, 250);
            inputDispatchers = new Dictionary<PlayerInput, PlayerInputDispatcher>
            {
                { PlayerInput.ShiftLeft, new PlayerInputDispatcher(() => SetShifting(-1), () => SetShifting(inputDispatchers[PlayerInput.ShiftRight].IsActivated ? 1 : 0)) },
                { PlayerInput.ShiftRight, new PlayerInputDispatcher(() => SetShifting(1), () => SetShifting(inputDispatchers[PlayerInput.ShiftLeft].IsActivated ? -1 : 0)) },
                { PlayerInput.SoftDrop, new PlayerInputDispatcher(softDropTimer.Start, softDropTimer.Stop) },
                { PlayerInput.RotateCW, new PlayerInputDispatcher(() => Rotate(true)) },
                { PlayerInput.RotateCCW, new PlayerInputDispatcher(() => Rotate(false)) },
                { PlayerInput.HardDrop, new PlayerInputDispatcher(HardDrop) },
                { PlayerInput.Hold, new PlayerInputDispatcher(Hold) },
            };
            previews = new ObservableCollection<StaticPiece>();
            dropTraces = new TraceCollection<DropTrace>();
            commandQueue = new ConcurrentQueue<Action>();
        }

        private static readonly DefaultScoring defaultScoring = new DefaultScoring();

        private readonly Grid grid;
        private readonly RandomBag randomBag;
        private readonly Queue<int> clearingLines;
        private readonly TimerManager timerManager;
        private readonly Timer fallingTimer;
        private readonly Timer lockDownTimer;
        private readonly Timer clearingTimer;
        private readonly Timer shiftingTimer;
        private readonly Timer softDropTimer;
        private readonly Dictionary<PlayerInput, PlayerInputDispatcher> inputDispatchers;
        private readonly ObservableCollection<StaticPiece> previews;
        private readonly TraceCollection<DropTrace> dropTraces;
        private readonly ConcurrentQueue<Action> commandQueue;

        private IScoring scoring;
        private ActivePiece activePiece;
        private StaticPiece holdingPiece;
        private int shifting;
        private int lockDownCount;
        private bool standby;
        private bool playing;
        private bool paused;
        private bool usingHoldingPiece;
        private bool isBackToBack;
        private int score;
        private int lines;
        private int level;
        private int combo = -1;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<LineClearEventArgs> LineClearing;
        public event EventHandler<LineClearEventArgs> LineCleared;

        public IScoring Scoring { get => scoring; private set => SetProperty(ref scoring, value); }

        public int Columns => grid.Columns;
        public int Rows => grid.Rows;
        public IEnumerable<StaticPiece> Previews => previews;
        public IEnumerable<LockedPiece> LockedPieces => grid.Pieces;
        public ITraceCollection<LockedPiece> LockedTraces => grid.LockedTraces;
        public ITraceCollection<DropTrace> DropTraces => dropTraces;
        public ActivePiece ActivePiece { get => activePiece; private set => SetProperty(ref activePiece, value); }
        public StaticPiece HoldingPiece { get => holdingPiece; private set => SetProperty(ref holdingPiece, value); }
        public bool Playing { get => playing; private set => SetProperty(ref playing, value); }
        public bool Standby { get => standby; private set => SetProperty(ref standby, value); }
        public bool Paused { get => paused; private set => SetProperty(ref paused, value); }
        public int Score { get => score; private set => SetProperty(ref score, value); }
        public int Lines
        {
            get => lines;
            private set
            {
                if (SetProperty(ref lines, value))
                    Level = lines / 10 + 1;
            }
        }
        public int Level
        {
            get => level;
            private set
            {
                if (SetProperty(ref level, value))
                {
                    var level = Math.Max(this.level, 1);
                    var falling = Math.Pow(0.8 - (level - 1) * 0.007, level - 1);
                    fallingTimer.Period = fallingTimer.DueTime = TimeSpan.FromSeconds(falling);
                    softDropTimer.Period = TimeSpan.FromSeconds(falling / 20);
                }
            }
        }

        private bool SetProperty<T>(ref T target, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(target, value))
            {
                target = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        private void PickNextPiece()
        {
            ActivePiece = ActivePiece.Create(previews[0].Tetromino, grid);

            for (int i = 0; i < previews.Count - 1; i++)
                previews[i] = previews[i + 1];

            previews[previews.Count - 1] = ActivePiece.GetPreview(randomBag.Next());
            usingHoldingPiece = false;

            if (!activePiece.ValidateLocation(0, 0, 0) && !activePiece.Move(0, -1))
            {
                activePiece.LockingDown = false;
                activePiece.LockDown(timerManager.TimeStamp);
                ActivePiece = null;
                Playing = false;
                return;
            }

            CheckLockDown();
        }

        private void ClearLine()
        {
            if (!playing) return;
            if (clearingLines.Count == 0) return;

            var clearingLine = clearingLines.Dequeue();
            LineCleared?.Invoke(this, new LineClearEventArgs(clearingLine));
            grid.ShiftDown(clearingLine);
            if (clearingLines.Count > 0)
            {
                clearingLine = clearingLines.Peek();
                grid.BreakLine(clearingLine);
                LineClearing?.Invoke(this, new LineClearEventArgs(clearingLine));
            }
            else
            {
                clearingTimer.Stop();
                fallingTimer.Start();
                PickNextPiece();
            }
        }

        private void CheckLockDown()
        {
            if (lockDownTimer.IsRunning)
                lockDownCount++;
            if (!activePiece.ValidateLocation(0, 1, 0))
            {
                if (lockDownCount < 16)
                {
                    lockDownTimer.Start();
                    activePiece.LockingDown = false;
                    activePiece.LockingDown = true;
                }
                else
                    LockDown();
            }
            else
                activePiece.LockingDown = false;
        }

        private void LockDown()
        {
            if (!playing || activePiece == null || activePiece.ValidateLocation(0, 1, 0)) return;

            lockDownCount = 0;
            activePiece.LockDown(timerManager.TimeStamp);
            activePiece.LockingDown = false;

            var maxLine = activePiece.Path.Max(paths => paths.Max(point => point.CellY));
            var minLine = activePiece.Path.Min(paths => paths.Min(point => point.CellY));

            for (int line = minLine; line <= maxLine; line++)
                if (grid.IsFullLine(line))
                    clearingLines.Enqueue(line);

            var tSpin = activePiece.TSpin;
            var clearingLinesCount = clearingLines.Count;

            if (clearingLinesCount > 0)
            {
                ActivePiece = null;
                var clearingLine = clearingLines.Peek();
                grid.BreakLine(clearingLine);
                LineClearing?.Invoke(this, new LineClearEventArgs(clearingLine));
                clearingTimer.Start();
                Lines += clearingLinesCount;
                combo++;
            }
            else
            {
                combo = -1;
                fallingTimer.Start();
                PickNextPiece();
            }

            Score += (scoring ?? defaultScoring).LineClear(level, clearingLinesCount, tSpin, isBackToBack, Math.Max(0, combo));

            isBackToBack = clearingLinesCount >= 4 || tSpin != TSpin.None;
        }

        private void SetShifting(int shifting)
        {
            if (this.shifting != shifting)
            {
                this.shifting = shifting;
                if (shifting != 0)
                {
                    Move(shifting, 0);
                    shiftingTimer.Start();
                }
                else
                    shiftingTimer.Stop();
            }
        }

        private bool Move(int offsetX, int offsetY)
        {
            if (!playing || paused || activePiece == null) return false;

            if (activePiece.Move(offsetX, offsetY))
            {
                CheckLockDown();
                return true;
            }
            return false;
        }

        private void Rotate(bool cw)
        {
            if (!playing || paused || activePiece == null) return;

            if (activePiece.Rotate(cw))
                CheckLockDown();
        }

        private void SoftDrop()
        {
            if (Move(0, 1))
                Score += (scoring ?? defaultScoring).SoftDrop(level);
        }

        private void HardDrop()
        {
            if (!playing || paused || activePiece == null) return;

            var offsetY = 1;
            while (activePiece.ValidateLocation(0, offsetY, 0))
                offsetY++;
            offsetY--;
            if (offsetY > 0)
            {
                var dropTrace = new DropTrace(activePiece, offsetY, timerManager.TimeStamp);
                activePiece.Move(0, offsetY);
                Score += (scoring ?? defaultScoring).HardDrop(level, offsetY);
                dropTraces.Add(dropTrace);
            }
            LockDown();
        }

        private void Hold()
        {
            if (!playing || paused || activePiece == null) return;

            if (holdingPiece == null)
            {
                HoldingPiece = ActivePiece.GetPreview(activePiece.Tetromino);
                PickNextPiece();
                usingHoldingPiece = true;
            }
            else if (!usingHoldingPiece)
            {
                var tetromino = holdingPiece.Tetromino;
                HoldingPiece = ActivePiece.GetPreview(activePiece.Tetromino);
                ActivePiece = ActivePiece.Create(tetromino, grid);
                usingHoldingPiece = true;
            }
        }

        public void Initialize() => commandQueue.Enqueue(() =>
        {
            if (playing) return;

            grid.Clear();
            previews.Clear();
            ActivePiece = null;
            HoldingPiece = null;
            Paused = false;
            Lines = 0;
            Level = 0;
            Score = 0;
        });

        public void Play() => commandQueue.Enqueue(() =>
        {
            if (playing) return;

            Initialize();
            Level = 1;
            while (previews.Count < 3)
                previews.Add(ActivePiece.GetPreview(randomBag.Next()));
            PickNextPiece();
            fallingTimer.Start();
            Playing = true;
        });

        public void Quit() => commandQueue.Enqueue(() =>
        {
            if (!playing) return;
            fallingTimer.Stop();
            Playing = false;
            Initialize();
        });

        public void Pause() => commandQueue.Enqueue(() =>
        {
            if (!playing) return;

            fallingTimer.Stop();
            Paused = true;
        });

        public void Resume() => commandQueue.Enqueue(() =>
        {
            if (!playing) return;

            fallingTimer.Start();
            Paused = false;
        });

        public bool Control(PlayerInput input, bool isActivated)
        {
            if (playing && !paused || !isActivated)
            {
                if (input == PlayerInput.None && !isActivated)
                    commandQueue.Enqueue(() =>
                    {
                        foreach (var dispatcher in inputDispatchers.Values)
                            dispatcher.IsActivated = false;
                    });
                else if (inputDispatchers.TryGetValue(input, out var dispatcher))
                    commandQueue.Enqueue(() => dispatcher.IsActivated = isActivated);
                return true;
            }
            return false;
        }

        public void Update(in TimeSpan renderingTime)
        {
            while (commandQueue.TryDequeue(out var action))
                action.Invoke();

            dropTraces.Vacuum(renderingTime);
            grid.VacuumLockedTraces(renderingTime);
            timerManager.UpdateTimers(renderingTime);
        }
    }
}