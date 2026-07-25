using SDL3;

namespace Engine.Core;

public class EngineApp
{
    private IntPtr _window;
    private IntPtr _renderer;
    private bool _running;

    public GameTime Time { get; } = new();
    public Input Input { get; } = new();
    public int WindowWidth { get; private set; }
    public int WindowHeight { get; private set; }

    private Action? _onInit;
    private Action<float>? _onUpdate;
    private Action<IntPtr>? _onRender;
    private Action? _onShutdown;

    public EngineApp(string title, int width, int height)
    {
        WindowWidth = width;
        WindowHeight = height;

        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"SDL Init failed: {SDL.GetError()}");

        if (!SDL.CreateWindowAndRenderer(title, width, height, 0, out _window, out _renderer))
            throw new Exception($"Window creation failed: {SDL.GetError()}");
    }

    public void OnInit(Action callback) => _onInit = callback;
    public void OnUpdate(Action<float> callback) => _onUpdate = callback;
    public void OnRender(Action<IntPtr> callback) => _onRender = callback;
    public void OnShutdown(Action callback) => _onShutdown = callback;

    public IntPtr Renderer => _renderer;

    public void Run()
    {
        _onInit?.Invoke();
        Time.Start();
        _running = true;

        while (_running)
        {
            Time.Update();
            Input.BeginFrame();

            while (SDL.PollEvent(out var e))
            {
                if (e.Type == (uint)SDL.EventType.Quit)
                {
                    _running = false;
                    break;
                }

                Input.ProcessEvent(e);
            }

            float dt = Time.DeltaTime;
            if (dt > 0.1f) dt = 0.1f;
            _onUpdate?.Invoke(dt);

            SDL.SetRenderDrawColor(_renderer, 15, 15, 20, 255);
            SDL.RenderClear(_renderer);

            _onRender?.Invoke(_renderer);

            SDL.RenderPresent(_renderer);
        }

        _onShutdown?.Invoke();

        SDL.DestroyRenderer(_renderer);
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }

    public void Quit() => _running = false;
}
