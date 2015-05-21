using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;
using DGui;
namespace BEProject3
{
    public enum SearchStatus
    {
        Stopped,
        PathFound,
        Searching,
        NoPath
    }
    

    public struct SearchNodes
    {
        public Vector2 position;
        public float distanceToGoal;
        public float distanceTraveled;

        public SearchNodes(Vector2 position, float distanceToGoal, float distanceTraveled)
        {
            this.position = position;
            this.distanceTraveled = distanceTraveled;
            this.distanceToGoal = distanceToGoal;
        }
    }
    public enum AlgorithmType
    {
        Astar, DynamicTAOMTP
    }
    public class AStar
    {
        public static AlgorithmType type;
        List<SearchNodes> openList;
        List<SearchNodes> closedList;
        Dictionary<Vector2, Vector2> paths;
        SearchStatus searchStatus;
        Vector2 end;
        SearchNodes nextExpanded;

        public AStar()
        {
            openList = new List<SearchNodes>();
            closedList = new List<SearchNodes>();
            paths = new Dictionary<Vector2, Vector2>();
            type = AlgorithmType.DynamicTAOMTP;
        }
        public void Reset()
        {
            searchStatus = SearchStatus.Stopped;
            openList.Clear();
            closedList.Clear();
            paths.Clear();
        }
        public void ExecuteSearch(int lookahead,Vector2 start, Vector2 end,out Stack<Vector2> waypoints)
        {
            //start and end are in tile coordinates
            Reset();
            this.end = end;
            searchStatus = SearchStatus.Searching;
            openList.Add(new SearchNodes(start, Vector2.Distance(start, end), 0f));
            if (type == AlgorithmType.Astar)
            {
                while (searchStatus == SearchStatus.Searching)
                {
                    DoSearchStep();
                }
            }
            else
            {
                while (searchStatus == SearchStatus.Searching && lookahead > 0)
                {
                    DoSearchStep();
                    lookahead--;
                }
            }
            waypoints = FinalPath();
            if (type == AlgorithmType.DynamicTAOMTP)
            {
                if (Pursuer.status == PursuerStatus.exploring)
                {
                    UpdateHeurestics();
                }
            }
        }

        private void UpdateHeurestics()
        {
            foreach(SearchNodes node in closedList)
            {
                SearchNodes newNode = new SearchNodes(node.position, node.distanceToGoal, node.distanceTraveled);
                newNode.distanceToGoal = nextExpanded.distanceTraveled + nextExpanded.distanceToGoal - newNode.distanceTraveled;
            }
        }
        private void DoSearchStep()
        {
            SearchNodes newOpenListNode;
            bool foundNewNode = SelectNodeToVisit(out newOpenListNode);
            nextExpanded = newOpenListNode;
            if (foundNewNode)
            {
                Vector2 currentPos = newOpenListNode.position;
                foreach (Vector2 point in Map.OpenNodes(currentPos))
                {
                    SearchNodes mapTile = new SearchNodes(point, StepDistance(point, end), newOpenListNode.distanceTraveled + 1);
                    if (!InList(openList, point) && !InList(closedList, point))
                    {
                        openList.Add(mapTile);
                        paths[point] = newOpenListNode.position;
                    }
                }
                if (currentPos == end)
                {
                    searchStatus = SearchStatus.PathFound;
                }
                openList.Remove(newOpenListNode);
                closedList.Add(newOpenListNode);
            }
            else
            {
                searchStatus = SearchStatus.NoPath;
            }
        }
        private bool InList(List<SearchNodes> list, Vector2 point)
        {
            bool result = false;
            foreach (SearchNodes node in list)
            {
                if (node.position == point)
                    result = true;
            }
            return result;
        }
        private float StepDistance(Vector2 point, Vector2 end)
        {
            float distanceX = Math.Abs(point.X - end.X);
            float distanceY = Math.Abs(point.Y - end.Y);
            return distanceX + distanceY;
        }
        private bool SelectNodeToVisit(out SearchNodes result)
        {
            result = new SearchNodes();
            bool success = false;
            float smallestDistance = float.PositiveInfinity;
            float currentDistance = 0f;
            if (openList.Count > 0)
            {
                foreach (SearchNodes node in openList)
                {
                    currentDistance = Heuristic(node);
                    if (currentDistance <= smallestDistance)
                    {
                        if (currentDistance < smallestDistance)
                        {
                            success = true;
                            result = node;
                            smallestDistance = currentDistance;
                        }
                        else if (currentDistance == smallestDistance &&
                            node.distanceTraveled > result.distanceTraveled)
                        {
                            success = true;
                            result = node;
                            smallestDistance = currentDistance;
                        }
                    }
                }
            }
            return success;
        }

        private float Heuristic(SearchNodes node)
        {
            return node.distanceTraveled + node.distanceToGoal;
        }

        public Stack<Vector2> FinalPath()
        {
            Stack<Vector2> path = new Stack<Vector2>();
            Vector2 curPrev = nextExpanded.position; 
                path.Push(curPrev);
                while (paths.ContainsKey(curPrev))
                {
                    curPrev = paths[curPrev];
                    path.Push(curPrev);
                }
            return path;
        }

    }

}