using System;

namespace Vegraris
{
    class PlayerInputDispatcher
    {
        public PlayerInputDispatcher(Action activated, Action dectivated = null)
        {
            this.activated = activated;
            this.dectivated = dectivated;
        }

        private readonly Action activated;
        private readonly Action dectivated;
        private bool isActivated;

        public bool IsActivated
        {
            get => isActivated;
            set
            {
                if (isActivated != value)
                {
                    isActivated = value;
                    if (isActivated)
                        OnActivated();
                    else
                        OnDectivated();
                }
            }
        }

        protected virtual void OnActivated() => activated?.Invoke();
        protected virtual void OnDectivated() => dectivated?.Invoke();
    }
}