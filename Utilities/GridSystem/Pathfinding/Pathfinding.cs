﻿using System.Collections.Generic;
using UnityEngine;

namespace CodeEasyYT.Utilities.GridSystem.Pathfinding
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGNOAL_COST = 14;

        private Grid<PathNode> grid;
        private List<PathNode> openList;
        private List<PathNode> closeList;

        public Pathfinding(int width, int height, float cellSize, Vector2 originPosition, bool showDebug = false, Color? debugColor = null, int fontSize = 20)
        {
            grid = new Grid<PathNode>(width, height, cellSize, originPosition, (Grid<PathNode> grid, int x, int y) => new PathNode(grid, x, y), true, Color.white, fontSize);
        }

        public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            PathNode startNode = grid.GetGridObject(startX, startY);
            PathNode endNode = grid.GetGridObject(endX, endY);

            openList = new List<PathNode> { startNode };
            closeList = new List<PathNode>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = grid.GetGridObject(x, y);
                    pathNode.gCost = int.MaxValue;
                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if(currentNode == endNode)
                {
                    //Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closeList.Add(currentNode);

                foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
                {
                    if (closeList.Contains(neighbourNode)) continue;
                    if(!neighbourNode.isWalkable)
                    {
                        closeList.Add(neighbourNode);
                        continue;
                    }

                    if (CalculateDistanceCost(currentNode, neighbourNode) == int.MaxValue)
                    {
                        Debug.LogWarning("Path could not found, make sure that you handled NullReferenceException");
                        return null;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                    if(tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalculateFCost();

                        if(!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            Debug.LogWarning("Path could not found, make sure that you handled NullReferenceException");
            return null;
        }

        private List<PathNode> GetNeighbourList(PathNode currentNode)
        {
            List<PathNode> neighbourList = new List<PathNode>();

            if (currentNode.x - 1 >= 0)
            {
                // Left
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));
                // Left Down
                if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
                // Left Up
                if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
            if (currentNode.x + 1 < grid.GetWidth())
            {
                // Right
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));
                // Right Down
                if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
                // Right Up
                if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
            // Down
            if (currentNode.y - 1 >= 0) neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
            // Up
            if (currentNode.y + 1 < grid.GetHeight()) neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));

            return neighbourList;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;
            while(currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            try
            {
                int xDistance = Mathf.Abs(a.x - b.x);
                int yDistance = Mathf.Abs(a.y - b.y);
                int remaining = Mathf.Abs(xDistance - yDistance);
                return MOVE_DIAGNOAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
            }
            catch
            {
                return int.MaxValue;
            }
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCost = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if(pathNodeList[i].fCost < lowestFCost.fCost)
                {
                    lowestFCost = pathNodeList[i];
                }
            }
            return lowestFCost;
        }

        public Grid<PathNode> GetGrid() => grid;

        public PathNode GetNode(int x, int y) => grid.GetGridObject(x, y);

        public void SetWalkableState(int x, int y, bool walkable)
        {
            grid.GetGridObject(x, y).isWalkable = walkable;
        }

        public bool GetWalkableState(int x, int y)
        {
            return grid.GetGridObject(x, y).isWalkable;
        }
    }
}