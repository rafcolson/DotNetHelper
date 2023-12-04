namespace WinFormsLib
{
    public static class ListControlExtensions
    {
        public static bool ContainsItem(this ListControl super, object item)
        {
            return super is ComboBox cb ? cb.Items.Contains(item) : super is ListBox lb && lb.Items.Contains(item);
        }

        public static int GetItemsCount(this ListControl super)
        {
            return super is ComboBox cb ? cb.Items.Count : super is ListBox lb ? lb.Items.Count : 0;
        }

        public static T? GetItemAt<T>(this ListControl super, int index)
        {
            return super is ComboBox cb ? (T)cb.Items[index] : super is ListBox lb ? (T)lb.Items[index] : (T?)(object?)null;
        }

        public static T[] GetItems<T>(this ListControl super)
        {
            return super is ComboBox cb ? cb.Items.Cast<T>().ToArray() : super is ListBox lb ? lb.Items.Cast<T>().ToArray() : Array.Empty<T>();
        }

        public static T[] TakeItems<T>(this ListControl super, Range range) => super.GetItems<T>().Take(range).ToArray();

        public static void RemoveItem(this ListControl super, object item)
        {
            if (super is ComboBox comboBox)
            {
                comboBox.Items.Remove(item);
            }
            else if (super is ListBox listBox)
            {
                listBox.Items.Remove(item);
            }
        }

        public static T? RemoveItemAt<T>(this ListControl super, int index)
        {
            T? item = super.GetItemAt<T>(index);
            if (super is ComboBox comboBox)
            {
                comboBox.Items.RemoveAt(index);
            }
            else if (super is ListBox listBox)
            {
                listBox.Items.RemoveAt(index);
            }
            return item;
        }

        public static void RemoveItems(this ListControl super, object[] items)
        {
            foreach (object item in items)
            {
                super.RemoveItem(item);
            }
        }

        public static T[] RemoveItems<T>(this ListControl super, Range range)
        {
            T[] items = super.TakeItems<T>(range);
            super.RemoveItems(items.Cast<object>().ToArray());
            return items;
        }

        public static void ClearItems(this ListControl super)
        {
            if (super is ComboBox comboBox)
            {
                comboBox.Items.Clear();
            }
            else if (super is ListBox listBox)
            {
                listBox.Items.Clear();
            }
        }

        public static void AddItem(this ListControl super, object item)
        {
            if (super is ComboBox comboBox)
            {
                comboBox.Items.Add(item);
            }
            else if (super is ListBox listBox)
            {
                listBox.Items.Add(item);
            }
        }

        public static void InsertItem(this ListControl super, int index, object item)
        {
            if (super is ComboBox comboBox)
            {
                comboBox.Items.Insert(index, item);
            }
            else if (super is ListBox listBox)
            {
                listBox.Items.Insert(index, item);
            }
        }

        public static void AddItems(this ListControl super, object[] items)
        {
            if (super is ComboBox comboBox)
            {
                comboBox.Items.AddRange(items);
            }
            else if (super is ListBox listBox)
            {
                listBox.Items.AddRange(items);
            }
        }
    }
}
