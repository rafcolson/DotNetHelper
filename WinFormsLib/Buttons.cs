using static WinFormsLib.Constants;
using static WinFormsLib.Utils;

namespace WinFormsLib
{
    /// <summary>Creates localized dialog and editing buttons for Windows Forms.</summary>
    public static class Buttons
    {
        /// <summary>Identifies individual dialog buttons that can be combined.</summary>
        [Flags]
        public enum DialogResultFlag
        {
            /// <summary>Specifies no button.</summary>
            [Value(DialogResult.None)]
            None = 0,
            /// <summary>Displays OK and returns the corresponding dialog result.</summary>
            [Value(DialogResult.OK)]
            [GlobalStringValue("OK")]
            OK = 1 << 0,
            /// <summary>Displays Yes and returns the corresponding dialog result.</summary>
            [Value(DialogResult.Yes)]
            [GlobalStringValue("Yes")]
            Yes = 1 << 1,
            /// <summary>Displays Retry and returns the corresponding dialog result.</summary>
            [Value(DialogResult.Retry)]
            [GlobalStringValue("Retry")]
            Retry = 1 << 2,
            /// <summary>Displays Ignore and returns the corresponding dialog result.</summary>
            [Value(DialogResult.Ignore)]
            [GlobalStringValue("Ignore")]
            Ignore = 1 << 3,
            /// <summary>Displays Continue and returns the corresponding dialog result.</summary>
            [Value(DialogResult.Continue)]
            [GlobalStringValue("Continue")]
            Continue = 1 << 4,
            /// <summary>Displays Cancel and returns the corresponding dialog result.</summary>
            [Value(DialogResult.Cancel)]
            [GlobalStringValue("Cancel")]
            Cancel = 1 << 5,
            /// <summary>Displays Abort and returns the corresponding dialog result.</summary>
            [Value(DialogResult.Abort)]
            [GlobalStringValue("Abort")]
            Abort = 1 << 6,
            /// <summary>Displays No and returns the corresponding dialog result.</summary>
            [Value(DialogResult.No)]
            [GlobalStringValue("No")]
            No = 1 << 7,
            /// <summary>Displays Close and returns the OK dialog result.</summary>
            [Value(DialogResult.OK)]
            [GlobalStringValue("Close")]
            Close = 1 << 8
        }

        /// <summary>Defines groups of dialog result buttons.</summary>
        [Flags]
        public enum DialogResultFlags
        {
            /// <summary>Includes OK.</summary>
            [Value(MessageBoxButtons.OK)]
            OK = DialogResultFlag.OK,
            /// <summary>Includes OK and Cancel.</summary>
            [Value(MessageBoxButtons.OKCancel)]
            OKCancel = DialogResultFlag.OK | DialogResultFlag.Cancel,
            /// <summary>Includes Abort, Retry, and Ignore.</summary>
            [Value(MessageBoxButtons.AbortRetryIgnore)]
            AbortRetryIgnore = DialogResultFlag.Ignore | DialogResultFlag.Retry | DialogResultFlag.Abort,
            /// <summary>Includes Yes, No, and Cancel.</summary>
            [Value(MessageBoxButtons.YesNoCancel)]
            YesNoCancel = DialogResultFlag.Yes | DialogResultFlag.No | DialogResultFlag.Cancel,
            /// <summary>Includes Yes and No.</summary>
            [Value(MessageBoxButtons.YesNo)]
            YesNo = DialogResultFlag.Yes | DialogResultFlag.No,
            /// <summary>Includes Retry and Cancel.</summary>
            [Value(MessageBoxButtons.RetryCancel)]
            RetryCancel = DialogResultFlag.Retry | DialogResultFlag.Cancel,
            /// <summary>Includes Cancel, Retry, and Continue.</summary>
            [Value(MessageBoxButtons.CancelTryContinue)]
            CancelTryContinue = DialogResultFlag.Continue | DialogResultFlag.Retry | DialogResultFlag.Cancel,
            /// <summary>Includes Close.</summary>
            Close = DialogResultFlag.Close
        }

