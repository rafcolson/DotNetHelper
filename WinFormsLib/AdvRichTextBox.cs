using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace WinFormsLib
{

    public sealed class RtfCharacterStyle
    {

        public string Name { get; private set; }
        public Font Font { get; private set; }
        public Color Color { get; private set; }

        public RtfCharacterStyle(string name, Font font, Color color)
        {
            Name = name;
            Font = font;
            Color = color;
        }

    }

    public class AdvRichTextBox : RichTextBox
    {

        #region Initialization

        public AdvRichTextBox()
        {

            #endregion

            #region ScrollToCaret

            ScrollToCaretTimer = new System.Windows.Forms.Timer();
            InitSelectionAlignment();
            InitInsertLink();
            ScrollToCaretTimer.Tick += AllMouseLeaveTimer_Tick;
        }

        private readonly System.Windows.Forms.Timer ScrollToCaretTimer;

        private void AllMouseLeaveTimer_Tick(object sender, EventArgs e)
        {
            ScrollToCaretTimer.Stop();
            base.ScrollToCaret();
        }

        public new void ScrollToCaret()
        {
            ScrollToCaretTimer.Start();
        }

        #endregion

        #region Zoomfactor

        public new void Clear()
        {
            float f = ZoomFactor;
            base.Clear();
            plainLinks.Clear();
            characterStyleRanges.Clear();
            ZoomFactor *= f;
        }

        #endregion

        #region Highlighting

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color HighlightForeColor { get; set; } = Color.Blue;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color HighlightBackColor { get; set; } = Color.Yellow;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool HighlightMatchCase { get; set; } = false;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool HighlightPartialMatch { get; set; } = false;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Utils.WordSearchOptions HighlightOptions { get; set; } = Utils.WordSearchOptions.AllWords;

        public enum TextAlignment
        {
            Left = 1,
            Right = 2,
            Center = 3,
            Justify = 4
        }

        public void AppendText(string text, Font? font = null, Color? color = null, string? highlighted = null, short characterStyle = 0)
        {

            if (!(font == null))
            {
                SelectionFont = font;
            }

            if (color.HasValue)
            {
                SelectionColor = color.Value;
            }

            int offset = default;

            if (!(highlighted == null))
            {
                offset = SelectionStart;
            }

            int start = SelectionStart;
            base.AppendText(text);

            if (!characterStyle.Equals(0) && text.Any())
            {
                AddCharacterStyleRange(start, text.Length, characterStyle);
            }

            if (!(highlighted == null))
            {
                bool argmatchCase = HighlightMatchCase;
                bool argpartialMatch = HighlightPartialMatch;
                Utils.WordSearchOptions argoptions = HighlightOptions;
                int[][] occurrences = Utils.GetOccurrences(highlighted, text, ref argmatchCase, ref argpartialMatch, ref argoptions);
                HighlightMatchCase = argmatchCase;
                HighlightPartialMatch = argpartialMatch;
                HighlightOptions = argoptions;
                foreach (int[] ia in occurrences)
                {
                    Select(offset + ia.First(), ia.Last());
                    SelectionColor = HighlightForeColor;
                    SelectionBackColor = HighlightBackColor;
                }
            }

            DeselectAll();
        }

        #endregion

        #region SelectionAlignment

        private const int EM_SETEVENTMASK = 1073;
        private const int EM_GETPARAFORMAT = 1085;
        private const int EM_SETPARAFORMAT = 1095;
        private const int EM_SETTYPOGRAPHYOPTIONS = 1226;
        private const int WM_SETREDRAW = 11;
        private const int TO_ADVANCEDTYPOGRAPHY = 1;
        private const int PFM_ALIGNMENT = 8;
        private const int SCF_SELECTION = 1;

        private int updating = 0;
        private int oldEventMask = 0;

        public void BeginUpdate()
        {
            updating += 1;
            if (updating > 1)
            {
                return;
            }

            oldEventMask = SendMessage(new HandleRef(this, Handle), EM_SETEVENTMASK, 0, 0);
            _ = SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, 0, 0);
        }

        public void EndUpdate()
        {
            updating -= 1;
            if (updating > 0)
            {
                return;
            }

            _ = SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, 1, 0);
            _ = SendMessage(new HandleRef(this, Handle), EM_SETEVENTMASK, 0, oldEventMask);
        }

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public new TextAlignment SelectionAlignment
        {
            get
            {
                PARAFORMAT fmt = new()
                {
                    cbSize = Marshal.SizeOf<PARAFORMAT>()
                };
                _ = SendMessage(new HandleRef(this, Handle), EM_GETPARAFORMAT, SCF_SELECTION, ref fmt);
                return (fmt.dwMask & (long)PFM_ALIGNMENT) == 0L ? TextAlignment.Left : (TextAlignment)fmt.wAlignment;
            }
            set
            {
                PARAFORMAT fmt = new()
                {
                    cbSize = Marshal.SizeOf<PARAFORMAT>(),
                    dwMask = PFM_ALIGNMENT,
                    wAlignment = (short)value
                };
                _ = SendMessage(new HandleRef(this, Handle), EM_SETPARAFORMAT, SCF_SELECTION, ref fmt);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PARAFORMAT
        {
            public int cbSize;
            public uint dwMask;
            public short wNumbering;
            public short wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public short wAlignment;
            public short cTabCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;
            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public short wShadingWeight;
            public short wShadingStyle;
            public short wNumberingStart;
            public short wNumberingStyle;
            public short wNumberingTab;
            public short wBorderSpace;
            public short wBorderWidth;
            public short wBorders;
        }

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT lp);

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern nint SendMessagePointer(HandleRef hWnd, int msg, nint wParam, nint lParam);

        private void InitSelectionAlignment()
        {
            _ = SendMessage(new HandleRef(this, Handle), EM_SETTYPOGRAPHYOPTIONS, TO_ADVANCEDTYPOGRAPHY, TO_ADVANCEDTYPOGRAPHY);
        }

        #endregion

        #region InsertLink

        private class PlainLink
        {
            internal readonly int Start;
            internal readonly int Length;
            internal readonly string Text;

            internal PlainLink(int start, int length, string text)
            {
                Start = start;
                Length = length;
                Text = text;
            }

            internal bool Contains(int index)
            {
                return index >= Start && index < Start + Length;
            }
        }

        private class CharacterStyleRange
        {
            internal readonly int Start;
            internal readonly int Length;
            internal readonly short Style;

            internal CharacterStyleRange(int start, int length, short style)
            {
                Start = start;
                Length = length;
                Style = style;
            }
        }

        private readonly List<PlainLink> plainLinks = [];
        private readonly List<CharacterStyleRange> characterStyleRanges = [];

        private void AddCharacterStyleRange(int start, int length, short style)
        {
            if (characterStyleRanges.Any())
            {
                CharacterStyleRange previous = characterStyleRanges.Last();
                if (previous.Style.Equals(style) && previous.Start + previous.Length == start)
                {
                    characterStyleRanges[^1] = new CharacterStyleRange(previous.Start, previous.Length + length, style);
                    return;
                }
            }
            characterStyleRanges.Add(new CharacterStyleRange(start, length, style));
        }

        // Private Const SCF_SELECTION As Integer = &H1
        private const int WM_USER = 0x400;
        private const int EM_GETCHARFORMAT = WM_USER + 58;
        private const int EM_SETCHARFORMAT = WM_USER + 68;
        private const uint CFE_LINK = 32U;
        private const uint CFM_LINK = 32U;
        private const uint CFM_UNDERLINE = 4U;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public new bool DetectUrls
        {
            get
            {
                return base.DetectUrls;
            }
            set
            {
                base.DetectUrls = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHARFORMAT2_STRUCT
        {
            public uint cbSize;
            public uint dwMask;
            public uint dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
            public ushort wWeight;
            public ushort sSpacing;
            public int crBackColor;
            public int lcid;
            public int dwReserved;
            public short sStyle;
            public short wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        private void InitInsertLink()
        {
            base.DetectUrls = false;
        }

        private void GetSelectionStyle(ref uint mask, ref uint effects)
        {
            CHARFORMAT2_STRUCT cf = new();
            cf.cbSize = Convert.ToUInt32(Marshal.SizeOf(cf));
            cf.szFaceName = new char[32];
            nint wpar = new(SCF_SELECTION);
            nint lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            try
            {
                Marshal.StructureToPtr(cf, lpar, false);
                _ = SendMessagePointer(new HandleRef(this, Handle), EM_GETCHARFORMAT, wpar, lpar);
                cf = Marshal.PtrToStructure<CHARFORMAT2_STRUCT>(lpar);
                mask = cf.dwMask;
                effects = cf.dwEffects;
            }
            finally
            {
                Marshal.FreeCoTaskMem(lpar);
            }
        }

        private void SetSelectionStyle(uint mask, uint effects)
        {
            CHARFORMAT2_STRUCT cf = new();
            cf.cbSize = Convert.ToUInt32(Marshal.SizeOf(cf));
            cf.dwMask = mask;
            cf.dwEffects = effects;
            nint wpar = new(SCF_SELECTION);
            nint lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            try
            {
                Marshal.StructureToPtr(cf, lpar, false);
                _ = SendMessagePointer(new HandleRef(this, Handle), EM_SETCHARFORMAT, wpar, lpar);
            }
            finally
            {
                Marshal.FreeCoTaskMem(lpar);
            }
        }

        public void AppendLink(string text, Font? font = null, Color? color = null, bool underlined = true, short characterStyle = 0)
        {
            if (!(font == null))
            {
                SelectionFont = font;
            }
            if (color.HasValue)
            {
                SelectionColor = color.Value;
            }

            uint mask = default;
            uint effects = default;
            GetSelectionStyle(ref mask, ref effects);

            int length = text.Length;
            int position = SelectionStart;
            SelectedText = text;
            Select(position, length);
            if (underlined)
            {
                SetSelectionStyle(CFM_LINK, CFE_LINK);
            }
            else
            {
                SetSelectionStyle(CFM_UNDERLINE, 0U);
                plainLinks.Add(new PlainLink(position, length, text));
            }
            if (!characterStyle.Equals(0) && text.Any())
            {
                AddCharacterStyleRange(position, length, characterStyle);
            }
            Select(position + length, 0);
            SetSelectionStyle(mask, effects);
        }

        public string GetRtfWithCharacterStyles(IDictionary<short, RtfCharacterStyle> styles)
        {
            if (styles.Count.Equals(0) || characterStyleRanges.Count.Equals(0))
            {
                return Rtf;
            }

            List<KeyValuePair<short, RtfCharacterStyle>> activeStyles = styles.Where(style => characterStyleRanges.Any(range => range.Style.Equals(style.Key))).ToList();
            if (activeStyles.Count.Equals(0))
            {
                return Rtf;
            }

            const string markerPrefix = "BQRTFCHARSTYLE";

            foreach (CharacterStyleRange range in characterStyleRanges.Where(item => activeStyles.Any(style => style.Key.Equals(item.Style))).OrderByDescending(item => item.Start))
            {
                Select(range.Start + range.Length, 0);
                SelectedText = markerPrefix + "END" + range.Style;
                Select(range.Start, 0);
                SelectedText = markerPrefix + "START" + range.Style;
            }

            string styledRtf = Rtf;
            int fontTableStart = styledRtf.IndexOf(@"{\fonttbl", StringComparison.Ordinal);
            int fontTableEnd = FindRtfGroupEnd(styledRtf, fontTableStart);
            int nextFontIndex = Regex
                .Matches(styledRtf.Substring(fontTableStart, fontTableEnd - fontTableStart + 1), @"\\f(\d+)")
                .Cast<Match>()
                .Select(item => int.Parse(item.Groups[1].Value))
                .DefaultIfEmpty(-1)
                .Max() + 1;

            int colorTableStart = styledRtf.IndexOf(@"{\colortbl", StringComparison.Ordinal);
            if (colorTableStart.Equals(-1))
            {
                styledRtf = styledRtf.Insert(fontTableEnd + 1, @"{\colortbl ;}");
                colorTableStart = fontTableEnd + 1;
            }
            int colorTableEnd = FindRtfGroupEnd(styledRtf, colorTableStart);
            int nextColorIndex = styledRtf.Substring(colorTableStart, colorTableEnd - colorTableStart + 1).Count(character => character.Equals(';'));

            StringBuilder styleSheet = new(@"{\stylesheet");
            foreach (KeyValuePair<short, RtfCharacterStyle> style in activeStyles)
            {
                int fontIndex = nextFontIndex;
                int colorIndex = nextColorIndex;
                nextFontIndex += 1;
                nextColorIndex += 1;

                fontTableEnd = FindRtfGroupEnd(styledRtf, fontTableStart);
                styledRtf = styledRtf.Insert(fontTableEnd, @"{\f" + fontIndex + @"\fnil\fcharset0 " + EscapeRtf(style.Value.Font.Name) + ";}");

                colorTableStart = styledRtf.IndexOf(@"{\colortbl", StringComparison.Ordinal);
                colorTableEnd = FindRtfGroupEnd(styledRtf, colorTableStart);
                styledRtf = styledRtf.Insert(colorTableEnd, @"\red" + style.Value.Color.R + @"\green" + style.Value.Color.G + @"\blue" + style.Value.Color.B + ";");

                _ = styleSheet.Append(@"{\*\cs").Append(style.Key).Append(@"\additive\f").Append(fontIndex).Append(@"\fs").Append((int)Math.Round(Math.Round((double)(style.Value.Font.SizeInPoints * 2.0f)))).Append(@"\cf").Append(colorIndex);
                if (style.Value.Font.Bold)
                {
                    _ = styleSheet.Append(@"\b");
                }
                if (style.Value.Font.Italic)
                {
                    _ = styleSheet.Append(@"\i");
                }
                if (style.Value.Font.Underline)
                {
                    _ = styleSheet.Append(@"\ul");
                }
                if (style.Value.Font.Strikeout)
                {
                    _ = styleSheet.Append(@"\strike");
                }
                _ = styleSheet.Append(" ").Append(EscapeRtf(style.Value.Name)).Append(";}");
            }
            _ = styleSheet.Append("}");

            foreach (KeyValuePair<short, RtfCharacterStyle> style in activeStyles)
            {
                styledRtf = styledRtf.Replace(markerPrefix + "START" + style.Key, @"{\cs" + style.Key + " ").Replace(markerPrefix + "END" + style.Key, "}");
            }

            int insertionIndex = styledRtf.IndexOf(@"{\fonttbl", StringComparison.Ordinal);
            if (insertionIndex.Equals(-1))
            {
                insertionIndex = styledRtf.IndexOf(' ');
            }
            return styledRtf.Insert(insertionIndex, styleSheet.ToString());
        }

        private static string EscapeRtf(string value)
        {
            return value.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");
        }

        private static int FindRtfGroupEnd(string rtf, int groupStart)
        {
            if (groupStart < 0)
            {
                return -1;
            }

            int depth = default;
            for (int index = groupStart, loopTo = rtf.Length - 1; index <= loopTo; index++)
            {
                if (rtf[index].Equals('{') && (index.Equals(0) || !rtf[index - 1].Equals('\\')))
                {
                    depth += 1;
                }
                else if (rtf[index].Equals('}') && (index.Equals(0) || !rtf[index - 1].Equals('\\')))
                {
                    depth -= 1;
                    if (depth.Equals(0))
                    {
                        return index;
                    }
                }
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Cursor = GetPlainLink(e.Location) is null ? Cursors.IBeam : Cursors.Hand;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!e.Button.Equals(MouseButtons.Left))
            {
                return;
            }

            PlainLink link = GetPlainLink(e.Location);
            if (!(link == null))
            {
                OnLinkClicked(new LinkClickedEventArgs(link.Text));
            }
        }

        private PlainLink GetPlainLink(Point location)
        {
            int index = GetCharIndexFromPosition(location);
            return plainLinks.FirstOrDefault(link => link.Contains(index));
        }

        #endregion

    }
}
