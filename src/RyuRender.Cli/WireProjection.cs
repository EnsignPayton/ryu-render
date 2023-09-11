using System.Numerics;
using static SDL2.SDL;

namespace RyuRender.Cli;

public sealed class WireProjection : IDisposable
{
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;

    private readonly Vector3[] _cubeVertices =
    {
        new(-1, -1, -1),
        new(-1, -1, 1),
        new(-1, 1, -1),
        new(-1, 1, 1),
        new(1, -1, -1),
        new(1, -1, 1),
        new(1, 1, -1),
        new(1, 1, 1),
    };

    private readonly nint _pWindow;
    private readonly nint _pRenderer;
    private bool _addingZ;

    public WireProjection()
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0) throw new SDLException();

        _pWindow = SDL_CreateWindow("Ryu Render",
            SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
            ScreenWidth, ScreenHeight,
            SDL_WindowFlags.SDL_WINDOW_SHOWN);

        if (_pWindow == IntPtr.Zero) throw new SDLException();

        _pRenderer = SDL_CreateRenderer(_pWindow, -1, SDL_RendererFlags.SDL_RENDERER_SOFTWARE);
        if (_pRenderer == IntPtr.Zero) throw new SDLException();

    }

    public void Dispose()
    {
        if (_pRenderer != IntPtr.Zero)
            SDL_DestroyRenderer(_pRenderer);
        if (_pWindow != IntPtr.Zero)
            SDL_DestroyWindow(_pWindow);

        SDL_Quit();
    }

    public void Start()
    {
        var run = true;
        while (run)
        {
            while (SDL_PollEvent(out var e) != 0)
            {
                if (e.type == SDL_EventType.SDL_QUIT)
                {
                    run = false;
                }
            }

            Update();
            Render();

            Thread.Sleep(100);
        }
    }

    private void Update()
    {
        if (_cubeVertices[0].Z < 0)
            _addingZ = true;
        if (_cubeVertices[0].Z > 10)
            _addingZ = false;

        for (var i = 0; i < _cubeVertices.Length; i++)
        {
            var v = _cubeVertices[i];
            if (_addingZ)
                _cubeVertices[i] = v with { Z = v.Z + 1 };
            else
                _cubeVertices[i] = v with { Z = v.Z - 1 };
        }
    }

    private void Render()
    {
        // Flush to black
        SDL_SetRenderDrawColor(_pRenderer, 0x00, 0x00, 0x00, 0xff);
        SDL_RenderClear(_pRenderer);

        // Draw wireframe
        SDL_SetRenderDrawColor(_pRenderer, 0xff, 0xff, 0xff, 0xff);

        Span<(int, int)> screenVerts = stackalloc (int, int)[8];

        for (var i = 0; i < _cubeVertices.Length; i++)
        {
            var vert = _cubeVertices[i];
            var hAngle = MathF.Atan(vert.X / vert.Z);
            var vAngle = MathF.Atan(vert.Y / vert.Z);

            var hFov = MathF.PI / 4F;
            var vFov = hFov / 16f * 9f;
            var hNorm = (hAngle + hFov) / (hFov * 2f);
            var vNorm = (vAngle + vFov) / (vFov * 2f);
            var hScreen = (int)(hNorm * ScreenWidth);
            var vScreen = (int)(vNorm * ScreenHeight);
            screenVerts[i] = (hScreen, vScreen);
        }

        for (var i = 0; i < screenVerts.Length - 1; i++)
        {
            for (var j = i + 1; j < screenVerts.Length; j++)
            {
                SDL_RenderDrawLine(_pRenderer,
                    screenVerts[i].Item1, screenVerts[i].Item2,
                    screenVerts[j].Item1, screenVerts[j].Item2);
            }
        }

        SDL_RenderPresent(_pRenderer);
    }
}
