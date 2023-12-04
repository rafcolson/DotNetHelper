using static WinFormsLib.Constants;
using static WinFormsLib.Utils;

namespace WinFormsLib
{
    public static class Buttons
    {
        [Flags]
        public enum DialogResultFlag
        {
            [Value(DialogResult.None)]
            None = 0,
            [Value(DialogResult.OK)]
            [GlobalStringValue("OK")]
            OK = 1 << 0,
            [Value(DialogResult.Yes)]
            [GlobalStringValue("Yes")]
            Yes = 1 << 1,
            [Value(DialogResult.Retry)]
            [GlobalStringValue("Retry")]
            Retry = 1 << 2,
            [Value(DialogResult.Ignore)]
            [GlobalStringValue("Ignore")]
            Ignore = 1 << 3,
            [Value(DialogResult.Continue)]
            [GlobalStringValue("Continue")]
            Continue = 1 << 4,
            [Value(DialogResult.Cancel)]
            [GlobalStringValue("Cancel")]
            Cancel = 1 << 5,
            [Value(DialogResult.Abort)]
            [GlobalStringValue("Abort")]
            Abort = 1 << 6,
            [Value(DialogResult.No)]
            [GlobalStringValue("No")]
            No = 1 << 7
        }

        [Flags]
        public enum DialogResultFlags
        {
            [Value(MessageBoxButtons.OK)]
            OK = DialogResultFlag.OK,
            [Value(MessageBoxButtons.OKCancel)]
            OKCancel = DialogResultFlag.OK | DialogResultFlag.Cancel,
            [Value(MessageBoxButtons.AbortRetryIgnore)]
            AbortRetryIgnore = DialogResultFlag.Ignore | DialogResultFlag.Retry | DialogResultFlag.Abort,
            [Value(MessageBoxButtons.YesNoCancel)]
            YesNoCancel = DialogResultFlag.Yes | DialogResultFlag.No | DialogResultFlag.Cancel,
            [Value(MessageBoxButtons.YesNo)]
            YesNo = DialogResultFlag.Yes | DialogResultFlag.No,
            [Value(MessageBoxButtons.RetryCancel)]
            RetryCancel = DialogResultFlag.Retry | DialogResultFlag.Cancel,
            [Value(MessageBoxButtons.CancelTryContinue)]
            CancelTryContinue = DialogResultFlag.Continue | DialogResultFlag.Retry | DialogResultFlag.Cancel
        }

        [Flags]
        public enum EditButton
        {
            None = 0,
            [GlobalStringValue("Add")]
            Add = 1 << 0,
            [GlobalStringValue("Remove")]
            Remove = 1 << 1,
            [GlobalStringValue("Rename")]
            Rename = 1 << 2,
            [GlobalStringValue("MoveUp")]
            MoveUp = 1 << 3,
            [GlobalStringValue("MoveDown")]
            MoveDown = 1 << 4,
            [GlobalStringValue("Sort")]
            Sort = 1 << 5,
            [GlobalStringValue("Edit")]
            Edit = 1 << 6,
            [GlobalStringValue("Move")]
            Move = 1 << 7
        }

