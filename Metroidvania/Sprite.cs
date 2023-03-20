using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;

namespace Metroidvania;

public record TextureRegion2D(Rectangle Region, Texture2D Texture);
public record Sprite(TextureRegion2D TextureRegion, Vector2 Origin);
public record SpriteAtlas(Sprite[] Sprites, Dictionary<string, int> SpriteNames);

public static class TextureDataStore
{
    private readonly static Dictionary<string, Texture2D> _textures = new();

    public static Texture2D Load(string fileName)
    {
        if (_textures.TryGetValue(fileName, out var texture))
            return texture;

        return (_textures[fileName] = Texture2D.FromFile(MetroidvaniaGame.Current.GraphicsDevice, fileName));
    }
}

public static class SpriteDataStore
{
    public static SpriteAtlas LoadSpriteAtlas(string fileName)
    {
        var json = File.ReadAllText(fileName);
        var spriteAtlasData = JsonSerializer.Deserialize<SpriteAtlasData>(json);
        var basePath = Path.GetDirectoryName(fileName);
        var texture = TextureDataStore.Load(Path.Combine(basePath, spriteAtlasData.TextureName));

        var sprites = new Sprite[spriteAtlasData.Sprites.Length];
        var spriteNames = new Dictionary<string, int>(capacity: sprites.Length);

        for (var i = 0; i < sprites.Length; i++)
        {
            var spriteData = spriteAtlasData.Sprites[i];
            var textureRegion = new TextureRegion2D(new Rectangle((int)spriteData.Min.X, (int)spriteData.Min.Y, (int) (spriteData.Max.X - spriteData.Min.X), (int)(spriteData.Max.Y - spriteData.Min.Y)), texture);
            var sprite = new Sprite(textureRegion, spriteData.Origin);
            sprites[i] = sprite;
            spriteNames[spriteData.Name] = i;
        }

        return new(sprites, spriteNames);
    }

    private record Vector2Dto(float X, float Y);
    private record SpriteData(string Name, Vector2Dto Min, Vector2Dto Max, Vector2 Origin);
    private record SpriteAtlasData(string TextureName, SpriteData[] Sprites);
}
