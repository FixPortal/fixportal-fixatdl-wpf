// Portions derived from Atdl4net (c) 2010-2011 Steve Wilkinson, MIT - see NOTICE.
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
