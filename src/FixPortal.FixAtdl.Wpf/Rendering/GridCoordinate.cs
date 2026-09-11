namespace FixPortal.FixAtdl.Wpf.Rendering;

public struct GridCoordinate
{
    public int Row { get; set; }
    public int Column { get; set; }

    public GridCoordinate(int row, int column)
    {
        Row = row;
        Column = column;
    }
}
