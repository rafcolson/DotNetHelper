using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace WinFormsLib
{

    /// <summary>Defines a named character style for RTF export.</summary>
    /// <param name="name">The style name.</param>
    /// <param name="font">The font for the style.</param>
    /// <param name="color">The text color for the style.</param>
    /// <param name="raise">The baseline offset in points. Negative values are treated as zero.</param>
    public sealed class RtfCharacterStyle(string name, Font font, Color color, float raise = 0)
    {

        /// <summary>Gets the style name.</summary>
        public string Name { get; } = name;
        /// <summary>Gets the font supplied for the style.</summary>
        public Font Font { get; } = font;
        /// <summary>Gets the text color for the style.</summary>
        public Color Color { get; } = color;
        /// <summary>Gets the nonnegative baseline offset in points.</summary>
        public float Raise { get; } = Math.Max(raise, 0);
    }

    /// <summary>Extends a rich text box with formatted appending, highlighting, link handling, and named RTF style export.</summary>
    public partial class AdvRichTextBox : RichTextBox
    {

        private string? preservedRtf;
        private float preservedZoomFactor;
        private string? dpiChangeRtf;
        private bool dpiChangePending;
        private float dpiChangeZoomFactor;
        private int dpiChangeSelectionStart;
        private int dpiChangeSelectionLength;

        [GeneratedRegex(@"\\f(\d+)")]
        private static partial Regex RtfFontIndexRegex();

        #region Initialization

        /// <summary>Initializes a rich text box with advanced typography and explicit link handling.</summary>
        public AdvRichTextBox()
        {
            ScrollToCaretTimer = new System.Windows.Forms.Timer();
            // Enable advanced RichEdit typography so paragraph justification is available.
            _ = SendMessage(new HandleRef(this, Handle), EM_SETTYPOGRAPHYOPTIONS, TO_ADVANCEDTYPOGRAPHY, TO_ADVANCEDTYPOGRAPHY);
            // Links are registered and handled explicitly by AppendLink and GetPlainLink.
            base.DetectUrls = false;
            ScrollToCaretTimer.Tick += ScrollToCaretTimer_Tick;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ScrollToCaretTimer.Stop();
                ScrollToCaretTimer.Tick -= ScrollToCaretTimer_Tick;
                ScrollToCaretTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (RecreatingHandle && IsHandleCreated)
            {
                preservedRtf = Rtf;
                preservedZoomFactor = ZoomFactor;
            }
            base.OnHandleDestroyed(e);
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (preservedRtf is not null)
            {
                Rtf = preservedRtf;
                RestoreZoomFactor(preservedZoomFactor);
                preservedRtf = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnDpiChangedBeforeParent(EventArgs e)
        {
            dpiChangePending = IsHandleCreated;
            if (dpiChangePending)
            {
                dpiChangeRtf = TextLength == 0 ? null : Rtf;
                dpiChangeZoomFactor = ZoomFactor;
                dpiChangeSelectionStart = SelectionStart;
                dpiChangeSelectionLength = SelectionLength;
            }
            base.OnDpiChangedBeforeParent(e);
        }

        /// <inheritdoc/>
        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            if (dpiChangePending)
            {
                if (dpiChangeRtf is not null)
                {
                    Rtf = dpiChangeRtf;
                }
                RestoreZoomFactor(dpiChangeZoomFactor);
                Select(dpiChangeSelectionStart, dpiChangeSelectionLength);
                dpiChangeRtf = null;
                dpiChangePending = false;
            }
        }

        #endregion

        #region Scrollbar dragging

        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;
        private const int SB_THUMBPOSITION = 4;
        private const int SB_THUMBTRACK = 5;

        /// <inheritdoc/>
        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL)
                && (m.WParam.ToInt64() & 0xFFFF) == SB_THUMBTRACK)
            {
                // Apply each drag position immediately, also when Windows disables
                // live scrolling while dragging the native scrollbar thumb.
                m.WParam = (nint)((m.WParam.ToInt64() & ~0xFFFFL) | SB_THUMBPOSITION);
            }
            base.WndProc(ref m);
        }

        #endregion

        #region ScrollToCaret

        private readonly System.Windows.Forms.Timer ScrollToCaretTimer;

        private void ScrollToCaretTimer_Tick(object? sender, EventArgs e)
        {
            ScrollToCaretTimer.Stop();
            base.ScrollToCaret();
        }

        /// <summary>Scrolls to the caret immediately during a batch update, or schedules scrolling on the UI timer.</summary>
        public new void ScrollToCaret()
        {
            if (updating > 0)
            {
                ScrollToCaretTimer.Stop();
                base.ScrollToCaret();
            }
            else
            {
                ScrollToCaretTimer.Start();
            }
        }

        #endregion

        #region Zoomfactor

        /// <summary>Clears the text and tracked link and style ranges while preserving the zoom factor.</summary>
        public new void Clear()
        {
            float zoomFactor = ZoomFactor;
            base.Clear();
            plainLinks.Clear();
            characterStyleRanges.Clear();
            RestoreZoomFactor(zoomFactor);
        }

        /// <summary>Synchronizes the current zoom with the native control and applies the supplied factor.</summary>
        /// <param name="zoomFactor">The zoom factor to restore.</param>
        public void RestoreZoomFactor(float zoomFactor)
        {
            _ = ZoomFactor; // Synchronize WinForms' cache with the native RichEdit zoom.
            ZoomFactor = zoomFactor;
        }

        #endregion

        #region Highlighting

        /// <summary>Gets or sets the foreground color of highlighted matches.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color HighlightForeColor { get; set; } = Color.Blue;
        /// <summary>Gets or sets the background color of highlighted matches.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Color HighlightBackColor { get; set; } = Color.Yellow;
        /// <summary>Gets or sets whether highlighting matches case.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool HighlightMatchCase { get; set; }
        /// <summary>Gets or sets whether highlighting permits partial matches.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public bool HighlightPartialMatch { get; set; }
        /// <summary>Gets or sets the word matching mode used for highlighting.</summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Visible)]
        public Utils.WordSearchOptions HighlightOptions { get; set; } = Utils.WordSearchOptions.AllWords;

        /// <summary>Specifies paragraph alignment in the native RichEdit control.</summary>
        public enum TextAlignment
        {
            /// <summary>Aligns text to the left.</summary>
            Left = 1,
            /// <summary>Aligns text to the right.</summary>
            Right = 2,
            /// <summary>Centers text.</summary>
            Center = 3,
            /// <summary>Justifies text between both margins.</summary>
            Justify = 4
        }

        /// <summary>Appends a text run and optionally formats or highlights it.</summary>
        /// <param name="text">The text to append.</param>
        /// <param name="font">The font to use, or null to keep the current formatting.</param>
        /// <param name="color">The text color to use, or null to keep the current formatting.</param>
        /// <param name="highlighted">The search text to highlight within the appended text, or null to skip highlighting.</param>
        /// <param name="characterStyle">The style identifier to track, or zero to leave the run untracked.</param>
        /// <param name="raise">The baseline offset in points. Negative values are treated as zero.</param>
        public void AppendText(string text, Font? font = null, Color? color = null, string? highlighted = null, short characterStyle = 0, float raise = 0)
        {
            // Highlighting temporarily selects matches in previously appended text. Always
            // restore the caret to the end before applying the formatting for the next run.
            SelectionStart = TextLength;
            SelectionLength = 0;

            if (font is not null)
            {
                SelectionFont = font;
            }

            if (color.HasValue)
            {
                SelectionColor = color.Value;
            }
            SelectionCharOffset = (int)Math.Round(Math.Max(raise, 0) * DeviceDpi / 72d);

            int offset = default;

            if (highlighted is not null)
            {
                offset = SelectionStart;
            }

            int start = SelectionStart;
            base.AppendText(text);

            if (!characterStyle.Equals(0) && text.Length != 0)
            {
                AddCharacterStyleRange(start, text.Length, characterStyle);
            }

            if (highlighted is not null)
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

            SelectionStart = TextLength;
            SelectionLength = 0;
            ResetSelectionBackColor();
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

        /// <summary>Starts a batch update that suppresses native redraw and notifications.</summary>
        /// <remarks>Calls can be nested. Pair each call with <see cref="EndUpdate"/>.</remarks>
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

        /// <summary>Ends a batch update, restoring native redraw and notifications after the outermost update.</summary>
        /// <exception cref="InvalidOperationException">There is no matching batch update.</exception>
        public void EndUpdate()
        {
            if (updating == 0)
            {
                throw new InvalidOperationException("EndUpdate must be preceded by BeginUpdate.");
            }

            updating -= 1;
            if (updating > 0)
            {
                return;
            }

            _ = SendMessage(new HandleRef(this, Handle), WM_SETREDRAW, 1, 0);
            _ = SendMessage(new HandleRef(this, Handle), EM_SETEVENTMASK, 0, oldEventMask);
        }

        /// <summary>Gets or sets the paragraph alignment of the current selection.</summary>
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
        private unsafe struct PARAFORMAT
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
            public fixed int rgxTabs[32];
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

        private static int SendMessage(HandleRef hWnd, int msg, int wParam, int lParam)
        {
            return unchecked((int)SendMessagePointer(hWnd, msg, wParam, lParam));
        }

        private static nint SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT lp)
        {
            nint result = SendMessageNative(hWnd.Handle, msg, wParam, ref lp);
            GC.KeepAlive(hWnd.Wrapper);
            return result;
        }

        private static nint SendMessagePointer(HandleRef hWnd, int msg, nint wParam, nint lParam)
        {
            nint result = SendMessageNative(hWnd.Handle, msg, wParam, lParam);
            GC.KeepAlive(hWnd.Wrapper);
            return result;
        }

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial nint SendMessageNative(nint hWnd, int msg, nint wParam, nint lParam);

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
        private static partial nint SendMessageNative(nint hWnd, int msg, nint wParam, ref PARAFORMAT lp);

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

        /// <summary>Finds the tracked character style range containing the supplied text index.</summary>
        /// <param name="index">The zero-based character index.</param>
        /// <param name="start">The zero-based start of the matching range, or zero if no range is found.</param>
        /// <param name="length">The length of the matching range, or zero if no range is found.</param>
        /// <param name="style">The matching style identifier, or zero if no range is found.</param>
        /// <returns><see langword="true"/> if a tracked style range contains the index.</returns>
        public bool TryGetCharacterStyleRange(int index, out int start, out int length, out short style)
        {
            int lower = 0;
            int upper = characterStyleRanges.Count - 1;
            while (lower <= upper)
            {
                int middle = lower + ((upper - lower) / 2);
                CharacterStyleRange range = characterStyleRanges[middle];
                if (index < range.Start)
                {
                    upper = middle - 1;
                }
                else if (index >= range.Start + range.Length)
                {
                    lower = middle + 1;
                }
                else
                {
                    start = range.Start;
                    length = range.Length;
                    style = range.Style;
                    return true;
                }
            }
            start = length = 0;
            style = 0;
            return false;
        }

        private void AddCharacterStyleRange(int start, int length, short style)
        {
            if (characterStyleRanges.Count != 0)
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
        private const uint CFE_UNDERLINE = 4U;
        private const uint CFE_LINK = 32U;
        private const uint CFE_AUTOBACKCOLOR = 0x04000000U;
        private const uint CFM_UNDERLINE = 4U;
        private const uint CFM_LINK = 32U;
        private const uint CFM_BACKCOLOR = 0x04000000U;

        /// <summary>Gets or sets automatic native URL detection. It is disabled initially.</summary>
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

        /// <summary>Restores the automatic background color for the selection.</summary>
        public void ResetSelectionBackColor()
        {
            SetSelectionStyle(CFM_BACKCOLOR, CFE_AUTOBACKCOLOR);
        }

        /// <summary>Appends clickable text using native underlined or manually tracked link handling.</summary>
        /// <param name="text">The text to append.</param>
        /// <param name="font">The font to use, or null to keep the current formatting.</param>
        /// <param name="color">The text color to use, or null to keep the current formatting.</param>
        /// <param name="underlined">Whether to use native underlined link formatting.</param>
        /// <param name="characterStyle">The style identifier to track, or zero to leave the run untracked.</param>
        public void AppendLink(string text, Font? font = null, Color? color = null, bool underlined = true, short characterStyle = 0)
        {
            SelectionStart = TextLength;
            SelectionLength = 0;

            if (font is not null)
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
                SetSelectionStyle(CFM_UNDERLINE, CFE_UNDERLINE);
            }
            else
            {
                SetSelectionStyle(CFM_UNDERLINE, 0U);
                plainLinks.Add(new PlainLink(position, length, text));
            }
            if (!characterStyle.Equals(0) && text.Length != 0)
            {
                AddCharacterStyleRange(position, length, characterStyle);
            }
            Select(position + length, 0);
            SetSelectionStyle(mask, effects);
            ResetSelectionBackColor();
        }

        /// <summary>Removes a range of text while preserving the control's read-only setting.</summary>
        /// <param name="position">The zero-based starting position of the text to remove.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <remarks>Tracked ranges after the deletion shift with the text. Intersecting style ranges shrink, and intersecting plain links are unregistered.</remarks>
        public void RemoveText(int position, int length)
        {
            bool readOnly = ReadOnly;
            try
            {
                ReadOnly = false;
                Select(position, length);
                int removedStart = SelectionStart;
                int previousTextLength = TextLength;
                SelectedText = string.Empty;
                int removedLength = previousTextLength - TextLength;
                if (removedLength > 0)
                {
                    UpdateTrackedRangesAfterRemoval(removedStart, removedLength);
                }
            }
            finally
            {
                ReadOnly = readOnly;
            }
        }

        private void UpdateTrackedRangesAfterRemoval(int removedStart, int removedLength)
        {
            int removedEnd = removedStart + removedLength;
            for (int i = plainLinks.Count - 1; i >= 0; i--)
            {
                PlainLink link = plainLinks[i];
                if (link.Start >= removedEnd)
                {
                    plainLinks[i] = new PlainLink(link.Start - removedLength, link.Length, link.Text);
                }
                else if (link.Start + link.Length > removedStart)
                {
                    plainLinks.RemoveAt(i);
                }
            }

            for (int i = characterStyleRanges.Count - 1; i >= 0; i--)
            {
                CharacterStyleRange range = characterStyleRanges[i];
                int rangeEnd = range.Start + range.Length;
                if (range.Start >= removedEnd)
                {
                    characterStyleRanges[i] = new CharacterStyleRange(range.Start - removedLength, range.Length, range.Style);
                }
                else if (rangeEnd > removedStart)
                {
                    int overlap = Math.Min(rangeEnd, removedEnd) - Math.Max(range.Start, removedStart);
                    int remainingLength = range.Length - overlap;
                    if (remainingLength == 0)
                    {
                        characterStyleRanges.RemoveAt(i);
                    }
                    else
                    {
                        characterStyleRanges[i] = new CharacterStyleRange(Math.Min(range.Start, removedStart), remainingLength, range.Style);
                    }
                }
            }
        }

        /// <summary>Exports the current RTF with named style definitions for matching tracked ranges.</summary>
        /// <param name="styles">The RTF style definitions keyed by tracked style identifier.</param>
        /// <returns>The resulting RTF, or the original RTF if there are no matching styles.</returns>
        public string? GetRtfWithCharacterStyles(IDictionary<short, RtfCharacterStyle> styles)
        {
            if (styles.Count.Equals(0) || characterStyleRanges.Count.Equals(0))
            {
                return Rtf;
            }

            List<KeyValuePair<short, RtfCharacterStyle>> activeStyles = [.. styles.Where(style => characterStyleRanges.Any(range => range.Style.Equals(style.Key)))];
            if (activeStyles.Count.Equals(0))
            {
                return Rtf;
            }

            const string markerPrefix = "BQRTFCHARSTYLE";
            using RichTextBox temporary = new()
            {
                DetectUrls = false,
                Rtf = Rtf
            };

            foreach (CharacterStyleRange range in characterStyleRanges.Where(item => activeStyles.Any(style => style.Key.Equals(item.Style))).OrderByDescending(item => item.Start))
            {
                temporary.Select(range.Start, 1);
                int characterOffset = temporary.SelectionCharOffset;
                temporary.Select(range.Start + range.Length, 0);
                temporary.SelectionCharOffset = characterOffset;
                temporary.SelectedText = markerPrefix + "END" + GetAlphabeticMarkerSuffix(range.Style);
                temporary.Select(range.Start, 0);
                // Put the elevation before the opening marker, outside the style group.
                temporary.SelectionCharOffset = characterOffset;
                temporary.SelectedText = markerPrefix + "START" + GetAlphabeticMarkerSuffix(range.Style);
            }

            string styledRtf = temporary.Rtf
                ?? throw new InvalidOperationException("RichTextBox did not provide RTF content.");
            int fontTableStart = styledRtf.IndexOf(@"{\fonttbl", StringComparison.Ordinal);
            int fontTableEnd = FindRtfGroupEnd(styledRtf, fontTableStart);
            int nextFontIndex = RtfFontIndexRegex()
                .Matches(styledRtf.Substring(fontTableStart, fontTableEnd - fontTableStart + 1))
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
            Dictionary<short, string> directStyleFormatting = [];
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

                _ = styleSheet.Append(@"{\*\cs").Append(style.Key).Append(@"\additive\f").Append(fontIndex).Append(@"\fs").Append((int)Math.Round(style.Value.Font.SizeInPoints * 2.0f)).Append(@"\cf").Append(colorIndex);
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
                _ = styleSheet.Append(' ').Append(EscapeRtf(style.Value.Name)).Append(";}");

                StringBuilder directFormatting = new();
                _ = directFormatting.Append(@"\fs").Append((int)Math.Round(style.Value.Font.SizeInPoints * 2.0f)).Append(@"\cf").Append(colorIndex);
                if (style.Value.Font.Bold)
                {
                    _ = directFormatting.Append(@"\b");
                }
                if (style.Value.Font.Italic)
                {
                    _ = directFormatting.Append(@"\i");
                }
                if (style.Value.Font.Underline)
                {
                    _ = directFormatting.Append(@"\ul");
                }
                if (style.Value.Font.Strikeout)
                {
                    _ = directFormatting.Append(@"\strike");
                }
                directStyleFormatting.Add(style.Key, directFormatting.ToString());
            }
            _ = styleSheet.Append('}');

            foreach (KeyValuePair<short, RtfCharacterStyle> style in activeStyles)
            {
                string markerSuffix = GetAlphabeticMarkerSuffix(style.Key);
                styledRtf = styledRtf
                    .Replace(markerPrefix + "START" + markerSuffix, @"{\cs" + style.Key + directStyleFormatting[style.Key] + " ")
                    .Replace(markerPrefix + "END" + markerSuffix, "}");
            }

            colorTableStart = styledRtf.IndexOf(@"{\colortbl", StringComparison.Ordinal);
            colorTableEnd = FindRtfGroupEnd(styledRtf, colorTableStart);
            int insertionIndex = colorTableEnd + 1;
            if (colorTableStart.Equals(-1) || colorTableEnd.Equals(-1))
            {
                int fontTablePosition = styledRtf.IndexOf(@"{\fonttbl", StringComparison.Ordinal);
                int fontTablePositionEnd = FindRtfGroupEnd(styledRtf, fontTablePosition);
                insertionIndex = fontTablePositionEnd >= 0 ? fontTablePositionEnd + 1 : styledRtf.IndexOf(' ');
            }
            return styledRtf.Insert(insertionIndex, styleSheet.ToString());
        }

        private static string GetAlphabeticMarkerSuffix(short value)
        {
            // Keep the entire marker alphabetic so RichEdit does not insert bidi controls
            // between the marker and a numeric suffix next to right-to-left text.
            uint markerValue = (ushort)value;
            Span<char> suffix = stackalloc char[4];
            for (int index = suffix.Length - 1; index >= 0; index--)
            {
                suffix[index] = (char)('A' + (markerValue & 0xF));
                markerValue >>= 4;
            }
            return new string(suffix);
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

        /// <inheritdoc/>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!e.Button.Equals(MouseButtons.Left))
            {
                return;
            }

            PlainLink? link = GetPlainLink(e.Location);
            if (link is null)
            {
                Cursor = Cursors.IBeam;
                return;
            }

            Cursor = Cursors.Hand;
            try
            {
                OnLinkClicked(new LinkClickedEventArgs(link.Text));
            }
            finally
            {
                Cursor = Cursors.IBeam;
            }
        }

        private PlainLink? GetPlainLink(Point location)
        {
            int index = GetCharIndexFromPosition(location);
            int lower = 0;
            int upper = plainLinks.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + ((upper - lower) / 2);
                PlainLink link = plainLinks[middle];

                if (index < link.Start)
                {
                    upper = middle - 1;
                }
                else if (index >= link.Start + link.Length)
                {
                    lower = middle + 1;
                }
                else
                {
                    return link;
                }
            }

            return null;
        }

        #endregion

    }
}
