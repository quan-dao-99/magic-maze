using System;
using UnityEngine;

namespace LiftStudio
{
    public class CustomGrid<T>
    {
        private readonly Transform _containerTransform;
        private readonly Vector3 _originPosition;
        private readonly T[,] _grid;
        private readonly int _width;
        private readonly int _height;
        private readonly int _cellSize;
        private Bounds _bounds;

        public CustomGrid(Transform containerTransform, Transform visualTransform, Vector3 originPosition, int width,
            int height, int cellSize,
            Func<CustomGrid<T>, int, int, T> createCellObject)
        {
            _containerTransform = containerTransform;
            _originPosition = originPosition;
            _width = width;
            _height = height;
            _cellSize = cellSize;

            _grid = new T[width, height];
            _bounds = new Bounds(visualTransform.position, new Vector3(4f, 0, 4f));

            SetupInitialCell(createCellObject);

            // DrawDebugGridGizmo(width, height);
        }

        private void SetupInitialCell(Func<CustomGrid<T>, int, int, T> createCellObject)
        {
            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    _grid[x, y] = createCellObject(this, x, y);
                }
            }
        }

        private void DrawDebugGridGizmo(int width, int height)
        {
            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    Debug.DrawLine(GetCellWorldPosition(x, y), GetCellWorldPosition(x + 1, y), Color.white, 50f);
                    Debug.DrawLine(GetCellWorldPosition(x, y), GetCellWorldPosition(x, y + 1), Color.white, 50f);
                }
            }

            Debug.DrawLine(GetCellWorldPosition(0, height), GetCellWorldPosition(width, height), Color.white, 50f);
            Debug.DrawLine(GetCellWorldPosition(width, 0), GetCellWorldPosition(width, height), Color.white, 50f);
        }

        public void DrawDebugGridGizmo()
        {
            DrawDebugGridGizmo(_width, _height);
        }

        public Vector3 GetCellCenterWorldPosition(Vector3 worldPosition)
        {
            var (x, y) = GetGridPosition(worldPosition);

            if (x < 0 || y < 0) return Vector3.negativeInfinity;

            return FormatWorldPosition(GetCellWorldPosition(x, y) +
                                       _containerTransform.rotation * new Vector3(_cellSize / 2f, 0, _cellSize / 2f));
        }

        public Vector3 GetCellCenterWorldPosition(int x, int y)
        {
            if (x < 0 || y < 0) return Vector3.negativeInfinity;

            return FormatWorldPosition(GetCellWorldPosition(x, y) +
                                       _containerTransform.rotation * new Vector3(_cellSize / 2f, 0, _cellSize / 2f));
        }

        private Vector3 GetCellWorldPosition(int x, int y)
        {
            return FormatWorldPosition(_containerTransform.rotation * (new Vector3(x, 0, y) * _cellSize) +
                                       _originPosition);
        }

        private Vector3 GetCellWorldPosition(Vector3 worldPosition)
        {
            var (x, y) = GetGridPosition(worldPosition);

            return GetCellWorldPosition(x, y);
        }

        private (int x, int y) GetGridPosition(Vector3 worldPosition)
        {
            if (!_bounds.Contains(worldPosition)) return (-1, -1);

            var containerRotation = _containerTransform.rotation;
            var (x, y) = (
                Mathf.FloorToInt(Mathf.Abs((containerRotation * (worldPosition - _originPosition)).x) / _cellSize),
                Mathf.FloorToInt(Mathf.Abs((containerRotation * (worldPosition - _originPosition)).z) / _cellSize));
            if (x < 0 || y < 0 || x >= _width || y >= _height) return (-1, -1);

            return (x, y);
        }

        private T GetGridCellObject(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height) return default(T);

            return _grid[x, y];
        }

        public T GetGridCellObject(Vector3 worldPosition)
        {
            var (x, y) = GetGridPosition(worldPosition);
            return GetGridCellObject(x, y);
        }

        private static Vector3 FormatWorldPosition(Vector3 vectorToFormat)
        {
            vectorToFormat.x = Mathf.Round(vectorToFormat.x * 10f) / 10f;
            vectorToFormat.z = Mathf.Round(vectorToFormat.z * 10f) / 10f;
            return vectorToFormat;
        }
    }
}