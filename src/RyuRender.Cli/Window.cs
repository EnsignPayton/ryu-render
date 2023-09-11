using static SDL2.SDL;

namespace RyuRender.Cli;

public record Window(nint Handle)
{
    public static Window Create(string title, int x, int y, int w, int h, SDL_WindowFlags flags)
    {
        var pWindow = SDL_CreateWindow(title, x, y, w, h, flags);
        if (pWindow == IntPtr.Zero) throw new SDLException();
        return new Window(pWindow);
    }
}

public static class WindowExtensions
{
    public static void Destroy(this Window window) =>
        SDL_DestroyWindow(window.Handle);
}
