using System.Diagnostics;
using ImGuiNET;
using SDL3;

namespace Engine.Editor;

public class Editor : IDisposable
{
    nint _window, _renderer;
    ImGuiSDL3? _platform;
    ImGuiSDL3Renderer? _backend;
    Stopwatch _timer = Stopwatch.StartNew();
    TimeSpan _lastTime;
    bool _running;

    public Editor()
    {
        if (!SDL.Init(SDL.InitFlags.Video))
            throw new Exception($"SDL_Init failed: {SDL.GetError()}");

        if (!SDL.CreateWindowAndRenderer("Tilemap Editor", 1280, 720, SDL.WindowFlags.Resizable,
             out _window, out _renderer))
            throw new Exception($"Window failed: {SDL.GetError()}");

        SDL.SetRenderVSync(_renderer, 1);

        ImGui.SetCurrentContext(ImGui.CreateContext());
        _platform = new ImGuiSDL3(_window, _renderer);
        _backend = new ImGuiSDL3Renderer(_renderer);
    }

    public void Run()
    {
        _running = true;
        while (_running)
        {
            float dt = (float)(_timer.Elapsed - _lastTime).TotalSeconds;
            _lastTime = _timer.Elapsed;
            if (dt > 0.1f) dt = 0.1f;

            // Poll
            if (ImGui.GetIO().WantTextInput && !SDL.TextInputActive(_window))
                SDL.StartTextInput(_window);
            else if (!ImGui.GetIO().WantTextInput && SDL.TextInputActive(_window))
                SDL.StopTextInput(_window);

            while (SDL.PollEvent(out var ev))
            {
                _platform!.ProcessEvent(ev);
                if ((SDL.EventType)ev.Type is SDL.EventType.Quit or SDL.EventType.WindowCloseRequested)
                    _running = false;
            }

            // ImGui frame
            _platform.NewFrame();
            _backend.NewFrame();
            ImGui.NewFrame();

            // Your UI here
            ImGui.ShowDemoWindow();

            // Render
            ImGui.EndFrame();
            SDL.SetRenderDrawColor(_renderer, 25, 25, 30, 255);
            SDL.RenderClear(_renderer);
            ImGui.Render();
            _backend.RenderDrawData(ImGui.GetDrawData());
            SDL.RenderPresent(_renderer);
        }
    }

    public void Dispose()
    {
        _backend?.Dispose();
        ImGui.DestroyContext();
        SDL.DestroyRenderer(_renderer);
        SDL.DestroyWindow(_window);
        SDL.Quit();
    }
}
