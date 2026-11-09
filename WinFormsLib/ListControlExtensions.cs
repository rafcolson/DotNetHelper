namespace WinFormsLib
{
    /// <summary>Provides item operations for combo boxes and list boxes through <see cref="ListControl"/>.</summary>
    /// <remarks>Other control types are treated as empty, and item mutations have no effect on them.</remarks>
    public static class ListControlExtensions
    {
        /// <summary>Determines whether the control contains the supplied item.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="item">The item to find, remove, or insert.</param>
        public static bool ContainsItem(this ListControl super, object item)
        {
            return super is ComboBox cb ? cb.Items.Contains(item) : super is ListBox lb && lb.Items.Contains(item);
        }

        /// <summary>Returns the number of items, or zero for an unsupported control type.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        public static int GetItemsCount(this ListControl super)
        {
            return super is ComboBox cb ? cb.Items.Count : super is ListBox lb ? lb.Items.Count : 0;
        }

        /// <summary>Gets the item at the specified index, or the default value if the item has a different type or the control is unsupported.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="index">The zero-based item index.</param>
        public static T? GetItemAt<T>(this ListControl super, int index)
        {
            return super switch
            {
                ComboBox cb => cb.Items[index] is T value ? value : default,
                _ => super is ListBox lb ? lb.Items[index] is T value ? value : default : default
            };
        }

        /// <summary>Copies the items into a typed array in control order.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="super">The control whose items are accessed.</param>
        /// <exception cref="InvalidCastException">An item in the control cannot be cast to <typeparamref name="T"/>.</exception>
        public static T[] GetItems<T>(this ListControl super)
        {
            return super is ComboBox cb ? [.. cb.Items.Cast<T>()] : super is ListBox lb ? [.. lb.Items.Cast<T>()] : [];
        }

        /// <summary>Copies the items in the specified range into a typed array.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="range">The range of item positions to select.</param>
        /// <exception cref="InvalidCastException">An item in the control cannot be cast to <typeparamref name="T"/>.</exception>
        public static T[] TakeItems<T>(this ListControl super, Range range) => [.. super.GetItems<T>().Take(range)];

        /// <summary>Removes the supplied item by value.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="item">The item to find, remove, or insert.</param>
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

        /// <summary>Removes the item at the specified index and returns it if it has the requested type.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="index">The zero-based item index.</param>
        /// <returns>The removed item, or the default value if it has a different type or the control is unsupported.</returns>
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

        /// <summary>Removes the supplied items one at a time by value.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="items">The items to add or remove.</param>
        public static void RemoveItems(this ListControl super, object[] items)
        {
            foreach (object item in items)
            {
                super.RemoveItem(item);
            }
        }

        /// <summary>Removes the items at the specified positions and returns them in their original order.</summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="range">The range of item positions to select.</param>
        /// <exception cref="InvalidCastException">An item in the control cannot be cast to <typeparamref name="T"/>.</exception>
        public static T[] RemoveItems<T>(this ListControl super, Range range)
        {
            T[] items = super.TakeItems<T>(range);
            int startIndex = Math.Max(0, range.Start.GetOffset(super.GetItemsCount()));
            for (int offset = items.Length - 1; offset >= 0; offset--)
            {
                _ = super.RemoveItemAt<T>(startIndex + offset);
            }
            return items;
        }

        /// <summary>Removes all items from the control.</summary>
        /// <param name="super">The control whose items are accessed.</param>
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

        /// <summary>Adds the supplied item to the control.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="item">The item to find, remove, or insert.</param>
        public static void AddItem(this ListControl super, object item)
        {
            if (super is ComboBox comboBox)
            {
                _ = comboBox.Items.Add(item);
            }
            else if (super is ListBox listBox)
            {
                _ = listBox.Items.Add(item);
            }
        }

        /// <summary>Inserts the supplied item at the specified index.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="index">The zero-based item index.</param>
        /// <param name="item">The item to find, remove, or insert.</param>
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

        /// <summary>Adds the supplied items to the control.</summary>
        /// <param name="super">The control whose items are accessed.</param>
        /// <param name="items">The items to add or remove.</param>
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
