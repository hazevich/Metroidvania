using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metroidvania;

public class MetroidvaniaGame : Game
{
    private readonly GraphicsDeviceManager _graphicsDeviceManager;
    private readonly TileMap _tileMap = new();
    private SpriteBatch _spriteBatch;
    private Player _player = new();
    private PlayerController _playerController;

    private RenderTarget2D _renderTarget;

    public static Vector2 DefaultGravity = Vector2.UnitY * 25f;

    private Matrix? _transformMatrix;

    private Texture2D _heroTexture;
    private Sprite _idleSprite;

    public MetroidvaniaGame()
    {
        _graphicsDeviceManager = new(this);
    }

    private void SpawnPlayer()
    {
        _player.Position = new(640 / 2, 0);
        _player.CollisionBox = AABB.Create(_player.Position.X, _player.Position.Y, 16, 32);
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new(GraphicsDevice, 640, 360);
        _graphicsDeviceManager.PreferredBackBufferWidth = 1920;
        _graphicsDeviceManager.PreferredBackBufferHeight = 1080;
        _graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
        _graphicsDeviceManager.ApplyChanges();

        SpawnPlayer();
        _playerController = new(_player, _tileMap);

        _heroTexture = Texture2D.FromFile(GraphicsDevice, @"C:\Users\user\source\repos\Metroidvania\Assets\idle(32x32).png");
        _idleSprite = new(new TextureRegion2D(new Rectangle(0, 0, 32, 32), _heroTexture), new Vector2(0));
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardListener.Update();

        if (KeyboardListener.IsDown(Keys.Escape))
            Exit();

        _playerController.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    private static Color BackgroundColor = new(110 / 255f, 115 / 255f, 113 / 255f);

    private void DrawPlayer(SpriteBatch spriteBatch)
    {
        spriteBatch.Render(_player.Position, _idleSprite);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin();
        _tileMap.Render(_spriteBatch);
        DrawPlayer(_spriteBatch);
        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(BackgroundColor);

        if (_transformMatrix == null)
        {
            var ratioX = 1920 / (float)640;
            var ratioY = 1080 / (float)360;

            var minRatio = Math.Min(ratioX, ratioY);

            _transformMatrix = Matrix.CreateScale(minRatio);
        }
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix);

        _spriteBatch.Draw(texture: _renderTarget, position: Vector2.Zero, color: Color.White);
        _spriteBatch.End();
    }
}

public class TileMap
{
    public const int TileSize = 32;
    private static Color TileColor = new(26 / 255f, 30 / 255f, 38 / 255f);

    private readonly int[,] _tiles = new int[12, 20]
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
    };

    public Span<AABB> Cast(AABB aabb, Span<AABB> buffer)
    {
        var minTileCol = Math.Clamp((int)(aabb.Min.X / TileSize) - 1, 0, 19);
        var maxTileCol = Math.Clamp((int)(aabb.Max.X / TileSize) + 1, 0, 19);
        var minTileRow = Math.Clamp((int)(aabb.Min.Y / TileSize) - 1, 0, 11);
        var maxTileRow = Math.Clamp((int)(aabb.Max.Y / TileSize) + 1, 0, 11);

        var bufferIndex = 0;

        for (var row = minTileRow; row <= maxTileRow; row++)
        {
            for (var col = minTileCol; col <= maxTileCol; col++)
            {
                if (_tiles[row, col] == 1)
                    buffer[bufferIndex++] = AABB.Create(col * TileSize, row * TileSize, TileSize, TileSize);
            }
        }

        return buffer[..bufferIndex];
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (var row = 0; row < _tiles.GetLength(0); row++)
        {
            for (var col = 0; col < _tiles.GetLength(1); col++)
            {
                if (_tiles[row, col] == 0) continue;

                var tilePosition = GetTilePosition(row, col);

                spriteBatch.DrawFilledRectangle(tilePosition, TileSize - 1, TileSize - 1, TileColor);
            }
        }
    }

    private static Vector2 GetTilePosition(int row, int col) => new Vector2(col, row) * TileSize;
}

public static class SpriteBatchExtension
{
    public static void DrawFilledRectangle(this SpriteBatch spriteBatch, Vector2 position, int width, int height, Color color) =>
        spriteBatch.Draw(
            texture: spriteBatch.GetPixel(),
            position: position,
            sourceRectangle: new(0, 0, width, height),
            color: color,
            rotation: 0,
            origin: Vector2.Zero,
            scale: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: 0);

    public static void Render(this SpriteBatch spriteBatch, Vector2 position, Sprite sprite) =>
        spriteBatch.Draw(
            texture: sprite.TextureRegion.Texture,
            position: position,
            sourceRectangle: sprite.TextureRegion.Region,
            color: Color.White,
            rotation: 0,
            origin: sprite.Origin,
            scale: Vector2.One,
            effects: SpriteEffects.None,
            layerDepth: 0);

    private static Texture2D? _pixel;
    private static Texture2D GetPixel(this SpriteBatch spriteBatch)
    {
        if (_pixel == null)
        {
            _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        return _pixel;
    }
}

public record struct AABB(Vector2 Min, Vector2 Max)
{
    public static AABB Create(float x, float y, float width, float height) =>
        new(new Vector2(x, y), new Vector2(x + width, y + height));

    public float Width => Max.X - Min.X;
    public float Height => Max.Y - Min.Y;
}

public static class CollisionMath
{
    public static bool AreColliding(AABB a, AABB b, out Vector2 penetrationVector)
    {
        var minkowskiDiff = MinkowskiDifference(a, b);

        var areColliding = minkowskiDiff.Min.X < 0 && minkowskiDiff.Max.X > 0 && minkowskiDiff.Min.Y < 0 && minkowskiDiff.Max.Y > 0;

        var minX = Math.Abs(minkowskiDiff.Min.X) < Math.Abs(minkowskiDiff.Max.X) ? minkowskiDiff.Min.X : minkowskiDiff.Max.X;
        var minY = Math.Abs(minkowskiDiff.Min.Y) < Math.Abs(minkowskiDiff.Max.Y) ? minkowskiDiff.Min.Y : minkowskiDiff.Max.Y;
        penetrationVector = Math.Abs(minX) < Math.Abs(minY) ? new Vector2(minX, 0) : new Vector2(0, minY);

        return areColliding;
    }

    public static AABB MinkowskiDifference(AABB a, AABB b)
    {
        var min = a.Min - b.Max;
        var max = a.Max - b.Min;

        return new(min, max);
    }
}

