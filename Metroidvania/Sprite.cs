using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Metroidvania;

public record TextureRegion2D(Rectangle Region, Texture2D Texture);
public record Sprite(TextureRegion2D TextureRegion, Vector2 Origin);
