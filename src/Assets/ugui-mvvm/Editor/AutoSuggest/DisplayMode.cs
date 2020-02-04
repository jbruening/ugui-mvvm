namespace AutoSuggest
{
    /// <summary>
    /// Values that specify how content (auto-suggestions) should be shown.
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>The content is shown immediately following the control, taking up space in the control layout.</summary>
        Inline,
        /// <summary>The content is drawn on top of adjacent controls, not taking up space in the control layout if possible.</summary>
        Overlay,
    }
}
