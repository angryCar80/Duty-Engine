using Engine.Core;
using Engine.Rendering3D;
using System;

namespace Demo3D;

public static class Program
{
    public static void Main()
    {
        var engine = new EngineApp("Demo3D", 960, 540, useOpenGL: true);

        Renderer3d? renderer = null;
        float angle = 0f;

        engine.OnInit(() =>
            renderer = new Renderer3d(engine, engine.WindowWidth, engine.WindowHeight));

        engine.OnUpdate(dt => angle += dt * 1.2f);

        engine.OnRender(_ =>
        {
            renderer!.SetViewport(engine.WindowWidth, engine.WindowHeight);
            renderer.Clear(0.09f, 0.10f, 0.13f);
            renderer.DrawTriangle(angle);
        });

        engine.Run();
    }
}
