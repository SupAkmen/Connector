using Connect.common;
using Connect.Core;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GamePlayManager : MonoBehaviour
{
    #region START_METHODS
    #region START_VARIABLES
    public static GamePlayManager Instance;

    [HideInInspector] public bool hasGameFinished;

    [SerializeField] TextMeshProUGUI _tittleText;
    [SerializeField] GameObject _winText;
    [SerializeField] SpriteRenderer _clickHighlight;

    [Header("Hint (goi y)")]
    [SerializeField] private int _maxHints = 3;          // so lan goi y moi man (cau hinh)
    [SerializeField] private float _hintDuration = 2f;   // thoi gian hien duong mo (giay)
    [SerializeField] private TextMeshProUGUI _hintCountText; // (tuy chon) hien so hint con lai
    private int _hintsLeft;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        _winText.SetActive(false);
        _tittleText.gameObject.SetActive(true);
        _tittleText.text = GameManager.Instance.StageName + "-" + GameManager.Instance.CurrentLevel.ToString();

        CurrentLevelData = GameManager.Instance.GetLevel();

        BuildShuffledColors();

        SpawnBoard();
        SpawnNodes();

        _hintsLeft = _maxHints;
        UpdateHintUI();
    }
    #endregion

    #region BOARD_SPAWN
    [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;

    // tao bang dua vao currentLevelSize sau do them cac o xep vao trong board, thay doi vi tri cam ra xa cho phu hop voi board
    private void SpawnBoard()
    {
        int currentLevelSize = GameManager.Instance.CurrentStage + 4;

        var board = Instantiate(_boardPrefab,
            new Vector3(currentLevelSize/2f,currentLevelSize/2f,0f)
            ,Quaternion.identity) ;

        board.size = new Vector2(currentLevelSize + 0.08f, currentLevelSize + 0.08f);

        for (int i = 0; i< currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {
                Instantiate(_bgCellPrefab, new Vector3(i + 0.5f,j + 0.5f, 0f), Quaternion.identity) ;
            }
        }

        Camera.main.orthographicSize = currentLevelSize + 2f;
        Camera.main.transform.position = new Vector3(currentLevelSize / 2f,currentLevelSize / 2f,-10f);

        _clickHighlight.size = new Vector2(currentLevelSize/4,currentLevelSize/4);
        _clickHighlight.transform.position = Vector3.zero;
        _clickHighlight.gameObject.SetActive(false);
    }
        #endregion

    #region NODE_SPAWN

    [SerializeField] private Node _nodePrefab;
    private LevelData CurrentLevelData;
    private List<Node> _nodes;
    public Dictionary<Vector2Int, Node> _nodeGrid;

    private void SpawnNodes()
    {
        _nodes = new List<Node>();
        _nodeGrid = new Dictionary<Vector2Int, Node>();

        int currentLevelSize = GameManager.Instance.CurrentStage + 4;

        Node spawnedNode;
        Vector3 spawnPos;

        for (int i = 0; i < currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {
                spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);

                spawnedNode = Instantiate(_nodePrefab,spawnPos, Quaternion.identity);

                spawnedNode.Init();

                int colorIdForSpawnNode = GetColorID(i, j);

                if(colorIdForSpawnNode != -1)
                {
                    spawnedNode.SetColorForPoint(colorIdForSpawnNode);
                }

                _nodes.Add(spawnedNode);

                _nodeGrid.Add(new Vector2Int(i, j), spawnedNode);

                spawnedNode.Pos2D = new Vector2Int(i, j);

                spawnedNode.gameObject.name = i.ToString() + j.ToString();
            }
        }

        List<Vector2Int> offsetPos = new List<Vector2Int>()
        {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};

        foreach(var item in _nodeGrid)
        {
            foreach(var offset in offsetPos)
            {
                var checkPos = item.Key + offset;

                if(_nodeGrid.ContainsKey(checkPos))
                {
                    item.Value.SetEdge(offset,_nodeGrid[checkPos]);
                }
            }
        }
    }

    public List<Color> NodeColors;

    // Bang mau da xao - dung de moi lan choi mau khac nhau.
    private List<Color> _shuffledColors;

    // Xao bang mau bang Random that (theo thoi gian) -> moi lan vao level mau khac.
    private void BuildShuffledColors()
    {
        _shuffledColors = new List<Color>(NodeColors);
        for (int i = _shuffledColors.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_shuffledColors[i], _shuffledColors[j]) = (_shuffledColors[j], _shuffledColors[i]);
        }
    }

    // Moi cho lay mau deu di qua ham nay (khong doc thang NodeColors nua).
    public Color GetNodeColor(int colorId)
    {
        if (_shuffledColors == null)
        {
            BuildShuffledColors();
        }
        return _shuffledColors[colorId % _shuffledColors.Count];
    }

    public int GetColorID(int i,int j)
    {
        List<Edge> edges  = CurrentLevelData.Edges;

        Vector2Int point = new Vector2Int(i, j);

        for(int colorId = 0; colorId < edges.Count; colorId++)
        {
            if (edges[colorId].StartPoint == point || edges[colorId].EndPoint == point)
            {
                return colorId;
            }
        }
        return -1;
    }

    public Color GetHighLightColor(int colorId)
    {
        Color result = GetNodeColor(colorId);
        result.a = 0.4f;
        return result;
    }
    #endregion

    #endregion

    #region UPDATES_METHODS

    private Node startNode;

    private void Update()
    {
        if (hasGameFinished) return;

        if(Input.GetMouseButtonDown(0))
        {
           
            startNode = null;
            return;
        }

        if(Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x,mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos,Vector2.zero);

            if(startNode == null)
            {
                if(hit && hit.collider.gameObject.TryGetComponent(out Node tNode) && tNode.IsClickable)
                {
                    startNode = tNode;
                    _clickHighlight.gameObject.SetActive(true);
                    _clickHighlight.gameObject.transform.position = (Vector3)mousePos2D;
                    _clickHighlight.color = GetHighLightColor(tNode.colorId);
                }
                return;
            }

            _clickHighlight.gameObject.transform.position = (Vector3)mousePos2D;

            if(hit && hit.collider.gameObject.TryGetComponent(out Node tempNode) && startNode != tempNode )
            {
                if(startNode.colorId != tempNode.colorId && tempNode.IsEndNode)
                {
                    return;
                }

                startNode.UpdateInput(tempNode);

                CheckWin();
                startNode = null;
            }

            return;
        }

        if(Input.GetMouseButtonUp(0))
        {
            startNode = null;
            _clickHighlight.gameObject.SetActive(false);
        }
    }
    #endregion

    #region WIN_CODITIONS
    private void CheckWin()
    {
        bool IsWinning = true;

        foreach(var item in  _nodes)
        {
            item.SolveHighLight();
        }

        foreach(var item in _nodes)
        {
            IsWinning &= item.IsWin;

            if(!IsWinning)
            {
                return;
            }
        }

        GameManager.Instance.UnlockLevel();

        _winText.gameObject.SetActive(true);
        _clickHighlight.gameObject.SetActive(false);

        hasGameFinished = true;
    }

    #endregion

    #region HINT
    private bool _isHintShowing;
    public void ClickedHint()
    {
        if (hasGameFinished) return;
        if (_isHintShowing) return;
        if (_hintsLeft <= 0) return;

        int colorId = FindUnsolvedColor();
        if (colorId == -1) return; 

        _hintsLeft--;
        UpdateHintUI();
        StartCoroutine(ShowHintRoutine(colorId));
    }

    private int FindUnsolvedColor()
    {
        List<Edge> edges = CurrentLevelData.Edges;
        for (int colorId = 0; colorId < edges.Count; colorId++)
        {
            Vector2Int start = edges[colorId].StartPoint;
            if (_nodeGrid.TryGetValue(start, out Node startNode))
            {
                if (startNode.ConnectedNodes.Count == 0)
                {
                    return colorId;
                }
            }
        }
        return -1;
    }

    IEnumerator ShowHintRoutine(int colorId)
    {
        _isHintShowing = true;

        List<Vector2Int> path = CurrentLevelData.Edges[colorId].Points;
        List<Node> hinted = new List<Node>();

        foreach (var pos in path)
        {
            if (_nodeGrid.TryGetValue(pos, out Node node))
            {
                node.ShowHintHighlight(colorId);
                hinted.Add(node);
            }
        }

        yield return new WaitForSeconds(_hintDuration);

        foreach (var node in hinted)
        {
            node.HideHintHighlight();
        }

        _isHintShowing = false;
    }

    private void UpdateHintUI()
    {
        if (_hintCountText != null)
        {
            _hintCountText.text = _hintsLeft.ToString();
        }
    }
    #endregion

    #region BUTTON_FUNCTIONS
    public void ClickedBack()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void ClickRestart()
    {
        GameManager.Instance.GoToGamePlay();
    }

    public void ClickedNextLevel()
    {
        if (!hasGameFinished) return;

        GameManager.Instance.GoToGamePlay();
    }
    #endregion

}
