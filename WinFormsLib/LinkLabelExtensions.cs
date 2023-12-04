using System.Diagnostics;

namespace WinFormsLib
{
    public static class LinkLabelExtensions
    {
        public static void UpdateLinks(this LinkLabel super, LinkLabel.Link[]? links = null, Action? linkClickedAction = null)
        {
            super.Links.Clear();
            links ??= Utils.GetLinkLabelLinks(super.Text);
            if (links.Any())
            {
                foreach (LinkLabel.Link link in links)
                {
                    super.Links.Add(link);
                }
                super.LinkClicked += (s, e) =>
                {
                    ProcessStartInfo psi = new((string)e.Link.LinkData)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(psi);
                    linkClickedAction?.Invoke();
                };
                super.Disposed += (s, e) => super.ClearEventHandlers();
            }
        }
    }
}
