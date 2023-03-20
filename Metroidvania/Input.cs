using Microsoft.Xna.Framework.Input;

namespace Metroidvania;

public static class KeyboardListener
{
    private static KeyboardState _currentState;

    public static bool IsDown(Keys key) => _currentState.IsKeyDown(key);

    public static void Update()
    {
        _currentState = Keyboard.GetState();
    }
}

public class Input
{
    private readonly List<Keys> _leftKeys = new(capacity: 10);
    private readonly List<Keys> _rightKeys = new(capacity: 10);
    private readonly List<Keys> _upKeys = new(capacity: 10);

    public Input BindKeyboardLeft(Keys key)
    {
        _leftKeys.Add(key);
        return this;
    }

    public Input BindKeyboardRight(Keys key)
    {
        _rightKeys.Add(key);
        return this;
    }

    public Input BindKeyboardUp(Keys key)
    {
        _upKeys.Add(key);
        return this;
    }

    public int GetHorizontalVelocity()
    {
        if (AnyKeyDown(_leftKeys)) return -1;
        if (AnyKeyDown(_rightKeys)) return 1;
        return 0;
    }

    public bool IsUpDown() => AnyKeyDown(_upKeys);

    private static bool AnyKeyDown(List<Keys> keys)
    {
        foreach (var key in keys)
            if (KeyboardListener.IsDown(key)) return true;

        return false;
    }
}
