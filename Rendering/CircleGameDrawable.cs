using Microsoft.Maui.Graphics;

public class CircleGameDrawable : IDrawable
{
    public Color CurrentColor { get; set; } = Colors.Red;
    public Color NextColor { get; set; } = Colors.Red;
    public float PulseScale { get; set; } = 1f;
    public List<float> GlowIntensities { get; set; } = new List<float>();
    public bool IsPulsing { get; set; } = false;
    public float PulseProgress { get; set; } = 0f;
    public bool IsTransitioning { get; set; } = false;
    public float TransitionProgress { get; set; } = 0f;
    public float TransitionScale { get; set; } = 1f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float baseRadius = 100 * PulseScale * TransitionScale;

        if (IsTransitioning)
        {
            // Dessine le cercle complet en ANCIENNE couleur surchargé par le serpent (ex: rouge)
            canvas.StrokeColor = NextColor;
            canvas.StrokeSize = 12;
            canvas.DrawCircle(centerX, centerY, baseRadius);

            // Dessine l'arc (serpent) en NOUVELLE couleur qui remplace le cercle (ex: vert)
            float endAngle = -90 + (360f * TransitionProgress);
            canvas.StrokeColor = CurrentColor;
            canvas.StrokeSize = 12;
            canvas.DrawArc(centerX - baseRadius, centerY - baseRadius, baseRadius * 2, baseRadius * 2, -90, endAngle, true, false);
        }
        else
        {
            canvas.StrokeColor = CurrentColor;
            canvas.StrokeSize = 12;

            canvas.DrawCircle(centerX, centerY, baseRadius);
        }

        // Glow
        for (int i = 0; i < GlowIntensities.Count; i++)
        {
            float glowRadius = baseRadius + (i * 12);

            float baseAlpha = 1f - (i * 0.15f);

            float glowAlpha = baseAlpha * GlowIntensities[i];

            canvas.StrokeColor = CurrentColor.WithAlpha(glowAlpha);

            canvas.StrokeSize = 12;

            canvas.DrawCircle(centerX, centerY, glowRadius);
        }
    }

    public static Color Lerp(Color start, Color end, float progress)
    {
        float r = start.Red + (end.Red - start.Red) * progress;
        float g = start.Green + (end.Green - start.Green) * progress;
        float b = start.Blue + (end.Blue - start.Blue) * progress;
        float a = start.Alpha + (end.Alpha - start.Alpha) * progress;
        return Color.FromRgba(r, g, b, a);
    }
}
