using SDL3;
using Engine.Math;

namespace Engine.Core;

public class Input
{
    private HashSet<SDL.Keycode> _currentKeys = new();
    private HashSet<SDL.Keycode> _previousKeys = new();
    private HashSet<SDL.Keycode> _downThisFrame = new();
    private HashSet<SDL.Keycode> _upThisFrame = new();

    private bool _mouseLeft;
    private bool _mouseRight;
    private bool _prevMouseLeft;
    private bool _prevMouseRight;
    private float _mouseX;
    private float _mouseY;

    public void BeginFrame()
    {
        _previousKeys = _currentKeys;
        _currentKeys = new HashSet<SDL.Keycode>(_currentKeys);
        _downThisFrame.Clear();
        _upThisFrame.Clear();

        _prevMouseLeft = _mouseLeft;
        _prevMouseRight = _mouseRight;
    }

    public void ProcessEvent(SDL.Event e)
    {
        var eventType = (SDL.EventType)e.Type;

        if (eventType == SDL.EventType.KeyDown)
        {
            var key = e.Key.Key;
            if (_currentKeys.Add(key))
                _downThisFrame.Add(key);
        }
        else if (eventType == SDL.EventType.KeyUp)
        {
            var key = e.Key.Key;
            _currentKeys.Remove(key);
            _upThisFrame.Add(key);
        }
        else if (eventType == SDL.EventType.MouseMotion)
        {
            _mouseX = e.Motion.X;
            _mouseY = e.Motion.Y;
        }
        else if (eventType == SDL.EventType.MouseButtonUp)
        {
            if (e.Button.Button == SDL.ButtonLeft)
                _mouseLeft = false;
            else if (e.Button.Button == SDL.ButtonRight)
                _mouseRight = false;
        }
        else if (eventType == SDL.EventType.MouseButtonDown)
        {
            if (e.Button.Button == SDL.ButtonLeft)
                _mouseLeft = true;
            else if (e.Button.Button == SDL.ButtonRight)
                _mouseRight = true;
        }
    }

    public bool IsKeyDown(SDL.Keycode key) => _currentKeys.Contains(key);
    public bool IsKeyUp(SDL.Keycode key) => !_currentKeys.Contains(key);
    public bool IsKeyPressed(SDL.Keycode key) => _downThisFrame.Contains(key);
    public bool IsKeyReleased(SDL.Keycode key) => _upThisFrame.Contains(key);

    public bool MouseLeftDown => _mouseLeft;
    public bool MouseRightDown => _mouseRight;
    public bool MouseLeftPressed => _mouseLeft && !_prevMouseLeft;
    public bool MouseRightPressed => _mouseRight && !_prevMouseRight;
    public float MouseX => _mouseX;
    public float MouseY => _mouseY;
    public Vector2 MousePosition => new(_mouseX, _mouseY);
}
