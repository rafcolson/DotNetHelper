namespace WinFormsLib
{
    /// <summary>Provides layout and item creation helpers for data grids.</summary>
    public static class DataGridViewExtensions
    {
        /// <summary>Sets the row header width to fit the widest header text plus a DPI-scaled margin.</summary>
        /// <param name="super">The grid to resize.</param>
        /// <param name="deviceDpi">The device DPI used to scale the margin.</param>
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

        /// <summary>Adds a text column and returns it.</summary>
        /// <param name="super">The destination grid.</param>
        /// <param name="columnName">The name used to identify the column.</param>
        /// <param name="headerText">The text displayed in the column header.</param>
        public static DataGridViewColumn AddColumn(this DataGridView super, string columnName, string headerText) => super.Columns[super.Columns.Add(columnName, headerText)];

        /// <summary>Adds a row and returns it.</summary>
        /// <param name="super">The destination grid.</param>
        /// <param name="values">The row values. If row headers are visible, the first value is used as the header and the rest populate the cells.</param>
        /// <exception cref="ArgumentOutOfRangeException">No values are supplied.</exception>
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
