namespace RyuRender.Cli;

public class SDLException : Exception
{
    public SDLException() : base(SDL2.SDL.SDL_GetError())
    {
    }
}
