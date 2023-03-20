using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Metroidvania;

public enum PlayerState
{
    Idle,
    Run,
    Jump,
    Fall
}

public class Player
{
    public Color Color = Color.Black;
    public Vector2 Position;
    public Vector2 Velocity;
    public AABB CollisionBox;
    public bool IsGrounded;
    public PlayerState State;

    public float JumpDuration;
}

public class PlayerController
{
    public void Update(float gameTime)
    {
        ApplyGravity(gameTime);

        _player.Velocity.X = _input.GetHorizontalVelocity() * PlayerMovementSpeed;

        var wasJumping = _isJumping;
        _isJumping = _input.IsUpDown();

        if (_isJumping && _player.IsGrounded)
        {
            _player.Velocity.Y = _jumpVelocity;
            _player.IsGrounded = false;
            _isJumping = true;
        }

        if (wasJumping && !_isJumping && _player.Velocity.Y < 0)
            _player.Velocity.Y = 0;

        ApplyVelocity(gameTime);
        ResolveCollisions();
    }

    private void ApplyVelocity(float gameTime)
    {
        Move(_player.Velocity * gameTime);
    }

    private void ApplyGravity(float gameTime)
    {
        if (_player.IsGrounded) return;

        _player.Velocity.Y += Gravity * gameTime;
    }

    private void Move(Vector2 move)
    {
        _player.Position += move;
        _player.CollisionBox.Min += move;
        _player.CollisionBox.Max += move;
    }

    private void ResolveCollisions()
    {
        Span<AABB> collisionBuffer = stackalloc AABB[20];
        var possiblyCollidingTiles = _tileMap.Cast(_player.CollisionBox, collisionBuffer);

        _player.IsGrounded = false;

        foreach (var tile in possiblyCollidingTiles)
        {
            if (CollisionMath.AreColliding(_player.CollisionBox, tile, out var penetrationVector))
            {
                Move(-penetrationVector);
            }

            if (tile.Min.Y == _player.CollisionBox.Max.Y)
            {
                _player.IsGrounded = true;
                _player.Velocity.Y = 0;
            }
        }
    }

    public PlayerController(Player player, TileMap tileMap)
    {
        _player = player;
        _tileMap = tileMap;

        _jumpVelocity = (2f * _jumpHeight / _jumpTimeToPeak) * -1f;
        _jumpGravity = (-2f * _jumpHeight / (_jumpTimeToPeak * _jumpTimeToPeak)) * -1f;
        _fallGravity = (-2f * _jumpHeight / (_jumpTimeToDescend * _jumpTimeToDescend)) * -1f;
    }

    private const int PlayerMovementSpeed = 300;
    private float Gravity => _player.Velocity.Y < 0 && _isJumping ? _jumpGravity : _fallGravity;

    private float _jumpHeight = 128;
    private float _jumpTimeToPeak = 0.5f;
    private float _jumpTimeToDescend = 0.4f;

    private float _jumpVelocity;
    private float _jumpGravity;
    private float _fallGravity;

    private bool _isJumping;

    private readonly Player _player;
    private readonly TileMap _tileMap;

    private readonly Input _input =
        new Input()
            .BindKeyboardLeft(Keys.Left)
            .BindKeyboardLeft(Keys.A)
            .BindKeyboardRight(Keys.Right)
            .BindKeyboardRight(Keys.D)
            .BindKeyboardUp(Keys.Space)
            .BindKeyboardUp(Keys.Z);
}
