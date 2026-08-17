using System.Runtime.InteropServices;

namespace WinFormsLib
{

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

        public UIColorTable(ref Color foreColor, ref Color backColor, [Optional, DefaultParameterValue(0.5d)] ref double contrast, [Optional, DefaultParameterValue(false)] ref bool useGradientColors) : this(new UIColorScheme(foreColor, backColor, contrast), useGradientColors)
        {
        }

        public static string[] KnownColorNames { get; } = GetKnownColorNames();

        public UIColorScheme ColorScheme { get; } = uiClrSchm;

        public bool UseGradient { get; set; } = useGradientColors;

        public override Color ToolStripDropDownBackground
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color CheckBackground
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        public override Color CheckPressedBackground
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color CheckSelectedBackground
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        public override Color MenuItemSelected
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        public override Color GripDark
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        public override Color GripLight
        {
            get
            {
                return ColorScheme.ForeColor4;
            }
        }

        public override Color SeparatorDark
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        public override Color SeparatorLight
        {
            get
            {
                return ColorScheme.ForeColor4;
            }
        }

        public override Color ToolStripBorder
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        public override Color MenuBorder
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        public override Color MenuItemBorder
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        public override Color ButtonPressedBorder
        {
            get
            {
                return ColorScheme.BackColor4;
            }
        }

        public override Color ButtonSelectedBorder
        {
            get
            {
                return ColorScheme.BackColor3;
            }
        }

        public override Color ButtonCheckedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ButtonCheckedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color ButtonCheckedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        public override Color ButtonPressedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ButtonPressedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color ButtonPressedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        public override Color ButtonSelectedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ButtonSelectedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color ButtonSelectedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        public override Color ImageMarginGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ImageMarginGradientBegin
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ImageMarginGradientMiddle
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ImageMarginRevealedGradientEnd
        {
            get
            {
                return ColorScheme.ForeColor2;
            }
        }

        public override Color ImageMarginRevealedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color ImageMarginRevealedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        public override Color MenuItemPressedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color MenuItemPressedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color MenuItemPressedGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color MenuStripGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color MenuStripGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color OverflowButtonGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color OverflowButtonGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color OverflowButtonGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

        public override Color RaftingContainerGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color RaftingContainerGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color ToolStripGradientEnd
        {
            get
            {
                return ColorScheme.BackColor2;
            }
        }

        public override Color ToolStripGradientBegin
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor4;
            }
        }

        public override Color ToolStripGradientMiddle
        {
            get
            {
                return !UseGradient ? ColorScheme.BackColor2 : ColorScheme.BackColor3;
            }
        }

    }

}
