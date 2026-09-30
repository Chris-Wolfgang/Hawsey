namespace Wolfgang.Hawsey.UI.Maui.Views;

/// <summary>Where a <see cref="SeatView"/> sits at the table.</summary>
public enum SeatPlacement
{
    /// <summary>The partner, across the table.</summary>
    Top,

    /// <summary>The human.</summary>
    Bottom,

    /// <summary>The opponent to the human's left.</summary>
    Left,

    /// <summary>The opponent to the human's right.</summary>
    Right,
}
