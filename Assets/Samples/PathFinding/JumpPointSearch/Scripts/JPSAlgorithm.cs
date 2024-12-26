

using System.Collections.Generic;
using UnityEngine;

namespace Algorithm
{
    public interface IGrid{
        bool IsMovable(Vector2Int pos); //检查目标点位是否存在且可移动
        bool IsObstacle(Vector2Int pos); //检查目标点是否为障碍物
        
    }
    
    /// <summary>
    /// Jump Point Search寻路算法
    /// 1.获取起点S和终点E
    /// 2.从起点S向上下左右四个方向移动，如果遇到跳点，则将跳点直接加入跳点表，跳点方向由前进方向和强制邻居方向共同决定
    /// 从起点S向左上、右上、左下、右下四个角点方向移动，每次移动一格，移动之后记当前
    /// 位置为parent，接着向角点方向的分量方向循环探索（右上的分量方向是右和上）
    /// 如果向右或者向上或者移动到当前位置时，三者有一者发现了跳点，则记录当前点parent为跳点，加入跳点表
    /// 方向由当前前进方向（比如之前是右上就还是右上）和强制邻居共同决定。
    /// 3.只要跳点表不为空，取出一个损耗最低的跳点，如果跳点为终点，退出循环
    /// 将跳点按照给定的方向循环检测。
    /// </summary>
    public class JPSAlgorithm
    {
        private BinaryHeap<JpsNode> _openList = new BinaryHeap<JpsNode>();
        private HashSet<JpsNode> _closeSet = new HashSet<JpsNode>();
        private Dictionary<Vector2Int, JpsNode> _jpsNodeDic; //存储所有的跳点
        private Vector2Int _start;
        private Vector2Int _end;
        private IGrid _grid;

        public List<Vector2Int> Find(IGrid grid, Vector2Int star, Vector2Int end)
        {
            _openList.Clear();
            _closeSet.Clear();
            _jpsNodeDic.Clear();
            _start = star;
            _end = end;
            _grid = grid;
            
            // 起点是一个特殊的跳点，也是唯一一个全方向检测的跳点，其他跳点最多拥有三个方向
            Vector2Int[] dirs = new Vector2Int[]{
                JPSHelper.Up,
                JPSHelper.Down,
                JPSHelper.Left,
                JPSHelper.Right,
                JPSHelper.UpLeft,
                JPSHelper.UpRight,
                JPSHelper.DownLeft,
                JPSHelper.DownRight,
            };
            AddJpsNode(default, star, dirs, 0);

            while (_openList.Count > 0)
            {
                JpsNode currNode = _openList[0];
                _openList.Remove(currNode);
                _closeSet.Add(currNode);
                if (currNode.Pos == _end)
                    return ReturnFinalPath(currNode);
                for (int i = 0; i < currNode.SearchDirections.Length; ++i)
                {
                    if (currNode.SearchDirections[i].x == 0 || currNode.SearchDirections[i].y == 0)
                    {
                        //水平或者垂直搜索
                        StraightSearch(currNode.Pos, currNode.SearchDirections[i], currNode.G);
                    }
                    else
                    {
                        //斜方向搜索
                        DiagonalSearch(currNode.Pos, currNode.Pos, currNode.SearchDirections[i], currNode.G);
                    }
                }
            }
            return null;
        }

        private List<Vector2Int> ReturnFinalPath(JpsNode endNode)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            JpsNode tmpNode = endNode;
            while(tmpNode != null)
            {
                path.Add(tmpNode.Pos);
                tmpNode = tmpNode.Parent;
            }
            return path;
        }

        /// <summary>
        /// 添加一个跳点
        /// </summary>
        /// <param name="parentPos"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <param name="gCost"></param>
        /// <returns></returns>
        private JpsNode AddJpsNode(Vector2Int parentPos, Vector2Int pos, Vector2Int[] dir = null, int gCost = 0)
        {
            //查找该跳点
            _jpsNodeDic.TryGetValue(pos, out JpsNode jpsNode);
            _jpsNodeDic.TryGetValue(parentPos, out JpsNode parentNode);
            if (jpsNode == null)
            {
                jpsNode = new JpsNode(pos, parentNode, dir, gCost, JPSHelper.CalcManhattan(pos, _end));
                _jpsNodeDic.Add(pos, jpsNode);
            }
            //是否已在close中
            if (_closeSet.Contains(jpsNode))
            {
                //检查是否更低代价
                if (jpsNode.G > gCost)
                {
                    jpsNode.Parent = parentNode;
                    jpsNode.G = gCost;
                    jpsNode.SearchDirections = dir;
                    _closeSet.Remove(jpsNode);
                    _openList.Add(jpsNode);
                }
            }
            else if (_openList.Contains(jpsNode)) //是否已在open中
            {
                //检查是否更低代价
                if (jpsNode.G > gCost)
                {
                    jpsNode.Parent = parentNode;
                    jpsNode.G = gCost;
                    jpsNode.SearchDirections = dir;
                    _openList.Update(_openList.IndexOf(jpsNode));
                }
            }
            else
            {
                _openList.Add(jpsNode);
            }

            return jpsNode;
        }
        
        /// <summary>
        /// 水平and垂直方向搜索
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="direction"></param>
        /// <param name="gCost"></param>
        /// <returns>是否找到跳点</returns>
        private bool StraightSearch(Vector2Int pos, Vector2Int direction, int gCost)
        {
            Vector2Int tmpPos = pos + direction;
            while (_grid.IsMovable(tmpPos))
            {
                if (tmpPos == _end)
                {
                    AddJpsNode(pos, tmpPos, null, gCost);
                    return true;
                }
                gCost += JPSHelper.HorizentalMoveCost;
                //检测是否有强制邻居
                List<Vector2Int> forceNeighborDirections = CheckForceNeighborStraight(tmpPos, direction);
                if (forceNeighborDirections.Count > 0)
                {
                    //将当前节点添加为跳点
                    //保留父节点方向
                    forceNeighborDirections.Add(direction);
                    AddJpsNode(pos, tmpPos, forceNeighborDirections.ToArray(), gCost);
                    //当前节点为跳点则退出查找
                    return true;
                }
                //继续往指定方向探索
                tmpPos += direction;
            }
            return false;
        }

