using Codice.CM.Common;
using Connect.common;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    #region START_METHOD
    [SerializeField] bool canGeneratorOne;
    [SerializeField] int stage;
    [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;
    [SerializeField] private NodeRenderer _nodePrefab;

    private int levelSize => stage + 4;

    private void Awake()
    {
        SpawnBoard();
        SpawnNodes();
    }

    private void SpawnBoard()
    {
        var board = Instantiate(_boardPrefab, new Vector3(levelSize / 2f, levelSize / 2f, 0), Quaternion.identity);

        board.size = new Vector2(levelSize + 0.08f, levelSize + 0.08f);

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                Instantiate(_bgCellPrefab, new Vector3(i + 0.5f, j + 0.5f, 0f), Quaternion.identity);
            }
        }

        Camera.main.orthographicSize = levelSize / 1.6f + 1f;
        Camera.main.transform.position = new Vector3(levelSize / 2f, levelSize / 2f, -10f);
    }

    public Dictionary<Vector2Int, NodeRenderer> nodeGrid;

    private void SpawnNodes()
    {
        nodeGrid = new Dictionary<Vector2Int, NodeRenderer>();
        Vector3 spawnPos;
        NodeRenderer spawnNode;

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                spawnPos = new Vector3(i + 0.5f, i + 0.5f, 0f);
                spawnNode = Instantiate(_nodePrefab, spawnPos, Quaternion.identity);
                spawnNode.Init();
                nodeGrid.Add(new Vector2Int(i, j), spawnNode);
                spawnNode.gameObject.name = i.ToString() + j.ToString(); ;
            }
        }
    }

    #endregion

    #region BUTTON_FUNCTION
    [SerializeField] private GameObject _simulateButton;

    public void CLickedSimulate()
    {
        Levels = new Dictionary<string, LevelData>();

        foreach (var item in _allLevelList.Levels)
        {
            Levels[item.LevelName] = item;
        }

        if (canGeneratorOne)
        {
            GenerateDefault();
        }
        else
        {

        }

        _simulateButton.SetActive(false);
    }

    [SerializeField] private LevelList _allLevelList;
    private Dictionary<string, LevelData> Levels;
    private LevelData currentLevelData;

    private void GenerateDefault()
    {
        GenerateLevelData();
    }

    private void GenerateLevelData(int level = 0)
    {
        string currentLevelName = "Level" + stage.ToString() + level.ToString();

        if(!Levels.ContainsKey(currentLevelName))
        {
#if UNITY_EDITOR
            currentLevelData = ScriptableObject.CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(currentLevelData, "Assets/Common/Prefabs/Levels" + currentLevelName + ".asset");
            AssetDatabase.SaveAssets();
#endif      
            Levels[currentLevelName] = currentLevelData;
            _allLevelList.Levels.Add(currentLevelData);

            currentLevelData.LevelName = currentLevelName;
            currentLevelData.Edges = new List<Edge>();
        }
    }
    #endregion

    #region NODE_RENDERING
    #endregion
}