        /// <summary>Identifies localized captions for common commands.</summary>
        public enum CommandButton
        {
            // File operations
            /// <summary>Represents the New command.</summary>
            [GlobalStringValue("New")]
            New,
            /// <summary>Represents the Open command.</summary>
            [GlobalStringValue("Open")]
            Open,
            /// <summary>Represents the Save command.</summary>
            [GlobalStringValue("Save")]
            Save,
            /// <summary>Represents the Save As command.</summary>
            [GlobalStringValue("SaveAs")]
            SaveAs,
            /// <summary>Represents the Import command.</summary>
            [GlobalStringValue("Import")]
            Import,
            /// <summary>Represents the Export command.</summary>
            [GlobalStringValue("Export")]
            Export,

            // Selection and customization
            /// <summary>Represents the Select command.</summary>
            [GlobalStringValue("Select")]
            Select,
            /// <summary>Represents the Browse command.</summary>
            [GlobalStringValue("Browse")]
            Browse,
            /// <summary>Represents the Custom command.</summary>
            [GlobalStringValue("Custom")]
            Custom,

            // Applying and restoring
            /// <summary>Represents the Apply command.</summary>
            [GlobalStringValue("Apply")]
            Apply,
            /// <summary>Represents the Refresh command.</summary>
            [GlobalStringValue("Refresh")]
            Refresh,
            /// <summary>Represents the Reset command.</summary>
            [GlobalStringValue("Reset")]
            Reset,
            /// <summary>Represents the Restore Defaults command.</summary>
            [GlobalStringValue("RestoreDefaults")]
            RestoreDefaults,
            /// <summary>Represents the Clear command.</summary>
            [GlobalStringValue("Clear")]
            Clear,

            // Information
            /// <summary>Represents the Information command.</summary>
            [GlobalStringValue("Info")]
            Info
        }

        /// <summary>Identifies individual editing commands that can be combined.</summary>
        [Flags]
        public enum EditButton
        {
            /// <summary>Specifies no button.</summary>
            None = 0,
            /// <summary>Represents the Add command.</summary>
            [GlobalStringValue("Add")]
            Add = 1 << 0,
            /// <summary>Represents the Remove command.</summary>
            [GlobalStringValue("Remove")]
            Remove = 1 << 1,
            /// <summary>Represents the Rename command.</summary>
            [GlobalStringValue("Rename")]
            Rename = 1 << 2,
            /// <summary>Represents the Move Up command.</summary>
            [GlobalStringValue("MoveUp")]
            MoveUp = 1 << 3,
            /// <summary>Represents the Move Down command.</summary>
            [GlobalStringValue("MoveDown")]
            MoveDown = 1 << 4,
            /// <summary>Represents the Sort command.</summary>
            [GlobalStringValue("Sort")]
            Sort = 1 << 5,
            /// <summary>Represents the Edit command.</summary>
            [GlobalStringValue("Edit")]
            Edit = 1 << 6,
            /// <summary>Represents the Move command.</summary>
            [GlobalStringValue("Move")]
            Move = 1 << 7,
            /// <summary>Represents the Clear command.</summary>
            [GlobalStringValue("Clear")]
            Clear = 1 << 8
        }

