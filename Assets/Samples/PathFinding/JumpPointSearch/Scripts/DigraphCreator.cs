
using System.Collections.Generic;
using Algorithm;
using Unity.VisualScripting;
using UnityEngine;

namespace JumpPointSearch
{
    public class JpsUIGrid
    {
        public Vector2Int Pos;
        public GameObject Obj;
        public bool IsObstacle;
        
    }
    
    public class DigraphCreator : MonoBehaviour, IGrid
    {
        [SerializeField]
        private int _brickWidth;
        [SerializeField]
        private int _brickHeigh;
        [SerializeField]
        private GameObject _NodePrefab;
        [SerializeField]
        private Material _whiteMaterial;
        [SerializeField]
        private Material _blackMaterial;
        [SerializeField]
        private Material _greenMaterial;
        [SerializeField]
        private Material _blueMaterial;
        [SerializeField]
        private Material _redMaterial;
        
        private JPSAlgorithm _jpsAlgorithm;
        private Dictionary<Vector2Int, JpsUIGrid> _grid;
        private Vector2Int _startPos;
        private Vector2Int _endPos;
        
        public bool IsMovable(Vector2Int pos)
        {
            if (!_grid.ContainsKey(pos))
                return false;
            _grid.TryGetValue(pos, out JpsUIGrid grid);
            return !grid.IsObstacle;
        }

        public bool IsObstacle(Vector2Int pos)
        {
            if (!_grid.ContainsKey(pos))
                return true;
            _grid.TryGetValue(pos, out JpsUIGrid grid);
            return grid.IsObstacle;
        }
        
        void Start()
        {
            _jpsAlgorithm = new JPSAlgorithm();
            _grid = new Dictionary<Vector2Int, JpsUIGrid>();
            _startPos = new Vector2Int(0, 0);
            _endPos = new Vector2Int(_brickWidth - 1, _brickHeigh - 1);
            
            int currX = -_brickWidth / 2;
            for (int i = 0; i < _brickWidth; i++)
            {
                int currY = -_brickHeigh / 2;
                for (int j = 0; j < _brickHeigh; j++)
                {
                    GameObject newObj = Instantiate(_NodePrefab,new Vector3(currX, currY, 0), Quaternion.identity);
                    newObj.transform.parent = transform;
                    _grid.Add(new Vector2Int(i, j), new JpsUIGrid()
                    {
                        Pos = new Vector2Int(i, j),
                        Obj = newObj,
                    });
                    currY++;
                }
                currX++;
            }

            _grid.TryGetValue(_startPos, out JpsUIGrid startGrid);
            startGrid.Obj.GetComponent<MeshRenderer>().material = _blueMaterial;
            _grid.TryGetValue(_endPos, out JpsUIGrid endGrid);
            endGrid.Obj.GetComponent<MeshRenderer>().material = _redMaterial;
            
        }
        
        void Update()
        {
            // 检测鼠标点击
            if (Input.GetMouseButtonDown(0))  // 0表示左键点击
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // 检查射线是否击中了格子的对象
                    foreach (var grid in _grid)
                    {
                        if (hit.collider.gameObject == grid.Value.Obj)
                        {
                            OnGridClicked(grid.Value);
                            break;
                        }
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("Return (Enter) key pressed.");
                List<Vector2Int> path = _jpsAlgorithm.Find(this, _startPos, _endPos);
                foreach (var grid in _grid)
                {
                    if (grid.Value.Pos == _startPos || grid.Value.Pos == _endPos)
                        continue;
                    if(grid.Value.IsObstacle)
                        continue;
                    bool bPath = path.Contains(grid.Value.Pos);
                    grid.Value.Obj.GetComponent<MeshRenderer>().material = bPath ? _greenMaterial : _whiteMaterial;
                }
            }
            
        }

        // 处理格子被点击时的逻辑
        public void OnGridClicked(JpsUIGrid clickedGrid)
        {
            Debug.Log($"Grid clicked at position: {clickedGrid.Pos}");

            // 切换障碍物状态并更新材质
            clickedGrid.IsObstacle = !clickedGrid.IsObstacle;
            var renderer = clickedGrid.Obj.GetComponent<MeshRenderer>();
            renderer.material = clickedGrid.IsObstacle ? _blackMaterial : _whiteMaterial;
        }
        
    }
}