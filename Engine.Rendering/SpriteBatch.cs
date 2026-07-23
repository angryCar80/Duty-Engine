using SDL3;
using Engine.Math;

namespace Engine.Rendering;

public class SpriteBatch
{
    private IntPtr _renderer;
    private Camera? _camera;
    private List<DrawCall> _drawCalls = new();

    private struct DrawCall
    {
        public Texture2D Texture;
        public Vector2 Position;
    };

    public SpriteBatch(IntPtr renderer)
    {
        _renderer = renderer;
    }
    public void Begin(Camera camera)
    {
        _camera = camera;
        _drawCalls.Clear();
    }
    public void Draw(Texture2D texture, Vector2 position)
    {
        _drawCalls.Add(new DrawCall
        {
            Texture = texture,
            Position = position,
        });
    }
    public void End()
    {
        _drawCalls.Sort((a, b) =>
                        a.Texture.Handle.CompareTo(b.Texture.Handle));
        foreach (var call in _drawCalls){
            Vector2 screenPos = _camera!.WorldToScreen(call.Position);

            var src = new SDL.FRect
            {
                X = 0,
                Y = 0,
                W = call.Texture.Width,
                H = call.Texture.Height
            };

            var dst = new SDL.FRect
            {
                X = screenPos.X,
                Y = screenPos.Y,
                W = call.Texture.Width,
                H = call.Texture.Height
            };

            SDL.RenderTexture(_renderer, call.Texture.Handle, in src, in dst);        }
        _drawCalls.Clear();
        _camera = null;
    }
}
