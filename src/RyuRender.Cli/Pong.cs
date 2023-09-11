using static SDL2.SDL;

namespace RyuRender.Cli;

public sealed class Pong : IDisposable
{
    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;
    private const int BallSize = 32;
    private const int PaddleWidth = 32;
    private const int PaddleHeight = 128;
    private const int PaddleOffset = 64;

    private readonly nint _pWindow;
    private readonly nint _pRenderer;
    private SDL_Rect _ball;
    private SDL_Rect _paddle1;
    private SDL_Rect _paddle2;
    private int _dx = 1;
    private int _dy = 1;
    private bool _wPressed;
    private bool _sPressed;
    private bool _upPressed;
    private bool _downPressed;
    private int _p1Score;
    private int _p2Score;

    public Pong()
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0) throw new SDLException();

        _pWindow = SDL_CreateWindow("Ryu Render",
            SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
            ScreenWidth, ScreenHeight,
            SDL_WindowFlags.SDL_WINDOW_SHOWN);

        if (_pWindow == IntPtr.Zero) throw new SDLException();

        _pRenderer = SDL_CreateRenderer(_pWindow, -1, SDL_RendererFlags.SDL_RENDERER_SOFTWARE);
        if (_pRenderer == IntPtr.Zero) throw new SDLException();

        _ball = new SDL_Rect
        {
            w = BallSize,
            h = BallSize,
            x = (ScreenWidth - BallSize) / 2,
            y = (ScreenHeight - BallSize) / 2,
        };

        _paddle1 = new SDL_Rect
        {
            w = PaddleWidth,
            h = PaddleHeight,
            x = PaddleOffset,
            y = (ScreenHeight - PaddleHeight) / 2,
        };

        _paddle2 = new SDL_Rect
        {
            w = PaddleWidth,
            h = PaddleHeight,
            x = ScreenWidth - PaddleWidth - PaddleOffset,
            y = (ScreenHeight - PaddleHeight) / 2,
        };
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
                else if (e.type == SDL_EventType.SDL_KEYDOWN)
                {
                    switch (e.key.keysym.sym)
                    {
                        case SDL_Keycode.SDLK_w:
                            _wPressed = true;
                            break;
                        case SDL_Keycode.SDLK_s:
                            _sPressed = true;
                            break;
                        case SDL_Keycode.SDLK_UP:
                            _upPressed = true;
                            break;
                        case SDL_Keycode.SDLK_DOWN:
                            _downPressed = true;
                            break;
                    }
                }
                else if (e.type == SDL_EventType.SDL_KEYUP)
                {
                    switch (e.key.keysym.sym)
                    {
                        case SDL_Keycode.SDLK_w:
                            _wPressed = false;
                            break;
                        case SDL_Keycode.SDLK_s:
                            _sPressed = false;
                            break;
                        case SDL_Keycode.SDLK_UP:
                            _upPressed = false;
                            break;
                        case SDL_Keycode.SDLK_DOWN:
                            _downPressed = false;
                            break;
                    }
                }
            }

            Run();
        }
    }

    private void Run()
    {
        Update();
        Render();

        // Don't eat up the whole thread
        // Thread.Sleep(1);
    }

    private void Update()
    {
        // Move paddles
        if (_wPressed && !_sPressed && _paddle1.y > 0)
            _paddle1.y -= 2;
        if (_sPressed && !_wPressed && _paddle1.y + _paddle1.h < ScreenHeight)
            _paddle1.y += 2;
        if (_upPressed && !_downPressed && _paddle2.y > 0)
            _paddle2.y -= 2;
        if (_downPressed && !_upPressed && _paddle2.y + _paddle2.h < ScreenHeight)
            _paddle2.y += 2;

        // Apply velocity
        _ball.x += _dx;
        _ball.y += _dy;

        // Bounce off walls
        if (_ball.y is < 0 or > ScreenHeight - BallSize)
            _dy = -_dy;

        // Bounce off paddle 1 right edge
        if (Math.Abs(_ball.x - (_paddle1.x + _paddle1.w)) < 2 &&
            _ball.y < _paddle1.y + _paddle1.h &&
            _paddle1.y < _ball.y + _ball.h)
        {
            _dx = 1;
        }

        // Bounce off paddle 2 left edge
        if (Math.Abs((_ball.x + _ball.w) - _paddle2.x) < 2 &&
            _ball.y < _paddle2.y + _paddle2.h &&
            _paddle2.y < _ball.y + _ball.h)
        {
            _dx = -1;
        }

        if (_ball.x < 0)
        {
            // Player 2 point
            _p2Score++;
            Console.WriteLine($"Player 2 Goal! Score: {_p1Score}:{_p2Score}");

            _ball.x = (ScreenWidth - BallSize) / 2;
            _ball.y = (ScreenHeight - BallSize) / 2;
        }
        else if (_ball.x + _ball.w > ScreenWidth)
        {
            // Player 1 point
            _p1Score++;
            Console.WriteLine($"Player 1 Goal! Score: {_p1Score}:{_p2Score}");

            _ball.x = (ScreenWidth - BallSize) / 2;
            _ball.y = (ScreenHeight - BallSize) / 2;
        }
    }

    private void Render()
    {
        SDL_SetRenderDrawColor(_pRenderer, 0x00, 0x00, 0x00, 0xff);
        SDL_RenderClear(_pRenderer);
        SDL_SetRenderDrawColor(_pRenderer, 0xff, 0xff, 0xff, 0xff);
        SDL_RenderFillRect(_pRenderer, ref _ball);
        SDL_RenderFillRect(_pRenderer, ref _paddle1);
        SDL_RenderFillRect(_pRenderer, ref _paddle2);
        SDL_RenderPresent(_pRenderer);
    }
}
