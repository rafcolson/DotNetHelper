using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WinFormsLib
{
    /// <summary>Provides helpers for populating and opening links in a Windows Forms label.</summary>
    public static class LinkLabelExtensions
    {
        private static readonly ConditionalWeakTable<LinkLabel, LinkLabelLinkClickedEventHandler> LinkClickHandlers = [];

        /// <summary>Replaces the label's links and updates the handler that opens their destinations through the Windows Shell.</summary>
        /// <param name="super">The label to update.</param>
        /// <param name="links">The links to add. If null, links are extracted from the label's text.</param>
        /// <param name="linkClickedAction">An optional callback invoked after the Shell launch call returns for a nonempty link destination.</param>
        /// <remarks>Each call replaces the handler installed by the previous call. Only links with nonempty string link data are opened.</remarks>
        public static void UpdateLinks(this LinkLabel super, LinkLabel.Link[]? links = null, Action? linkClickedAction = null)
        {
            if (LinkClickHandlers.TryGetValue(super, out LinkLabelLinkClickedEventHandler? previousHandler))
            {
                super.LinkClicked -= previousHandler;
                _ = LinkClickHandlers.Remove(super);
            }

            super.Links.Clear();
            links ??= Utils.GetLinkLabelLinks(super.Text);
            if (links.Length != 0)
            {
                foreach (LinkLabel.Link link in links)
                {
                    _ = super.Links.Add(link);
                }
                void handler(object? _, LinkLabelLinkClickedEventArgs e)
                {
                    if (e.Link != null)
                    {
                        string? linkData = e.Link.LinkData as string;
                        if (!string.IsNullOrEmpty(linkData))
                        {
                            ProcessStartInfo psi = new(linkData)
                            {
                                UseShellExecute = true,
                                Verb = "open"
                            };
                            _ = Process.Start(psi);
                            linkClickedAction?.Invoke();
                        }
                    }
                }
                LinkClickHandlers.Add(super, handler);
                super.LinkClicked += handler;
            }
        }
    }
}
