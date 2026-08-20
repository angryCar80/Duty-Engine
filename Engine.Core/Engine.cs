using SDL3;

namespace Engine.Core;

public class EngineApp
{
    private IntPtr _window;
    private IntPtr _renderer;
    private bool _running;

    // 3D Rendering
    private bool _useOpenGL;
    private IntPtr _glContext;

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

    public EngineApp(string title, int width, int height, bool useOpenGL)
    {
        _useOpenGL = useOpenGL;
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"SDL Init Faild: {SDL.GetError()}");

        if (useOpenGL)
        {
            SDL.GLMakeCurrent(_window, _glContext);
            SDL.GLSetAttribute(SDL.GLAttr.DoubleBuffer, 1);
            SDL.GLSetAttribute(SDL.GLAttr.DepthSize, 24);
            SDL.GLSetAttribute(SDL.GLAttr.ContextMajorVersion, 4);
            SDL.GLSetAttribute(SDL.GLAttr.ContextMinorVersion, 1);
            SDL.GLSetAttribute(SDL.GLAttr.ContextProfileMask, (int)SDL.GLProfile.Core);

            _window = SDL.CreateWindow(title, width, height, SDL.WindowFlags.OpenGL);
            if (_window == IntPtr.Zero)
                throw new Exception($"Window Creation Faild: {SDL.GetError()}");

            _glContext = SDL.GLCreateContext(_window);
            if (_glContext == IntPtr.Zero)
                throw new Exception($"GL Context faild: {SDL.GetError()}");

            SDL.GLSetSwapInterval(1);
        }
        else
        {
            if (!SDL.CreateWindowAndRenderer(title, width, height, 0, out _window, out _renderer))
                throw new Exception($"Window Creation Failed: {SDL.GetError()}");
        }
    }

    public IntPtr GlContext => _glContext;
    public IntPtr GetGLProcAddress(string proc) => SDL.GLGetProcAddress(proc);

    public void OnInit(Action callback) => _onInit = callback;
    public void OnUpdate(Action<float> callback) => _onUpdate = callback;
    public void OnRender(Action<IntPtr> callback) => _onRender = callback;
    public void OnShutdown(Action callback) => _onShutdown = callback;

    public IntPtr Renderer => _renderer;

    public void Run()
    {
        if (_useOpenGL)
            SDL.GLMakeCurrent(_window, _glContext);

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

                if (e.Type == (uint)SDL.EventType.WindowResized
                    || e.Type == (uint)SDL.EventType.WindowPixelSizeChanged)
                {
                    SDL.GetWindowSizeInPixels(_window, out int w, out int h);
                    WindowWidth = w;
                    WindowHeight = h;
                }

                Input.ProcessEvent(e);
            }

            float dt = Time.DeltaTime;
            if (dt > 0.1f) dt = 0.1f;
            _onUpdate?.Invoke(dt);

            if (_useOpenGL)
            {
                SDL.GLMakeCurrent(_window, _glContext);
                _onRender?.Invoke(_window);
                SDL.GLSwapWindow(_window);
            }
            else
            {
                SDL.SetRenderDrawColor(_renderer, 15, 15, 20, 255);
                SDL.RenderClear(_renderer);
                _onRender?.Invoke(_renderer);
                SDL.RenderPresent(_renderer);
            }
        }

        _onShutdown?.Invoke();


        if (_useOpenGL)
        {
            SDL.GLDestroyContext(_glContext);
            SDL.DestroyWindow(_window);
        }

        else
        {
            SDL.DestroyRenderer(_renderer);
            SDL.DestroyWindow(_window);
        }
        SDL.Quit();
    }

    public void Quit() => _running = false;
}
