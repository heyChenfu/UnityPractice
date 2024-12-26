using UnityEngine;

namespace Algorithm
{
    public static class JPSHelper
    {
        public static readonly int HorizentalMoveCost = 10; //横向移动开销

        public static readonly int DiagonalMovementCost = 14; //斜向移动开销

        //八个方向
        public static readonly Vector2Int Left = Vector2Int.left;
        public static readonly Vector2Int Right = Vector2Int.right;
        public static readonly Vector2Int Up = Vector2Int.up;
        public static readonly Vector2Int Down = Vector2Int.down;
        public static readonly Vector2Int UpRight = Vector2Int.one;
        public static readonly Vector2Int UpLeft = new Vector2Int(-1, 1);
        public static readonly Vector2Int DownRight = new Vector2Int(1, -1);
        public static readonly Vector2Int DownLeft = new Vector2Int(-1, -1);

        //垂直于水平方向的方向
        public static readonly Vector2Int[] PerpendicularHorizontalDir =
            new Vector2Int[] { JPSHelper.Up, JPSHelper.Down };
        //垂直于竖直方向的方向
        public static readonly Vector2Int[] PerpendicularVerticalDir =
            new Vector2Int[] { JPSHelper.Left, JPSHelper.Right };
        
        /// <summary>
        /// 曼哈顿距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static int CalcManhattan(Vector2Int p1, Vector2Int p2)
        {
            int diff_x = Mathf.Abs(p1.x - p2.x); // 水平方向的差距
            int diff_y = Mathf.Abs(p1.y - p2.y); // 垂直方向的差距
            int min = Mathf.Min(diff_x, diff_y); // 可以走对角线的步数
            int diff = Mathf.Abs(diff_x - diff_y); // 剩下的水平或垂直步数
            return min * DiagonalMovementCost + diff * HorizentalMoveCost;
        }

        /// <summary>
        /// 欧拉距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static int CalcEulerLen(in Vector2Int p1, in Vector2Int p2)
        {
            int dx = p1.x - p2.x;
            int dy = p1.y - p2.y;
            return dx* dx + dy* dy;
        }
    }
    
}