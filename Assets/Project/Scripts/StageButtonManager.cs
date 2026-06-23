using Connect.Core;
using UnityEngine;
using UnityEngine.UI;

public class StageButtonManager : MonoBehaviour
{
    [SerializeField] string _stageName;
    [SerializeField] Color _stageColor;
    [SerializeField] int _stageNumber;
    [SerializeField] Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(ClickedButton);
    }

    void ClickedButton()
    {
       
       GameManager.Instance.CurrentStage = _stageNumber;
       GameManager.Instance.StageName = _stageName;
       MainMenuManager.instance.ClickedStage(_stageName,_stageColor);
    }
}
