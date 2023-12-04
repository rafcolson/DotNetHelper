namespace WinFormsLib
{
    public static class DataGridViewExtensions
    {
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
                int i = n == 1 ? super.Rows.Add() : super.Rows.Add(values.Skip(1).ToArray());
                row = super.Rows[i];
                row.HeaderCell.Value = values[0];
            }
            return row;
        }
    }
}
