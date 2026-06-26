using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AllNeighbours : MonoBehaviour,IGenerateMethod
{
    [SerializeField] private TextMeshProUGUI _timeText, _gridCountText;
    private List<GridData> _checkingGrid;
    private LevelGenerator Instance;
    private bool isCreating;

    [SerializeField] private float speedMultipler;
    [SerializeField] private float speed;

    private void Start()
    {
        _checkingGrid = new List<GridData>();
        Instance = GetComponent<LevelGenerator>();
    }

    public void Generate()
    {
        
    }
}

public class GridData
{
    public GridData()
    {

    }

}