        [Flags]
        public enum EditButtons
        {
            AddRemove = EditButton.Add | EditButton.Remove,
            AddRemoveRename = EditButton.Add | EditButton.Remove | EditButton.Rename,
            AddRemoveEdit = EditButton.Add | EditButton.Remove | EditButton.Edit,
            AddRemoveMoveUpMoveDown = EditButton.Add | EditButton.Remove | EditButton.MoveUp | EditButton.MoveDown,
            AddRemoveMoveUpMoveDownSort = EditButton.Add | EditButton.Remove | EditButton.MoveUp | EditButton.MoveDown | EditButton.Sort,
            AddRemoveRenameMoveUpMoveDownSort = EditButton.Add | EditButton.Remove | EditButton.Rename | EditButton.MoveUp | EditButton.MoveDown | EditButton.Sort,
            AddRemoveMoveUpMoveDownSortEdit = EditButton.Add | EditButton.Remove | EditButton.MoveUp | EditButton.MoveDown | EditButton.Sort | EditButton.Edit
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
                Margin = (Padding)margin,
                MinimumSize = BUTTON_SIZE_MINIMUM,
                Size = GetMaxSize(new[] { globalStringValue }, font, PADDING_MINIMUM, BUTTON_SIZE_MINIMUM),
                Text = globalStringValue
            };
            button.Disposed += (object? sender, EventArgs e) => { button.ClearEventHandlers(); };
            return button;
        }

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
                DialogResultFlag acceptFlags = DialogResultFlag.OK | DialogResultFlag.Yes | DialogResultFlag.Retry;
                DialogResultFlag cancelFlags = DialogResultFlag.Cancel | DialogResultFlag.No | DialogResultFlag.Abort;
                if (acceptFlags.HasFlag(dialogResultFlag))
                {
                    form.AcceptButton = button;
                }
                else if (cancelFlags.HasFlag(dialogResultFlag))
                {
                    form.CancelButton = button;
                }
            }

            public static Button? AddButton(Control control, DialogResultFlag dialogResultFlag)
            {
                if (GetButton(dialogResultFlag.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                {
                    Form form = control.FindForm();
                    SetAcceptOrCancelButton(button, dialogResultFlag, form);
                    DialogResult dialogResult = dialogResultFlag.GetValue<DialogResult>();
                    button.DialogResult = dialogResult;
                    button.Click += (object? sender, EventArgs e) => { form.DialogResult = dialogResult; };
                    control.Controls.Add(button);
                    return button;
                }
                return null;
            }

            public static Button? AddButton(Control control, DialogResult dialogResult) => AddButton(control, GetDialogResultFlag(dialogResult));

            public static Button[] AddButtons(Control control, DialogResultFlags dialogResultFlags)
            {
                Form form = control.FindForm();
                List<Button> buttons = new();
                foreach (DialogResultFlag dialogResultFlag in dialogResultFlags.ToEnum<DialogResultFlag>().GetContainingFlags())
                {
                    if (GetButton(dialogResultFlag.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                    {
                        SetAcceptOrCancelButton(button, dialogResultFlag, form);
                        DialogResult dialogResult = dialogResultFlag.GetValue<DialogResult>();
                        button.DialogResult = dialogResult;
                        button.Click += (object? sender, EventArgs e) => { form.DialogResult = dialogResult; };
                        buttons.Add(button);
                    }
                }
                Size size = GetMaxSize(buttons.Select(x => x.Text).ToArray(), control.Font, PADDING_MINIMUM, BUTTON_SIZE_MINIMUM);
                foreach (Button button in buttons)
                {
                    button.Size = size;
                    control.Controls.Add(button);
                }
                return buttons.ToArray();
            }

            public static Button[] AddButtons(Control control, MessageBoxButtons messageBoxButtons) => AddButtons(control, GetDialogResultFlags(messageBoxButtons));
        }

        public static class DialogEditButtons
        {
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

            public static Button[] AddButtons(Control control, EditButtons editButtons)
            {
                List<Button> buttons = new();
                foreach (EditButton editButton in editButtons.ToEnum<EditButton>().GetContainingFlags())
                {
                    if (GetButton(editButton.GetGlobalStringValue(), control.Font, control.Margin) is Button button)
                    {
                        button.Tag = editButton;
                        buttons.Add(button);
                    }
                }
                Size size = GetMaxSize(buttons.Select(x => x.Text).ToArray(), control.Font, PADDING_MINIMUM, BUTTON_SIZE_MINIMUM);
                foreach (Button button in buttons)
                {
                    button.Size = size;
                    control.Controls.Add(button);
                }
                return buttons.ToArray();
            }
        }
    }
}
