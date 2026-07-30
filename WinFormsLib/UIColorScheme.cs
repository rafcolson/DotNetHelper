namespace WinFormsLib;

public readonly struct UIColorScheme
{
    public bool IsDark { get; }

    public Color ForeColor0 { get; }
    public Color ForeColor1 { get; }
    public Color ForeColor2 { get; }
    public Color ForeColor3 { get; }
    public Color ForeColor4 { get; }

    public Color BackColor0 { get; }
    public Color BackColor1 { get; }
    public Color BackColor2 { get; }
    public Color BackColor3 { get; }
    public Color BackColor4 { get; }

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
