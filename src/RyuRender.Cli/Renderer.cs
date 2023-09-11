using static SDL2.SDL;

namespace RyuRender.Cli;

public readonly record struct Renderer(nint Handle)
{
    public static Renderer Create(Window window, int index, SDL_RendererFlags flags) =>
        Create(window.Handle, index, flags);

    public static Renderer Create(nint window, int index, SDL_RendererFlags flags)
    {
        var pRenderer = SDL_CreateRenderer(window, index, flags);
        if (pRenderer == IntPtr.Zero) throw new SDLException();
        return new Renderer(pRenderer);
    }
}

public static class RendererExtensions
{
    public static void Destroy(this Renderer renderer) =>
        SDL_DestroyRenderer(renderer.Handle);

    public static int SetDrawColor(this Renderer renderer, byte r, byte g, byte b, byte a) =>
        SDL_SetRenderDrawColor(renderer.Handle, r, g, b, a);

    public static int Clear(this Renderer renderer) =>
        SDL_RenderClear(renderer.Handle);

    public static int DrawLine(this Renderer renderer, int x1, int y1, int x2, int y2) =>
        SDL_RenderDrawLine(renderer.Handle, x1, y1, x2, y2);

    public static int FillRect(this Renderer renderer, ref SDL_Rect rect) =>
        SDL_RenderFillRect(renderer.Handle, ref rect);

    public static void Present(this Renderer renderer) =>
        SDL_RenderPresent(renderer.Handle);
}
