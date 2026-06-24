using System;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private GameObject _point;
    [SerializeField] private GameObject _topEdge;
    [SerializeField] private GameObject _botEdge;
    [SerializeField] private GameObject _leftEdge;
    [SerializeField] private GameObject _rightEdge;
    [SerializeField] private GameObject _highLight;

    private Dictionary<Node, GameObject> ConnectedEdges;
    public List<Node> ConnectedNodes = new List<Node>();

    [HideInInspector] public int colorId;
    public Vector2Int Pos2D { get; set; }
    public bool IsWin
    {
        get
        {
            if (_point.activeSelf)
            {
                return ConnectedNodes.Count == 1;
            }
            return ConnectedNodes.Count == 2;
        }
    }

    public bool IsClickable
    {
        get
        {
            if (_point.activeSelf)
            {
                return true;
            }

            return ConnectedNodes.Count > 0;
        }
    }

    public bool IsEndNode => _point.activeSelf;

    public void Init()
    {
        _point.SetActive(false);
        _topEdge.SetActive(false);
        _botEdge.SetActive(false);
        _leftEdge.SetActive(false);
        _rightEdge.SetActive(false);
        _highLight.SetActive(false);
        ConnectedEdges = new Dictionary<Node, GameObject>();
        ConnectedNodes = new List<Node>();
    }

    public void SetColorForPoint(int colorIdForSpawnNode)
    {
        colorId = colorIdForSpawnNode;
        _point.SetActive(true);
        _point.GetComponent<SpriteRenderer>().color = GamePlayManager.Instance.NodeColors[colorId];
    }

    public void SetEdge(Vector2Int offset, Node node)
    {
        if (offset == Vector2Int.up)
        {
            ConnectedEdges[node] = _topEdge;
            return;
        }

        if (offset == Vector2Int.down)
        {
            ConnectedEdges[node] = _botEdge;
            return;
        }

        if (offset == Vector2Int.left)
        {
            ConnectedEdges[node] = _leftEdge;
            return;
        }

        if (offset == Vector2Int.right)
        {
            ConnectedEdges[node] = _rightEdge;
            return;
        }
    }

    public void SolveHighLight()
    {

    }

    public void UpdateInput(Node connectedNode)
    {
        // Invalid Input 
        if (!ConnectedEdges.ContainsKey(connectedNode))
        {
            return;
        }

        AddEdge(connectedNode);
    }

    private void AddEdge(Node connectedNode)
    {
        connectedNode.colorId = colorId;
        connectedNode.ConnectedNodes.Add(this);
        ConnectedNodes.Add(connectedNode);
        GameObject connectedEdge = ConnectedEdges[connectedNode];
        connectedEdge.SetActive(true);
        connectedEdge.GetComponent<SpriteRenderer>().color = GamePlayManager.Instance.NodeColors[colorId];
    }
}
