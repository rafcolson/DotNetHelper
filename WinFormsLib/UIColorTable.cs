using System.Runtime.InteropServices;

namespace WinFormsLib
{

    /// <summary>Supplies WinForms tool strip and menu colors from a color scheme.</summary>
    /// <param name="uiClrSchm">The scheme used to render the controls.</param>
    /// <param name="useGradientColors">Whether to use distinct gradient stops where supported.</param>
    public class UIColorTable(UIColorScheme uiClrSchm, bool useGradientColors = false) : ProfessionalColorTable
    {
        private static string[] GetKnownColorNames()
        {
            List<string> colorNames = [];
            foreach (KnownColor kc in Enum.GetValues<KnownColor>())
            {
                if (Enum.GetName(kc) is string n && !n.Equals("Transparent"))
                {
                    colorNames.Add(n);
                }
            }
            return [.. colorNames];
        }

        /// <summary>Creates a tool strip color table from base colors and contrast settings.</summary>
        /// <param name="foreColor">The base foreground color.</param>
        /// <param name="backColor">The base background color.</param>
        /// <param name="contrast">The multiplier used to derive color variants.</param>
        /// <param name="useGradientColors">Whether to use distinct gradient stops where supported.</param>
        public UIColorTable(ref Color foreColor, ref Color backColor, [Optional, DefaultParameterValue(0.5d)] ref double contrast, [Optional, DefaultParameterValue(false)] ref bool useGradientColors) : this(new UIColorScheme(foreColor, backColor, contrast), useGradientColors)
        {
        }

        /// <summary>Gets the shared array of known color names, excluding Transparent.</summary>
        public static string[] KnownColorNames { get; } = GetKnownColorNames();

        /// <summary>Gets the color scheme used by this table.</summary>
        public UIColorScheme ColorScheme { get; } = uiClrSchm;

        /// <summary>Gets or sets whether configurable gradient stops use different shades from the scheme.</summary>
        public bool UseGradient { get; set; } = useGradientColors;

        /// <inheritdoc/>
        public override Color ToolStripDropDownBackground
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color CheckBackground
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color CheckPressedBackground
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color CheckSelectedBackground
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemSelected
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color GripDark
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color GripLight
        {
            get
            {
                return ColorScheme.ForeColor4;
            }
        }

        /// <inheritdoc/>
        public override Color SeparatorDark
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color SeparatorLight
        {
            get
            {
                return ColorScheme.ForeColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ToolStripBorder
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color MenuBorder
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemBorder
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonPressedBorder
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonSelectedBorder
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonCheckedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonCheckedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonCheckedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonPressedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonPressedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonPressedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonSelectedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonSelectedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ButtonSelectedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color ImageMarginGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ImageMarginGradientBegin
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ImageMarginGradientMiddle
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        /// <remarks>Uses the base foreground color for gradients and the base background color for a flat fill.</remarks>
        public override Color ImageMarginRevealedGradientEnd
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.ForeColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ImageMarginRevealedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ImageMarginRevealedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemPressedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemPressedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemPressedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemSelectedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color MenuItemSelectedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color MenuStripGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color MenuStripGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color OverflowButtonGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color OverflowButtonGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color OverflowButtonGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        /// <inheritdoc/>
        public override Color RaftingContainerGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color RaftingContainerGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ToolStripGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        /// <inheritdoc/>
        public override Color ToolStripGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        /// <inheritdoc/>
        public override Color ToolStripGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

    }

}
