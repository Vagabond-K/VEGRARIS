using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Specialized;
using VagabondK.Indicators.DigitalFonts;

namespace Vegraris.Wasm.Pages
{
    partial class Index : IAsyncDisposable
    {
        private IJSObjectReference? jsModule;
        private readonly StateMonitor stateMonitor = new();
        private readonly Game game = new();
        private bool isReady;
        private readonly SevenSegmentFont sevenSegmentFont = new()
        {
            Size = 38,
            SlantAngle = 8
        };

        private ValueTask InvokeAsyncJS(string identifier, params object?[]? args)
            => jsModule?.InvokeVoidAsync(identifier, args: args) ?? ValueTask.CompletedTask;

        protected override async Task OnInitializedAsync()
        {
            game.DropTraces.VacuumAtIdle = true;
            game.LockedTraces.VacuumAtIdle = true;
            game.LineCleared += async (sender, e) => await InvokeAsyncJS("lineClear");
            ((INotifyCollectionChanged)game.LockedTraces).CollectionChanged += async (sender, e) =>
            {
                if (e.NewItems != null)
                    await InvokeAsyncJS("lockDown");
            };

            stateMonitor.Add(game);
            stateMonitor.Add(game.LockedPieces);
            stateMonitor.Add(game.Previews);
            stateMonitor.Add(game.DropTraces);
            stateMonitor.Add(game.LockedTraces);

            jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./Pages/Index.razor.js");
            await InvokeAsyncJS("initialize", DotNetObjectReference.Create(this));
        }

        private static PlayerInput ToPlayerInput(string key)
            => key switch
            {
                "ArrowLeft" => PlayerInput.ShiftLeft,
                "ArrowRight" => PlayerInput.ShiftRight,
                "ArrowUp" => PlayerInput.RotateCW,
                "KeyZ" or "ControlLeft" => PlayerInput.RotateCCW,
                "ArrowDown" => PlayerInput.SoftDrop,
                "Space" or "ShiftRight" => PlayerInput.HardDrop,
                "KeyC" or "ShiftLeft" => PlayerInput.Hold,
                _ => PlayerInput.None
            };

        private async void Play()
        {
            if (!game.Playing)
            {
                isReady = true;
                game.Initialize();
                await InvokeAsyncJS("readyMessage");

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    game.Play();
                    isReady = false;
                    StateHasChanged();
                });
            }
        }

        private void OnHardDrop(bool state) => game.Control(PlayerInput.HardDrop, state);
        private void OnShiftLeft(bool state) => game.Control(PlayerInput.ShiftLeft, state);
        private void OnShiftRight(bool state) => game.Control(PlayerInput.ShiftRight, state);
        private void OnSoftDrop(bool state) => game.Control(PlayerInput.SoftDrop, state);
        private void OnRotateCW(bool state) => game.Control(PlayerInput.RotateCW, state);
        private void OnRotateCCW(bool state) => game.Control(PlayerInput.RotateCCW, state);
        private void OnHold(bool state) => game.Control(PlayerInput.Hold, state);

        [JSInvokable]
        public void UpdateFrame(double timeStamp)
        {
            var renderingTime = TimeSpan.FromMilliseconds(timeStamp);

            stateMonitor.StateHasChanged = false;
            game.Update(renderingTime);
            if (stateMonitor.StateHasChanged)
                StateHasChanged();
            stateMonitor.Vacuum();
        }

        [JSInvokable]
        public void OnKeyDown(KeyboardEventArgs e) => game.Control(ToPlayerInput(e.Code), true);

        [JSInvokable]
        public void OnKeyUp(KeyboardEventArgs e) => game.Control(ToPlayerInput(e.Code), false);

        [JSInvokable]
        public void OnBlur() => game.Control(PlayerInput.None, false);

        public async ValueTask DisposeAsync()
        {
            if (jsModule is not null)
            {
                try
                {
                    await (jsModule?.DisposeAsync() ?? ValueTask.CompletedTask);
                }
                catch { }
            }
            GC.SuppressFinalize(this);
        }
    }
}
