namespace WinFormsLib
{
    public static class DataGridViewExtensions
    {
        public static void ResizeRowHeaders(this DataGridView super, int deviceDpi)
        {
            int textWidth = 0;
            foreach (DataGridViewRow row in super.Rows)
            {
                if (row.HeaderCell.Value?.ToString() is not string value)
                {
                    continue;
                }
                Font font = row.HeaderCell.InheritedStyle.Font ?? super.Font;
                int width = TextRenderer.MeasureText(value, font).Width;
                textWidth = Math.Max(textWidth, width);
            }

            int margin = (int)Math.Ceiling(35F * deviceDpi / 96F);
            super.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            super.RowHeadersWidth = textWidth + margin;
        }

        public static DataGridViewColumn AddColumn(this DataGridView super, string columnName, string headerText) => super.Columns[super.Columns.Add(columnName, headerText)];

        public static DataGridViewRow AddRow(this DataGridView super, params object[] values)
        {
            int n = values.Length;
            if (n == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }
            DataGridViewRow row;
            if (!super.RowHeadersVisible)
            {
                row = super.Rows[super.Rows.Add(values)];
            }
            else
            {
                int i = n == 1 ? super.Rows.Add() : super.Rows.Add([.. values.Skip(1)]);
                row = super.Rows[i];
                row.HeaderCell.Value = values[0];
                row.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            }
            return row;
        }
    }
}
