using Connect.Core;
using NUnit.Framework;
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

    // Dung luu node hang nom -> Edge dung de noi toi node do ( A--B--C : B.ConnectedNodes { A -> leftEdge , C -> rightEdge} B co the noi voi A va C )
    private Dictionary<Node, GameObject> ConnectedEdges;

    // Dung de luu cac node duoc noi that su 
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

    public void UpdateInput(Node connectedNode)
    {
        // Invalid Input 
        if (!ConnectedEdges.ContainsKey(connectedNode))
        {
            return;
        }
        // Connected node already exist 
        // Delete the edges and the parts
        if (ConnectedNodes.Contains(connectedNode))
        {
            ConnectedNodes.Remove(connectedNode);
            connectedNode.ConnectedNodes.Remove(this);
            RemoveEdge(connectedNode);
            DeleteNode();
            connectedNode.DeleteNode();
            return;
        }
        //Start Node has 2 edges
        if (ConnectedNodes.Count == 2)
        {
            Node tempNode = ConnectedNodes[0];

            if (!tempNode.IsConnectedToEndNode())
            {
                ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(this);
                RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }
            else
            {
                tempNode = ConnectedNodes[1];
                ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(this);
                RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }
        }
        //EndNode has 2 edges
        if (connectedNode.ConnectedNodes.Count == 2)
        {
            Node tempNode = connectedNode.ConnectedNodes[0];

            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();

            tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }
        // Starting Node is different color and connected node has 1 edge
        if (connectedNode.ConnectedNodes.Count == 1 && connectedNode.colorId != colorId)
        {
            Node tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }
        // Starting is Edge Node and has 1 edge already 
        if (ConnectedNodes.Count == 1 && IsEndNode)
        {
            Node tempNode = ConnectedNodes[0];
            ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(this);
            RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }
        // ConnectedNode is EdgeNode and ha 1 edge already
        if (connectedNode.ConnectedNodes.Count == 1 && connectedNode.IsEndNode)
        {
            Node tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }

        AddEdge(connectedNode);

        // Dont allow box 
        if (colorId != connectedNode.colorId)
        {
            return;
        }

        List<Node> checkingNodes = new List<Node>() { this };
        List<Node> resultsNodes = new List<Node>();

        while (checkingNodes.Count > 0)
        {
            foreach (var item in checkingNodes[0].ConnectedNodes)
            {
                if (!resultsNodes.Contains(item))
                {
                    resultsNodes.Add(item);
                    checkingNodes.Add(item);
                }
            }

            checkingNodes.Remove(checkingNodes[0]);
        }

        foreach (var item in resultsNodes)
        {
            if (!item.IsEndNode && item.IsDegreeThree(resultsNodes))
            {
                Node tempNode = item.ConnectedNodes[0];
                item.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(item);
                item.RemoveEdge(tempNode);
                tempNode.DeleteNode();

                if (item.ConnectedNodes.Count == 0) return;

                tempNode = item.ConnectedNodes[0];
                item.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(item);
                item.RemoveEdge(tempNode);
                tempNode.DeleteNode();

                return;
            }
        }

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

    public void RemoveEdge(Node node)
    {
        GameObject edge = ConnectedEdges[node];
        edge.SetActive(false);
        edge = node.ConnectedEdges[this];
        edge.SetActive(false);
    }

    private void DeleteNode()
    {
        Node startNode = this;

        if (startNode.IsConnectedToEndNode())
        {
            return;
        }

        while (startNode != null)
        {
            Node tempNode = null;
            if (startNode.ConnectedNodes.Count != 0)
            {
                tempNode = startNode.ConnectedNodes[0];
                startNode.ConnectedNodes.Clear();
                tempNode.ConnectedNodes.Remove(startNode);
                startNode.RemoveEdge(tempNode);
            }

            startNode = tempNode;
        }
    }

    public bool IsConnectedToEndNode(List<Node> checkNode = null)
    {
        if (checkNode == null)
        {
            checkNode = new List<Node>();
        }

        if (IsEndNode)
        {
            return true;
        }

        foreach (var item in ConnectedNodes)
        {
            if (!checkNode.Contains(item))
            {
                checkNode.Add(item);
                return item.IsConnectedToEndNode(checkNode);
            }
        }

        return false;
    }

    private List<Vector2Int> directionCheck = new List<Vector2Int>()
    {
        Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right
    };

    public bool IsDegreeThree(List<Node> resultNodes)
    {
        bool isDegreeThree = false;

        int numOfNeighbours = 0;

        for (int i = 0; i < directionCheck.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int checkingPos = Pos2D + directionCheck[(i + j) % directionCheck.Count];

                if (GamePlayManager.Instance._nodeGrid.TryGetValue(checkingPos, out Node result))
                {
                    if (resultNodes.Contains(result))
                    {
                        numOfNeighbours++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (numOfNeighbours == 3)
            {
                break;
            }

            numOfNeighbours = 0;
        }

        if (numOfNeighbours >= 3)
        {
            isDegreeThree = true;
        }

        return isDegreeThree;
    }

    public void SolveHighLight()
    {
        if(ConnectedNodes.Count == 0)
        {
            _highLight.SetActive(false);
            return;
        }


        List<Node> checkingNodes = new List<Node>() { this };
        List<Node> resultNodes = new List<Node>() { this };

        while(checkingNodes.Count > 0)
        {
            foreach(var item in checkingNodes[0].ConnectedNodes)
            {
                if(!resultNodes.Contains(item))
                {
                    resultNodes.Add(item);
                    checkingNodes.Add(item);
                }
            }

            checkingNodes.Remove(checkingNodes[0]);
        }

        checkingNodes.Clear();

        foreach(var item in resultNodes)
        {
            if(item.IsEndNode)
            { 
                checkingNodes.Add(item);
            }
        }


        if(checkingNodes.Count == 2)
        {
            _highLight.SetActive(true);
            _highLight.GetComponent<SpriteRenderer>().color = GamePlayManager.Instance.GetHighLightColor(colorId);
        }
        else
        {
            _highLight.SetActive(false);
        }
    }
}
