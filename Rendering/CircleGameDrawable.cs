using Microsoft.Maui.Graphics;

//public class CircleGameDrawable : IDrawable
//{
//    public Color CurrentColor { get; set; } = Colors.Red;
//    public float GlowIntensity { get; set; }
//    public float PulseScale { get; set; } = 1f;
//    public int GlowCircleCount { get; set; } = 0; // Nouveau : nombre de cercles de glow actifs

//    //    public void Draw(ICanvas canvas, RectF dirtyRect)
//    //    {
//    //        float centerX = dirtyRect.Center.X;
//    //        float centerY = dirtyRect.Center.Y;

//    //        float radius = 120 * PulseScale;

//    //        Color displayedColor = CurrentColor;

//    //        // Glow
//    //        for (int i = 5; i >= 1; i--)
//    //        {
//    //            canvas.StrokeColor = CurrentColor.WithAlpha(GlowIntensity * 0.15f);

//    //            canvas.StrokeSize = 10 + (i * 6);

//    //            canvas.DrawCircle(centerX, centerY, radius);
//    //        }

//    //        if (GlowIntensity > 0.95f)
//    //        {
//    //            if (DateTime.Now.Millisecond < 500)
//    //            {
//    //                displayedColor = Colors.White;
//    //            }
//    //        }

//    //        // Cercle principal
//    //        canvas.StrokeColor = CurrentColor;
//    //        canvas.StrokeSize = 12;

//    //        canvas.DrawCircle(centerX, centerY, radius);
//    //    }

//    public void Draw(ICanvas canvas, RectF dirtyRect)
//    {
//        float centerX = dirtyRect.Center.X;
//        float centerY = dirtyRect.Center.Y;
//        float radius = 120 * PulseScale;

//        // Cercle principal
//        canvas.StrokeColor = CurrentColor;
//        canvas.StrokeSize = 12;
//        canvas.DrawCircle(centerX, centerY, radius);

//        // Effet de glow progressif (uniquement les cercles actifs)
//        for (int i = 1; i <= GlowCircleCount; i++)
//        {
//            float glowRadius = radius + (i * 10); // Cercles de plus en plus grands
//            float glowAlpha = GlowIntensity * (0.3f / i); // Opacité décroissante
//            //float glowAlpha = GlowIntensity * (0.5f / i); // Opacité décroissante

//            canvas.StrokeColor = CurrentColor.WithAlpha(glowAlpha);
//            canvas.StrokeSize = 8;
//            canvas.DrawCircle(centerX, centerY, glowRadius);
//        }
//    }
//}

public class CircleGameDrawable : IDrawable
{
    public Color CurrentColor { get; set; } = Colors.Red;
    public float PulseScale { get; set; } = 1f;
    public List<float> GlowIntensities { get; set; } = new List<float>();
    public bool IsPulsing { get; set; } = false;
    public float PulseProgress { get; set; } = 0f;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float centerX = dirtyRect.Center.X;
        float centerY = dirtyRect.Center.Y;
        float radius = 120 * PulseScale;

        // Cercle principal
        canvas.StrokeColor = CurrentColor;
        canvas.StrokeSize = 12;
        canvas.DrawCircle(centerX, centerY, radius);

        // Effet de glow avec dégradé continu (pas de contours)
        for (int i = 0; i < GlowIntensities.Count; i++)
        {
            float glowRadius = radius + (i + 1) * 10;
            float glowAlpha = GlowIntensities[i] * 0.3f;

            if (IsPulsing)
            {
                glowAlpha *= (0.5f + 0.5f * PulseProgress);
            }

            // Trait épais pour estomper les contours
            canvas.StrokeColor = CurrentColor.WithAlpha(glowAlpha);
            canvas.StrokeSize = 15; // Trait épais pour un effet de dégradé
            canvas.DrawCircle(centerX, centerY, glowRadius);
        }
    }
}