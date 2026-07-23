using SDL3;
using Engine.Math;

namespace Engine.Rendering;

public class SpriteRenderer
{
    private IntPtr _renderer;
    private Camera? _camera;

    public SpriteRenderer(IntPtr renderer)
    {
        _renderer = renderer;
    }

    public void Begin(Camera camera)
    {
        _camera = camera;
    }
    public void Draw(Texture2D texture, Vector2 position)
    {
        Vector2 screenPos = _camera!.WorldToScreen(position);

        var src = new SDL.FRect { X = 0, Y = 0, W = texture.Width, H = texture.Height };

        var dst = new SDL.FRect
        {
            X = screenPos.X,
            Y = screenPos.Y,
            W = texture.Width,
            H = texture.Height
        };

        SDL.RenderTexture(_renderer, texture.Handle, in src, in dst);
    }
    public void Draw(Texture2D texture, Vector2 position, Vector2 scale, float rotation)
    {
        Vector2 screenPos = _camera!.WorldToScreen(position);

        var src = new SDL.FRect { X = 0, Y = 0, W = texture.Width, H = texture.Height };

        var dst = new SDL.FRect { X = screenPos.X, Y = screenPos.Y, W = texture.Width * scale.X, H = texture.Height * scale.Y };

        SDL.RenderTextureRotated(
            _renderer,
            texture.Handle,
            src,
            dst,
            rotation,
            IntPtr.Zero,
            SDL.FlipMode.None
        );
    }

    public void Draw(Texture2D texture, Vector2 position, Color color)
    {
        texture.SetColorMod(color.R, color.G, color.B);
        texture.SetAlphaMod(color.A);
        Draw(texture, position);

        texture.SetColorMod(255, 255, 255);
        texture.SetAlphaMod(255);
    }

    public void End() {
        _camera = null;
    }
}
