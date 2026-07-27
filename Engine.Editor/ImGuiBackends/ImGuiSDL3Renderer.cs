using System.Numerics;
using ImGuiNET;
using SDL3;

namespace Engine.Editor;

/// <summary>
/// SDL3 renderer backend for ImGui.
/// Adapted from behindcurtain3/SDL3-ImGui for SDL3-CS 3.4.x.
/// </summary>
public class ImGuiSDL3Renderer : IDisposable
{
    public readonly nint Renderer;
    private nint _fontTexture = IntPtr.Zero;

    public ImGuiSDL3Renderer(nint renderer)
    {
        Renderer = renderer;
        ImGui.GetIO().BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
    }

    public void Dispose()
    {
        DestroyDeviceObjects();
    }

    public void NewFrame()
    {
        if (_fontTexture == IntPtr.Zero)
            CreateDeviceObjects();
    }

    public void RenderDrawData(ImDrawDataPtr drawData)
    {
        unsafe
        {
            if (drawData.NativePtr == null || drawData.CmdListsCount == 0)
                return;
        }

        Vector2 renderScale = new(drawData.FramebufferScale.X, drawData.FramebufferScale.Y);
        int fbWidth = (int)(drawData.DisplaySize.X * renderScale.X);
        int fbHeight = (int)(drawData.DisplaySize.Y * renderScale.Y);
        if (fbWidth <= 0 || fbHeight <= 0) return;

        // Backup renderer state
        SDL.GetRenderViewport(Renderer, out SDL.Rect oldViewport);
        bool hadClip = SDL.RenderClipEnabled(Renderer);
        SDL.GetRenderClipRect(Renderer, out SDL.Rect oldClip);

        // Setup for ImGui
        SDL.SetRenderViewport(Renderer, 0);
        SDL.SetRenderClipRect(Renderer, IntPtr.Zero);
        SDL.SetRenderDrawBlendMode(Renderer, SDL.BlendMode.Blend);

        Vector2 clipOffset = drawData.DisplayPos;

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmdList = drawData.CmdLists[n];

            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex++)
            {
                ImDrawCmdPtr cmd = cmdList.CmdBuffer[cmdIndex];
                if (cmd.UserCallback != IntPtr.Zero) continue;

                // Clipping rectangle
                Vector4 clipRect = cmd.ClipRect;
                Vector2 clipMin = new(
                    (clipRect.X - clipOffset.X) * renderScale.X,
                    (clipRect.Y - clipOffset.Y) * renderScale.Y);
                Vector2 clipMax = new(
                    (clipRect.Z - clipOffset.X) * renderScale.X,
                    (clipRect.W - clipOffset.Y) * renderScale.Y);
                clipMin.X = System.Math.Max(0, clipMin.X);
                clipMin.Y = System.Math.Max(0, clipMin.Y);
                clipMax.X = System.Math.Min(fbWidth, clipMax.X);
                clipMax.Y = System.Math.Min(fbHeight, clipMax.Y);

                SDL.Rect r = new()
                {
                    X = (int)clipMin.X, Y = (int)clipMin.Y,
                    W = (int)(clipMax.X - clipMin.X), H = (int)(clipMax.Y - clipMin.Y)
                };
                SDL.SetRenderClipRect(Renderer, r);

                // Render the draw command
                RenderDrawCommand(cmdList, cmd, renderScale);
            }
        }

        // Restore renderer state
        SDL.SetRenderViewport(Renderer, oldViewport);
        if (hadClip)
            SDL.SetRenderClipRect(Renderer, oldClip);
        else
            SDL.SetRenderClipRect(Renderer, IntPtr.Zero);
    }

    private unsafe void RenderDrawCommand(ImDrawListPtr drawList, ImDrawCmdPtr cmd, Vector2 scale)
    {
        int indexOffset = (int)cmd.IdxOffset;
        int vertexOffset = (int)cmd.VtxOffset;
        int elemCount = (int)cmd.ElemCount;

        // Find vertex range used by this command
        ushort minVertexIdx = ushort.MaxValue;
        ushort maxVertexIdx = 0;
        for (int i = 0; i < elemCount; i++)
        {
            ushort idx = drawList.IdxBuffer[indexOffset + i];
            minVertexIdx = (ushort)System.Math.Min(minVertexIdx, idx);
            maxVertexIdx = (ushort)System.Math.Max(maxVertexIdx, idx);
        }
        minVertexIdx = (ushort)(minVertexIdx + vertexOffset);
        maxVertexIdx = (ushort)(maxVertexIdx + vertexOffset);
        int numVertices = maxVertexIdx - minVertexIdx + 1;

        // Convert ImGui vertices to SDL vertices
        SDL.Vertex[] vertices = new SDL.Vertex[numVertices];
        for (int i = 0; i < numVertices; i++)
        {
            ImDrawVertPtr srcVert = drawList.VtxBuffer[minVertexIdx + i];
            uint col = srcVert.col;
            vertices[i] = new SDL.Vertex
            {
                Position = new SDL.FPoint { X = srcVert.pos.X, Y = srcVert.pos.Y },
                Color = new SDL.FColor
                {
                    R = ((col >> 0) & 0xFF) / 255f,
                    G = ((col >> 8) & 0xFF) / 255f,
                    B = ((col >> 16) & 0xFF) / 255f,
                    A = ((col >> 24) & 0xFF) / 255f
                },
                TexCoord = new SDL.FPoint { X = srcVert.uv.X, Y = srcVert.uv.Y }
            };
        }

        // Adjust indices relative to our vertex array
        int[] indices = new int[elemCount];
        for (int i = 0; i < elemCount; i++)
        {
            ushort originalIdx = drawList.IdxBuffer[indexOffset + i];
            indices[i] = (ushort)(originalIdx - (minVertexIdx - vertexOffset));
        }

        nint texId = cmd.GetTexID();
        SDL.RenderGeometry(Renderer, texId, vertices, numVertices, indices, elemCount);
    }

    private unsafe bool CreateDeviceObjects()
    {
        ImGuiIOPtr io = ImGui.GetIO();
        io.Fonts.AddFontFromFileTTF("/usr/share/fonts/TTF/JetBrainsMono-Regular.ttf", 18f);
        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

        var surface = SDL.CreateSurfaceFrom(width, height, SDL.PixelFormat.RGBA8888, (nint)pixels, width * 4);
        if (surface == IntPtr.Zero) return false;

        _fontTexture = SDL.CreateTextureFromSurface(Renderer, surface);
        SDL.DestroySurface(surface);
        if (_fontTexture == IntPtr.Zero) return false;

        SDL.UpdateTexture(_fontTexture, IntPtr.Zero, (nint)pixels, width * 4);
        SDL.SetTextureBlendMode(_fontTexture, SDL.BlendMode.Blend);
        SDL.SetTextureScaleMode(_fontTexture, SDL.ScaleMode.Linear);

        io.Fonts.SetTexID(_fontTexture);
        io.Fonts.ClearTexData();

        return true;
    }

    private void DestroyDeviceObjects()
    {
        if (_fontTexture != IntPtr.Zero)
        {
            ImGui.GetIO().Fonts.SetTexID(IntPtr.Zero);
            SDL.DestroyTexture(_fontTexture);
            _fontTexture = IntPtr.Zero;
        }
    }
}
