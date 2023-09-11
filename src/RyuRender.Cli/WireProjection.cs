using System.Numerics;
using static SDL2.SDL;

namespace RyuRender.Cli;

public sealed class WireProjection : IDisposable
{
    private const int ScreenWidth = 640;
    private const int ScreenHeight = 640;
    private const int FocalLength = 4;

    private readonly Vector3 _camPos = new(0, 0, -16);
    private readonly Vector3 _camRot = Vector3.Zero;

    private readonly Vector3[] _cubePoints =
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

    private float _cubeRot;

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
        _cubeRot += MathF.PI / 16f;
        if (_cubeRot > 2f * MathF.PI)
            _cubeRot -= 2f * MathF.PI;
    }

    private void Render()
    {
        // Flush to black
        SDL_SetRenderDrawColor(_pRenderer, 0x00, 0x00, 0x00, 0xff);
        SDL_RenderClear(_pRenderer);

        // Draw wireframe
        SDL_SetRenderDrawColor(_pRenderer, 0xff, 0xff, 0xff, 0xff);

        Span<Point2D> screenPoints = stackalloc Point2D[8];

        for (var i = 0; i < _cubePoints.Length; i++)
        {
            var worldPoint = _cubePoints[i];
            var rotated1 = RotateY(worldPoint, _cubeRot);
            var rotated2 = RotateX(rotated1, _cubeRot);
            var cameraPoint = WorldToCamera(rotated2);
            var focalPoint = CameraToFocalPlane(cameraPoint);
            var screenPoint = FocalToScreen(focalPoint);
            screenPoints[i] = screenPoint;
        }

        for (var i = 0; i < screenPoints.Length - 1; i++)
        {
            for (var j = i + 1; j < screenPoints.Length; j++)
            {
                SDL_RenderDrawLine(_pRenderer,
                    screenPoints[i].X, screenPoints[i].Y,
                    screenPoints[j].X, screenPoints[j].Y);
            }
        }

        SDL_RenderPresent(_pRenderer);
    }

    private static Vector3 RotateX(Vector3 point, float angle) => new(
        MathF.Cos(angle) * point.X - MathF.Sin(angle) * point.Z,
        point.Y,
        MathF.Sin(angle) * point.X + MathF.Cos(angle) * point.Z);

    private static Vector3 RotateY(Vector3 point, float angle) => new(
        point.X,
        MathF.Cos(angle) * point.Y - MathF.Sin(angle) * point.Z,
        MathF.Sin(angle) * point.Y + MathF.Cos(angle) * point.Z);

    private Vector3 WorldToCamera(Vector3 point)
    {
        // Translate from world to camera
        var pointWrtCamera = point - _camPos;

        // TODO: Rotate from world to camera
        return pointWrtCamera;
    }

    private static Vector2 CameraToFocalPlane(Vector3 point)
    {
        if (point.Z < FocalLength)
            return Vector2.Zero;

        var u = FocalLength * point.X / point.Z;
        var v = FocalLength * point.Y / point.Z;
        return new Vector2(u, v);
    }

    private static Point2D FocalToScreen(Vector2 point)
    {
        var x = (int)(point.X * ScreenWidth) + (ScreenWidth / 2);
        var y = (int)(point.Y * ScreenHeight) + (ScreenHeight / 2);
        return new Point2D(x, y);
    }

    private readonly record struct Point2D(int X, int Y);
}
