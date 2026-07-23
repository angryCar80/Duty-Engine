using System.Diagnostics;

namespace Engine.Core;

public class GameTime
{
    private readonly Stopwatch _stopwatch = new();
    private long _lastTicks;

    public float DeltaTime { get; private set; }
    public float TotalTime { get; private set; }
    public int FrameCount { get; private set; }
    public float FPS { get; private set; }

    private float _fpsTimer;
    private int _fpsFrames;

    public void Start()
    {
        _stopwatch.Start();
        _lastTicks = _stopwatch.ElapsedTicks;
    }

    public void Update()
    {
        long currentTicks = _stopwatch.ElapsedTicks;
        DeltaTime = (float)(currentTicks - _lastTicks) / Stopwatch.Frequency;
        _lastTicks = currentTicks;
        TotalTime += DeltaTime;
        FrameCount++;

        _fpsFrames++;
        _fpsTimer += DeltaTime;
        if (_fpsTimer >= 1.0f)
        {
            FPS = _fpsFrames / _fpsTimer;
            _fpsFrames = 0;
            _fpsTimer = 0;
        }
    }
}
