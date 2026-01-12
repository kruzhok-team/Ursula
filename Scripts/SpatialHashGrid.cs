using Godot;
using System;
using System.Collections.Generic;

public class SpatialHashGrid3D
{
    private readonly float cellSize;
    private readonly Dictionary<Vector3I, HashSet<Node3D>> grid = new();
    private readonly Dictionary<Node3D, Vector3I> nodeCells = new();

    public SpatialHashGrid3D(float cellSize = 5f)
    {
        this.cellSize = cellSize;
    }

    private Vector3I WorldToCell(Vector3 pos)
    {
        return new Vector3I(
            Mathf.FloorToInt(pos.X / cellSize),
            Mathf.FloorToInt(pos.Y / cellSize),
            Mathf.FloorToInt(pos.Z / cellSize)
        );
    }

    // Добавление объекта в сетку.
    public void Add(Node3D node, Vector3 position)
    {
        Vector3I cell = WorldToCell(position);

        if (!grid.TryGetValue(cell, out var bucket))
        {
            bucket = new HashSet<Node3D>();
            grid[cell] = bucket;
        }

        bucket.Add(node);
        nodeCells[node] = cell;
    }

    // Обновление позиции объекта — перемещение между ячейками.
    public void Update(Node3D node, Vector3 position)
    {
        Vector3I newCell = WorldToCell(position);

        if (!nodeCells.TryGetValue(node, out Vector3I oldCell))
        {
            Add(node, position);
            return;
        }

        if (newCell == oldCell)
            return;

        // Удаление старой записи
        if (grid.TryGetValue(oldCell, out var oldBucket))
        {
            oldBucket.Remove(node);
            if (oldBucket.Count == 0)
                grid.Remove(oldCell);
        }

        // Добавление в новую ячейку
        if (!grid.TryGetValue(newCell, out var newBucket))
        {
            newBucket = new HashSet<Node3D>();
            grid[newCell] = newBucket;
        }

        newBucket.Add(node);
        nodeCells[node] = newCell;
    }

    // Удаление объекта из сетки.
    public void Remove(Node3D node)
    {
        if (!nodeCells.TryGetValue(node, out var cell))
            return;

        if (grid.TryGetValue(cell, out var bucket))
        {
            bucket.Remove(node);
            if (bucket.Count == 0)
                grid.Remove(cell);
        }

        nodeCells.Remove(node);
    }

    // Получение списка объектов в радиусе.
    public List<Node3D> GetItemsNodes(Vector3 center, float radius)
    {
        List<Node3D> result = new();
        float radiusSq = radius * radius;

        int minX = Mathf.FloorToInt((center.X - radius) / cellSize);
        int maxX = Mathf.FloorToInt((center.X + radius) / cellSize);

        int minY = Mathf.FloorToInt((center.Y - radius) / cellSize);
        int maxY = Mathf.FloorToInt((center.Y + radius) / cellSize);

        int minZ = Mathf.FloorToInt((center.Z - radius) / cellSize);
        int maxZ = Mathf.FloorToInt((center.Z + radius) / cellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3I cell = new(x, y, z);

                    if (!grid.TryGetValue(cell, out var bucket))
                        continue;

                    foreach (var node in bucket)
                    {
                        float distSq = center.DistanceSquaredTo(node.GlobalPosition);
                        if (distSq <= radiusSq)
                            result.Add(node);
                    }
                }
            }
        }

        return result;
    }
}
