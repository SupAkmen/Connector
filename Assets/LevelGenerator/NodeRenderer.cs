using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class NodeRenderer : MonoBehaviour
{
    [SerializeField] private List<Color> NodeColors;

    [SerializeField] private GameObject _point;
    [SerializeField] private GameObject _topEdge;
    [SerializeField] private GameObject _botEdge;
    [SerializeField] private GameObject _rightEge;
    [SerializeField] private GameObject _leftEdge;

    public void Init()
    {
        _point.SetActive(false);
        _topEdge.SetActive(false);
        _botEdge.SetActive(false);
        _rightEge.SetActive(false);
        _leftEdge.SetActive(false);
    }

    public void SetEdge(int colorID,Vector2 direction)
    {
        GameObject connectedNode = _point;

        if(direction == Vector2Int.up)
        {
            connectedNode = _topEdge;
        }
        else if(direction == Vector2Int.down)
        {
            connectedNode = _botEdge;
        }
        else if(direction == Vector2Int.left)
        {
            connectedNode = _leftEdge;
        }
        else if(direction == Vector2Int.right)
        {
            connectedNode = _rightEge ;
        }

        connectedNode.SetActive(true);
        connectedNode.GetComponent<SpriteRenderer>().color = NodeColors[colorID];

    }
}
