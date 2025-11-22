namespace NRL_PROJECT.Models
{
    /// <summary>
    /// Error view model passed to the Error view.
    /// Contains the RequestId so the view can display it when helpful for support.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
