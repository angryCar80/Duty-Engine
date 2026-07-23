using SDL3;
using Engine.Math;

namespace Engine.Rendering;

public class Camera
{
    private int _viewWidth;
    private int _viewHeight;
    public float Zoom { get; set; } = 1f;
    public Vector2 Position { get; set; }

    public Camera(int viewWidth, int viewHeight)
    {
        _viewWidth = viewWidth;
        _viewHeight = viewHeight;
    }

    public float Rotation { get; set; }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        Vector2 offset = worldPos - Position;

        offset *= Zoom;

        return new Vector2(
            offset.X + _viewWidth / 2f,
            offset.Y + _viewHeight / 2f
        );
    }
    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        float offsetX = screenPos.X - _viewWidth / 2f;
        float offsetY = screenPos.Y - _viewHeight / 2f;

        float worldX = offsetX / Zoom;
        float worldY = offsetY / Zoom;

        return new Vector2(
            worldX + Position.X,
            worldY + Position.Y
        );
    }

    public void Follow(Vector2 target, float dt, float smoothSpeed = 5f)
    {
        Position = Vector2.Lerp(Position, target, smoothSpeed * dt);
    }
}
