namespace FrinkyEngine.Core.UI.Internal;

internal interface IUiBackend : IDisposable
{
    UiInputCapture InputCapture { get; }

    void PrepareFrame(float dt, in UiFrameDesc frameDesc);

    void RenderFrame(IReadOnlyList<Action<UiContext>> drawCommands, UiContext context);

    void ClearFrame();
}

