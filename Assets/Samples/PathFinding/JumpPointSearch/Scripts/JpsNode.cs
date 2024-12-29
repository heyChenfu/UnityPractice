
using System;
using UnityEngine;

namespace Algorithm
{
    public class JpsNode : IComparable
    {
        public Vector2Int Pos;
        public Vector2Int ParentPos;
        public Vector2Int[] SearchDirections; //跳点探索方向
        public int G; //从起点到当前节点移动代价
        public int H; //从当前节点到终点估算(忽略障碍物)
        public int F => G + H;

        public JpsNode(Vector2Int pos, Vector2Int parentPos, Vector2Int[] searchDirections, int g, int h)
        {
            Pos = pos;
            ParentPos = parentPos;
            SearchDirections = searchDirections;
            G = g;
            H = h;
        }
        
        public int CompareTo(object obj)
        {
            if (obj == null) 
                return 1;

            JpsNode node = obj as JpsNode;
            if (node != null)
                return F.CompareTo(node.F);
            else
                throw new ArgumentException("Object is not a AStarNode");
        }
        
    }
}