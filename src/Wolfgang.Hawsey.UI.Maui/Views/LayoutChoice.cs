namespace Wolfgang.Hawsey.UI.Maui.Views;

/// <summary>Which layout the game page shows (<see cref="LayoutPicker"/>).</summary>
public enum LayoutChoice
{
    /// <summary>The table where it fits well enough to play by touch; the compact layout where it does not.</summary>
    Auto,

    /// <summary>The table, laid out like the Blazor UI and scaled to fit.</summary>
    Table,

    /// <summary>The compact phone layout, at full size with touch-sized controls.</summary>
    Compact,
}
