using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DishwasherGrid : MonoBehaviour
{
    private class GridCell
    {
        public IGridContent content;

        public Vector2Int gridPosition;
        
        public Vector3 worldDownCornerPosition;
        public Vector3 worldTopCornerPosition;

        public bool IsPointInsideCell(Vector3 point)
        {
            bool isHigherThanDown = worldDownCornerPosition.x >= point.x && worldDownCornerPosition.z >= point.z;
            bool isLowerThanTop = worldTopCornerPosition.x <= point.x && worldTopCornerPosition.z <= point.z;

            return isHigherThanDown && isLowerThanTop;
        }

        public override string ToString()
        {
            return $"{gridPosition.x}, {gridPosition.y}";
        }
    }

    public Action<int> onFreeSpaceChanged;

    [SerializeField] private Transform _gridStart;
    [SerializeField] private Transform _gridEnd;
    [SerializeField] private Transform _contentParent;

    [Space] 
    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;
    
    [Space] 
    [SerializeField] private float _contentHoverHeight;

    private List<IGridContent> _placedContent = new List<IGridContent>();
    
    private float _cellWidth;
    private float _cellHeight;
    
    private GridCell[,] _grid;
    private List<GridCell> _currentSelectedGrid;
    
    private void Start()
    {
        _grid = new GridCell[_gridWidth,_gridHeight];

        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                _grid[i, j] = new GridCell();
                _grid[i, j].gridPosition = new Vector2Int(i, j);

                float downX = Mathf.Lerp(_gridStart.position.x, _gridEnd.position.x, (float) i / (float) _gridWidth);
                float downZ = Mathf.Lerp(_gridStart.position.z, _gridEnd.position.z, (float) j / (float) _gridHeight);
                
                float topX = Mathf.Lerp(_gridStart.position.x, _gridEnd.position.x, (float) (i + 1) / (float) _gridWidth);
                float topZ = Mathf.Lerp(_gridStart.position.z, _gridEnd.position.z, (float) (j + 1) / (float) _gridHeight);
                
                _grid[i, j].worldDownCornerPosition = new Vector3(downX, _gridStart.position.y,  downZ);
                _grid[i, j].worldTopCornerPosition = new Vector3(topX, _gridStart.position.y,  topZ);
            }
        }
    }

    public (Vector3 hoverPos, bool isPlaceble) GetContentHoverPosition(Vector3 position, IGridContent gridContent)
    {
        var targetCell = FindCellForPoint(position);

        if (targetCell != null)
        {
            var cells = GetCellsForContent(gridContent, targetCell);

            if (cells != null && cells.Count > 0)
            {
                _currentSelectedGrid = cells;
                var hoverPos = GetWorldPointForCellList(cells);
                hoverPos.y = position.y + _contentHoverHeight;

                bool isCellsFree = true;

                foreach (var cell in cells)
                {
                    isCellsFree = isCellsFree && cell.content == null;
                }
                
                return (hoverPos, isCellsFree);
            }
        }

        return (position, false);
    }

    public bool PlaceContent(Vector3 position, IGridContent gridContent)
    {
        var targetCell = FindCellForPoint(position);

        if (targetCell != null)
        {
            var cells = GetCellsForContent(gridContent, targetCell);

            if (cells != null && cells.Count > 0)
            {
                _currentSelectedGrid = cells;
                var contentPos = GetWorldPointForCellList(cells);
                contentPos.y = position.y;

                bool isCellsFree = true;
                
                foreach (var cell in cells)
                {
                    isCellsFree = isCellsFree && cell.content == null;
                }

                if (isCellsFree)
                {
                    _placedContent.Add(gridContent);
                    gridContent.onContentRemoved += ClearGridFromContent;
                    
                    gridContent.SetPosition(contentPos);
                    gridContent.SetParent(_contentParent);

                    foreach (var cell in cells)
                    {
                        cell.content = gridContent;
                    }

                    onFreeSpaceChanged?.Invoke(GetFreeCells().Count);
                    
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsFilledCorrectly()
    {
        bool isFilledCorrectly = true;
        
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                if (_grid[i, j].content != null)
                {
                    isFilledCorrectly = isFilledCorrectly && _grid[i, j].content.IsCompatibleWithDishwasher;
                }
            }
        }

        return isFilledCorrectly;
    }

    public void OnDrawGizmos()
    {
        if (_grid != null && _currentSelectedGrid != null)
        {
            for (int i = 0; i < _currentSelectedGrid.Count; i++)
            {
                Gizmos.DrawSphere(_currentSelectedGrid[i].worldDownCornerPosition, 0.05f);
                Gizmos.DrawSphere(_currentSelectedGrid[i].worldTopCornerPosition, 0.05f);
            }
        }
    }

    private void ClearGridFromContent(IGridContent content)
    {
        content.onContentRemoved -= ClearGridFromContent;
        _placedContent.Remove(content);
        
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                if (_grid[i, j].content == content)
                    _grid[i, j].content = null;
            }
        }
    }

    private GridCell FindCellForPoint(Vector3 point)
    {
        GridCell cell = null;
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                if (_grid[i, j].IsPointInsideCell(point))
                {
                    cell = _grid[i, j];
                    break;
                }
            }
        }

        return cell;
    }

    private List<GridCell> GetCellsForContent(IGridContent content, GridCell positioningCell)
    {
        print(positioningCell);
        Vector2Int contentCentalCell = new Vector2Int((int)((float)content.Width / 2 - 0.5f), (int)((float)content.Height / 2 - 0.5f));

        Vector2Int contentDownCellCoordinates = positioningCell.gridPosition - contentCentalCell;
        Vector2Int contentTopCellCoordinates = contentDownCellCoordinates + new Vector2Int(content.Width - 1, content.Height - 1);

        if (IsCoordinatesInsideGrid(contentDownCellCoordinates) && IsCoordinatesInsideGrid(contentTopCellCoordinates))
        {
            List<GridCell> contentCellList = new List<GridCell>();

            for (int i = 0; i < content.Width; i++)
            {
                for (int j = 0; j < content.Height; j++)
                {
                    contentCellList.Add(_grid[contentDownCellCoordinates.x + i, contentDownCellCoordinates.y + j]);
                }
            }

            return contentCellList;
        }

        return null;
    }

    private Vector3 GetWorldPointForCellList(List<GridCell> cells)
    {
        Vector2Int minCoordinates = cells[0].gridPosition;
        Vector2Int maxCoordinates = cells[0].gridPosition;

        foreach (var cell in cells)
        {
            if (cell.gridPosition.x < minCoordinates.x)
                minCoordinates.x = cell.gridPosition.x;
            
            if (cell.gridPosition.y < minCoordinates.y)
                minCoordinates.y = cell.gridPosition.y;
            
            if (cell.gridPosition.x > maxCoordinates.x)
                maxCoordinates.x = cell.gridPosition.x;
            
            if (cell.gridPosition.y > maxCoordinates.y)
                maxCoordinates.y = cell.gridPosition.y;
        }

        float cellsCenterX = (_grid[minCoordinates.x, minCoordinates.y].worldDownCornerPosition.x + _grid[maxCoordinates.x, maxCoordinates.y].worldTopCornerPosition.x) / 2;
        float cellsCenterZ = (_grid[minCoordinates.x, minCoordinates.y].worldDownCornerPosition.z + _grid[maxCoordinates.x, maxCoordinates.y].worldTopCornerPosition.z) / 2;
        
        Vector3 cellsCenter = new Vector3(cellsCenterX, cells[0].worldDownCornerPosition.y, cellsCenterZ);
        
        return cellsCenter;
    }

    private bool IsCoordinatesInsideGrid(Vector2Int coordinates)
    {
        bool isCoordinatesPositive = coordinates.x >= 0 && coordinates.y >= 0;
        bool isCoordinatesLessThanGridSize = coordinates.x < _grid.GetLength(0) && coordinates.y < _grid.GetLength(1);
        
        return isCoordinatesPositive && isCoordinatesLessThanGridSize;
    }

    private List<GridCell> GetFreeCells()
    {
        List<GridCell> _freeCells = new List<GridCell>();
        
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                if (_grid[i, j].content == null)
                {
                    _freeCells.Add(_grid[i, j]);
                }
            }
        }

        return _freeCells;
    }

    [EditorButton]
    public void TestPoint(Transform point)
    {
        print(FindCellForPoint(point.position));
    }
    
    [EditorButton]
    public void TestCoordinateInsideGrid()
    {
        print(_grid.GetLength(0));
        print(_grid.GetLength(1));
        print(IsCoordinatesInsideGrid(new Vector2Int(8, 3)));
    }
}