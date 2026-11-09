namespace WinFormsLib;

/// <summary>Provides foreground and background colors with derived HSV value variants.</summary>
public readonly struct UIColorScheme
{
    /// <summary>Gets whether the foreground's highest RGB channel is greater than the background's.</summary>
    public bool IsDark { get; }

    /// <summary>Gets the foreground variant with the largest contrast increase.</summary>
    public Color ForeColor0 { get; }
    /// <summary>Gets the foreground variant with a smaller contrast increase.</summary>
    public Color ForeColor1 { get; }
    /// <summary>Gets the original foreground color.</summary>
    public Color ForeColor2 { get; }
    /// <summary>Gets the foreground variant with a smaller contrast decrease.</summary>
    public Color ForeColor3 { get; }
    /// <summary>Gets the foreground variant with the largest contrast decrease.</summary>
    public Color ForeColor4 { get; }

    /// <summary>Gets the background variant with the largest contrast increase.</summary>
    public Color BackColor0 { get; }
    /// <summary>Gets the background variant with a smaller contrast increase.</summary>
    public Color BackColor1 { get; }
    /// <summary>Gets the original background color.</summary>
    public Color BackColor2 { get; }
    /// <summary>Gets the background variant with a smaller contrast decrease.</summary>
    public Color BackColor3 { get; }
    /// <summary>Gets the background variant with the largest contrast decrease.</summary>
    public Color BackColor4 { get; }

    /// <summary>Creates a color scheme from the supplied foreground and background.</summary>
    /// <param name="foreColor">The base foreground color.</param>
    /// <param name="backColor">The base background color.</param>
    /// <param name="contrast">The multiplier for the value adjustments used to derive variants.</param>
    /// <remarks>Derived colors are opaque, and their HSV value is clamped to the range from 0 to 1.</remarks>
    public UIColorScheme(Color foreColor, Color backColor, double contrast = 0.5)
    {
        IsDark = foreColor.IsLighterThan(backColor);
        ForeColor2 = foreColor;
        BackColor2 = backColor;

        if (IsDark)
        {
            ForeColor0 = foreColor.WithValue(0.2 * contrast);
            ForeColor1 = foreColor.WithValue(0.1 * contrast);
            ForeColor3 = foreColor.WithValue(-0.1 * contrast);
            ForeColor4 = foreColor.WithValue(-0.2 * contrast);
            BackColor0 = backColor.WithValue(-0.2 * contrast);
            BackColor1 = backColor.WithValue(-0.1 * contrast);
            BackColor3 = backColor.WithValue(0.1 * contrast);
            BackColor4 = backColor.WithValue(0.2 * contrast);
        }
        else
        {
            ForeColor0 = foreColor.WithValue(-0.2 * contrast);
            ForeColor1 = foreColor.WithValue(-0.1 * contrast);
            ForeColor3 = foreColor.WithValue(0.1 * contrast);
            ForeColor4 = foreColor.WithValue(0.2 * contrast);
            BackColor0 = backColor.WithValue(0.2 * contrast);
            BackColor1 = backColor.WithValue(0.1 * contrast);
            BackColor3 = backColor.WithValue(-0.1 * contrast);
            BackColor4 = backColor.WithValue(-0.2 * contrast);
        }
    }
}