        /// <summary>Defines preset groups of editing commands.</summary>
        [Flags]
        public enum EditButtons
        {
            /// <summary>Includes Add, Remove.</summary>
            AddRemove = EditButton.Add | EditButton.Remove,
            /// <summary>Includes Add, Remove, Rename.</summary>
            AddRemoveRename = EditButton.Add | EditButton.Remove | EditButton.Rename,
            /// <summary>Includes Add, Remove, Edit.</summary>
            AddRemoveEdit = EditButton.Add | EditButton.Remove | EditButton.Edit,
            /// <summary>Includes Add, Remove, Move Up, Move Down.</summary>
            AddRemoveMoveUpMoveDown = EditButton.Add | EditButton.Remove | EditButton.MoveUp | EditButton.MoveDown,
            /// <summary>Includes Add, Remove, Move Up, Move Down, Sort.</summary>
            AddRemoveMoveUpMoveDownSort = EditButton.Add | EditButton.Remove | EditButton.MoveUp | EditButton.MoveDown | EditButton.Sort,
            /// <summary>Includes Add, Remove, Rename, Move Up, Move Down, Sort.</summary>
            AddRemoveRenameMoveUpMoveDownSort = EditButton.Add | EditButton.Remove | EditButton.Rename | EditButton.MoveUp | EditButton.MoveDown | EditButton.Sort,
            /// <summary>Includes Add, Remove, Move Up, Move Down, Sort, Edit.</summary>
            AddRemoveMoveUpMoveDownSortEdit = EditButton.Add | EditButton.Remove | EditButton.MoveUp | EditButton.MoveDown | EditButton.Sort | EditButton.Edit,
            /// <summary>Includes Remove, Clear.</summary>
            RemoveClear = EditButton.Remove | EditButton.Clear
        }

        private static Button? GetButton(string globalStringValue, Font? font = null, Padding? margin = null)
        {
            if (string.IsNullOrEmpty(globalStringValue))
            {
                return null;
            }
            font ??= FONT_DEFAULT;
            margin ??= PADDING_MINIMUM;
            Button button = new()
            {
                Name = $"Button{globalStringValue}",
                Font = font,
                Margin = (Padding)margin,
                MinimumSize = BUTTON_SIZE_MINIMUM,
                Size = GetMaxSize([globalStringValue], font, PADDING_MINIMUM, BUTTON_SIZE_MINIMUM),
                Text = globalStringValue
            };
            button.Disposed += (sender, e) => { button.ClearEventHandlers(); };
            return button;
        }

        /// <summary>Adds localized buttons that return a result from their containing form.</summary>
        public static class DialogResultButtons
        {
            private static DialogResultFlag GetDialogResultFlag(DialogResult dialogResult)
            {
                DialogResultFlag flag = DialogResultFlag.None;
                foreach (DialogResultFlag value in Enum.GetValues<DialogResultFlag>())
                {
                    if (value.GetValue<object>() is DialogResult dr && dr == dialogResult)
                    {
                        flag = value;
                        break;
                    }
                }
                return flag;
            }

            private static DialogResultFlags GetDialogResultFlags(MessageBoxButtons messageBoxButtons)
            {
                DialogResultFlags flag = DialogResultFlags.OK;
                foreach (DialogResultFlags value in Enum.GetValues<DialogResultFlags>())
                {
                    if (value.GetValue<object>() is MessageBoxButtons mbb && mbb == messageBoxButtons)
                    {
                        flag = value;
                        break;
                    }
                }
                return flag;
            }

            private static void SetAcceptOrCancelButton(Button button, DialogResultFlag dialogResultFlag, Form form)
            {
                DialogResultFlag acceptFlags = DialogResultFlag.OK | DialogResultFlag.Yes | DialogResultFlag.Retry | DialogResultFlag.Close;
                DialogResultFlag cancelFlags = DialogResultFlag.Cancel | DialogResultFlag.No | DialogResultFlag.Abort | DialogResultFlag.Close;
                if (acceptFlags.HasFlag(dialogResultFlag))
                {
                    form.AcceptButton = button;
                }
                if (cancelFlags.HasFlag(dialogResultFlag))
                {
                    form.CancelButton = button;
                }
            }

