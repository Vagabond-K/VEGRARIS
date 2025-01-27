using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Vegraris.Wasm.Components
{
    partial class GameButton : IAsyncDisposable
    {
        private IJSObjectReference? jsModule;
        private ElementReference elementReference;

        [Parameter]
        public RenderFragment? Content { get; set; }

        [Parameter]
        public string? Style { get; set; }

        [Parameter]
        public EventCallback<bool> GameButtonStateChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./Components/GameButton.razor.js");
        }

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

        private async Task OnPointerEvent(PointerEventArgs e, bool state)
        {
            await (jsModule?.InvokeVoidAsync("setState", elementReference, e.PointerId, state) ?? ValueTask.CompletedTask);
            await GameButtonStateChanged.InvokeAsync(state);
        }

        private Task OnPointerDown(PointerEventArgs e) => OnPointerEvent(e, true);
        private Task OnPointerUp(PointerEventArgs e) => OnPointerEvent(e, false);
        private Task OnLostCapture(PointerEventArgs e) => OnPointerEvent(e, false);
    }
}
