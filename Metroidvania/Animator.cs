namespace Metroidvania;

public class Animator
{
    public Sprite[] Frames { get; }
    public int FrameId { get; private set; }

    public void Update(float gameTime)
    {
        var duration = _duration % _animationDuration;
        FrameId = (int) (duration / _frameDuration);

        _duration += gameTime;
    }

    public Animator(Sprite[] frames, float frameDuration)
    {
        Frames = frames;
        _frameDuration = frameDuration;
        _animationDuration = frames.Length * frameDuration;
    }

    private readonly float _frameDuration;
    private float _duration;
    private readonly float _animationDuration;
}
