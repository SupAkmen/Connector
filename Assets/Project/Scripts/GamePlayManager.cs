using Connect.common;
using Connect.Core;
using NUnit.Framework;
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

        private void Awake()
        {
            Instance = this;

            hasGameFinished = false;
            _winText.SetActive(false);
            _tittleText.gameObject.SetActive(true);
            _tittleText.text = GameManager.Instance.StageName + "-" + GameManager.Instance.CurrentLevel.ToString();


            SpawnBoard();

            SpawnNodes();

        }
    #endregion

    #region BOARD_SPAWN
    [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;

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

    private LevelData CurrentLevelData;
    [SerializeField] private Node _nodePrefab;
    private List<Node> _nodes;

    public Dictionary<Vector2Int, Node> _nodeGrid;

    private void SpawnNodes()
    {
        _nodes = new List<Node>();
        _nodeGrid = new Dictionary<Vector2Int, Node>();

        int currentLevelSize = GameManager.Instance.CurrentLevel + 4;

        Node spawnedNode;
        Vector3 spawnPos;

        for (int i = 0; i < currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {
                spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);

                spawnedNode = Instantiate(_nodePrefab,spawnPos, Quaternion.identity);
            }
        }

    }

    public List<Color> NodeColors;

    #endregion

    #endregion

    #region UPDATES_METHODS
    #endregion

    #region WIN_CODITIONS
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
