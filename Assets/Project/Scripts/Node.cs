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
        // Connected node already exist 
        // Delete the edges and the parts
        if(ConnectedNodes.Contains(connectedNode))
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
        if(connectedNode.ConnectedNodes.Count == 2)
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

        if(startNode .IsConnectedToEndNode())
        {
            return;
        }

        while(startNode != null)
        {
            Node tempNode = null;
            if(startNode.ConnectedNodes.Count != 0)
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

        if(IsEndNode)
        {
            return true;
        }

        foreach(var item in ConnectedNodes)
        {
            if(!checkNode.Contains(item))
            {
                checkNode.Add(item);
                return item.IsConnectedToEndNode(checkNode);
            }
        }

        return false;
    }
}
