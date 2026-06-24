using System.Diagnostics;

namespace WinFormsLib
{
    public static class LinkLabelExtensions
    {
        public static void UpdateLinks(this LinkLabel super, LinkLabel.Link[]? links = null, Action? linkClickedAction = null)
        {
            super.Links.Clear();
            links ??= Utils.GetLinkLabelLinks(super.Text);
            if (links.Length != 0)
            {
                foreach (LinkLabel.Link link in links)
                {
                    _ = super.Links.Add(link);
                }
                super.LinkClicked += (s, e) =>
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
                };
                super.Disposed += (s, e) => super.ClearEventHandlers();
            }
        }
    }
}