        /// <summary>
        /// 水平and垂直方向检测强制邻居
        /// </summary>
        /// <returns></returns>
        private List<Vector2Int> CheckForceNeighborStraight(Vector2Int pos, Vector2Int dir)
        {
            //获取垂直于当前方向的方向
            Vector2Int[] perpendicularDir = (dir == JPSHelper.Left || dir == JPSHelper.Right) ? 
                JPSHelper.PerpendicularHorizontalDir : JPSHelper.PerpendicularVerticalDir;
            List<Vector2Int> directions = new List<Vector2Int>();
            for (int i = 0; i < perpendicularDir.Length; ++i)
            {
                Vector2Int blockPt = pos + perpendicularDir[i];
                //强制邻居判定逻辑
                if(_grid.IsObstacle(blockPt) && _grid.IsMovable(blockPt + dir))
                    directions.Add(perpendicularDir[i] + dir);
            }
            return directions;
        }
        
        /// <summary>
        /// 斜向检测强邻居
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="blockPos"></param>
        /// <param name="dir">从障碍节点探测强邻居所需移动方向</param>
        /// <returns></returns>
        private List<Vector2Int> CheckForceNeighborDiagonal(Vector2Int pos, Vector2Int blockPos, Vector2Int dir)
        {
            List<Vector2Int> directions = new List<Vector2Int>();
            blockPos += dir;
            if(_grid.IsMovable(blockPos)){
                directions.Add(blockPos - pos);
            }
            return directions;
        }

        /// <summary>
        /// 斜方向搜索。假设当前为右上方向，则只检测右节点，上节点，和右上节点，因为这些节点是必须依赖当前节点才能到达的
        /// </summary>
        private void DiagonalSearch(Vector2Int parent, Vector2Int pos, Vector2Int direction, int gCost)
        {
            //在斜方向分量上的两个位置
            Vector2Int b1 = new Vector2Int(pos.x + direction.x, pos.y);
            Vector2Int b2 = new Vector2Int(pos.x, pos.y + direction.y);
            if (_grid.IsMovable(b1))
            {
                if (_grid.IsMovable(b2))
                {
                    //向斜方向移动一个位置
                    pos += direction;
                    if (_grid.IsMovable(pos))
                    {
                        gCost += JPSHelper.DiagonalMovementCost;
                        if (pos == _end)
                        {
                            AddJpsNode(parent, pos, null, gCost);
                            return;
                        }
                        if (DiagonalSplitSearch(pos, direction, gCost))
                        {
                            //如果分量方向找到跳点，则当前节点也为跳点
                            AddJpsNode(parent, pos, new Vector2Int[]{direction}, gCost);
                            return;
                        }
                        //继续递归移动
                        DiagonalSearch(parent, pos, direction, gCost);
                    }

                }
                else
                {
                    //垂直方向存在障碍
                    pos += direction;
                    if (_grid.IsMovable(pos))
                    {
                        gCost += JPSHelper.DiagonalMovementCost;
                        if (pos == _end)
                        {
                            AddJpsNode(parent, pos, null, gCost);
                            return;
                        }
                        //检查障碍是否会导致当前节点拥有强邻居
                        List<Vector2Int> dirs = CheckForceNeighborDiagonal(pos, b2, 
                            direction * Vector2Int.up /*屏蔽水平方向只保留竖直方向*/);
                        if (DiagonalSplitSearch(pos, direction, gCost) || dirs.Count > 0)
                        {
                            dirs.Add(direction);
                            AddJpsNode(parent, pos, dirs.ToArray(), gCost);
                            return;
                        }
                        DiagonalSearch(parent, pos, direction, gCost);
                    }
                }
            }
            else
            {
                //水平方向存在障碍
                if (_grid.IsMovable(b2))
                {
                    pos += direction;
                    if (_grid.IsMovable(pos))
                    {
                        gCost += JPSHelper.DiagonalMovementCost;
                        if (pos == _end)
                        {
                            AddJpsNode(parent, pos, null, gCost);
                            return;
                        }
                        //检查障碍是否会导致当前节点拥有强邻居
                        List<Vector2Int> dirs = CheckForceNeighborDiagonal(pos, b1, 
                            direction * Vector2Int.right /*屏蔽竖直方向只保留水平方向*/);
                        if (DiagonalSplitSearch(pos, direction, gCost) || dirs.Count > 0)
                        {
                            dirs.Add(direction);
                            AddJpsNode(parent, pos, dirs.ToArray(), gCost);
                            return;
                        }
                        DiagonalSearch(parent, pos, direction, gCost);
                    }
                }
                else
                {
                    //水平垂直皆有障碍
                }
            }
            
        }

        /// <summary>
        /// 将斜方向拆解为水平和竖直方向探索，如果找到跳点则返回true
        /// </summary>
        /// <returns></returns>
        private bool DiagonalSplitSearch(Vector2Int pos, Vector2Int direction, int gCost)
        {
            bool bHor = StraightSearch(pos, new Vector2Int(direction.x, 0), gCost);
            bool bVer = StraightSearch(pos, new Vector2Int(0, direction.y), gCost);
            return bHor || bVer;
        }
        
    }
}