            /// <summary>Adds a localized button for dialog results.</summary>
            /// <param name="control">The container to receive the button.</param>
            /// <param name="dialogResultFlag">The button to add.</param>
            /// <returns>The added button, or null if no button can be created or the container has no form.</returns>
            public static Button? AddButton(Control control, DialogResultFlag dialogResultFlag)
            {
                if (GetButton(dialogResultFlag.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                {
                    if (control.FindForm() is Form form)
                    {
                        SetAcceptOrCancelButton(button, dialogResultFlag, form);
                        DialogResult dialogResult = dialogResultFlag.GetValue<DialogResult>();
                        button.DialogResult = dialogResult;
                        button.Click += (sender, e) => { form.DialogResult = dialogResult; };
                        control.Controls.Add(button);
                        return button;
                    }
                }
                return null;
            }

            /// <summary>Adds a localized button for dialog results.</summary>
            /// <param name="control">The container to receive the button.</param>
            /// <param name="dialogResult">The button to add.</param>
            /// <returns>The added button, or null if no button can be created or the container has no form.</returns>
            public static Button? AddButton(Control control, DialogResult dialogResult) => AddButton(control, GetDialogResultFlag(dialogResult));

            /// <summary>Adds localized buttons of a common size for dialog results.</summary>
            /// <param name="control">The container to receive the buttons.</param>
            /// <param name="dialogResultFlags">The button group to add.</param>
            /// <returns>The buttons added, or an empty array if the container has no form.</returns>
            public static Button[] AddButtons(Control control, DialogResultFlags dialogResultFlags)
            {
                List<Button> buttons = [];
                if (control.FindForm() is Form form)
                {
                    foreach (DialogResultFlag dialogResultFlag in dialogResultFlags.ToEnum<DialogResultFlag>().GetContainingFlags())
                    {
                        if (GetButton(dialogResultFlag.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                        {
                            SetAcceptOrCancelButton(button, dialogResultFlag, form);
                            DialogResult dialogResult = dialogResultFlag.GetValue<DialogResult>();
                            button.DialogResult = dialogResult;
                            button.Click += (sender, e) => { form.DialogResult = dialogResult; };
                            buttons.Add(button);
                        }
                    }
                    Size size = GetMaxSize([.. buttons.Select(x => x.Text)], control.Font, PADDING_MINIMUM, BUTTON_SIZE_MINIMUM);
                    foreach (Button button in buttons)
                    {
                        button.Size = size;
                        control.Controls.Add(button);
                    }
                }
                return [.. buttons];
            }

            /// <summary>Adds localized buttons of a common size for dialog results.</summary>
            /// <param name="control">The container to receive the buttons.</param>
            /// <param name="messageBoxButtons">The button group to add.</param>
            /// <returns>The buttons added, or an empty array if the container has no form.</returns>
            public static Button[] AddButtons(Control control, MessageBoxButtons messageBoxButtons) => AddButtons(control, GetDialogResultFlags(messageBoxButtons));
        }

        /// <summary>Adds localized editing buttons with their command stored in the button tag.</summary>
        public static class DialogEditButtons
        {
            /// <summary>Adds a localized button for editing commands.</summary>
            /// <param name="control">The container to receive the button.</param>
            /// <param name="editButton">The button to add.</param>
            /// <returns>The added button, or null if no button can be created.</returns>
            /// <remarks>The caller supplies the editing behavior.</remarks>
            public static Button? AddButton(Control control, EditButton editButton)
            {
                if (GetButton(editButton.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                {
                    button.Tag = editButton;
                    control.Controls.Add(button);
                    return button;
                }
                return null;
            }

            /// <summary>Adds localized buttons of a common size for editing commands.</summary>
            /// <param name="control">The container to receive the buttons.</param>
            /// <param name="editButtons">The button group to add.</param>
            /// <returns>The buttons added.</returns>
            /// <remarks>The caller supplies the editing behavior.</remarks>
            public static Button[] AddButtons(Control control, EditButtons editButtons)
            {
                List<Button> buttons = [];
                foreach (EditButton editButton in editButtons.ToEnum<EditButton>().GetContainingFlags())
                {
                    if (GetButton(editButton.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                    {
                        button.Tag = editButton;
                        buttons.Add(button);
                    }
                }
                Size size = GetMaxSize([.. buttons.Select(x => x.Text)], control.Font, PADDING_MINIMUM, BUTTON_SIZE_MINIMUM);
                foreach (Button button in buttons)
                {
                    button.Size = size;
                    control.Controls.Add(button);
                }
                return [.. buttons];
            }
        }
    }
}
