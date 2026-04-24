using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteAtlasForge.Core.Models;

public partial class GridDefinition : ObservableObject
{
    [ObservableProperty]
    private int _originX = 0;

    [ObservableProperty]
    private int _originY = 0;

    [ObservableProperty]
    private int _cellWidth = 64;

    [ObservableProperty]
    private int _cellHeight = 64;

    [ObservableProperty]
    private int _spacing = 0;

    [ObservableProperty]
    private int _padding = 0;

    [ObservableProperty]
    private int _columns = 10;

    [ObservableProperty]
    private int _rows = 10;

    public GridDefinition() { }

    public GridDefinition(int originX, int originY, int cellWidth, int cellHeight, 
                         int columns, int rows, int spacing = 0, int padding = 0)
    {
        OriginX = originX;
        OriginY = originY;
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        Columns = columns;
        Rows = rows;
        Spacing = spacing;
        Padding = padding;
    }

    public (int x, int y) GetCellPosition(int col, int row)
    {
        int x = OriginX + Padding + col * (CellWidth + Spacing);
        int y = OriginY + Padding + row * (CellHeight + Spacing);
        return (x, y);
    }

    public (int col, int row)? GetCellAt(int x, int y)
    {
        // Adjust for origin and padding
        int adjustedX = x - OriginX - Padding;
        int adjustedY = y - OriginY - Padding;

        if (adjustedX < 0 || adjustedY < 0)
            return null;

        int cellWithSpacing = CellWidth + Spacing;
        int rowWithSpacing = CellHeight + Spacing;

        int col = adjustedX / cellWithSpacing;
        int row = adjustedY / rowWithSpacing;

        // Check if we're in the spacing area
        if (adjustedX % cellWithSpacing >= CellWidth || 
            adjustedY % rowWithSpacing >= CellHeight)
            return null;

        if (col >= Columns || row >= Rows)
            return null;

        return (col, row);
    }
}
