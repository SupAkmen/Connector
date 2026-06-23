using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;

    [SerializeField] GameObject _titlePanel;
    [SerializeField] GameObject _stagePanel;
    [SerializeField] GameObject _levelPanel;

    private void Awake()
    {
        instance = this;

        _titlePanel.SetActive(true);
        _stagePanel.SetActive(false);
        _levelPanel.SetActive(false);
    }

    public void ClickedPlay()
    {
        _titlePanel.SetActive(false);
        _stagePanel.SetActive(true);
    }

    public void ClickedBackToTitle()
    {
        _titlePanel.SetActive(true);
        _stagePanel.SetActive(false);
    }

    public void ClickedBackToStage()
    {
        _stagePanel.SetActive(true);
        _levelPanel.SetActive(false);
    }

    public UnityAction OnStageSelected;

    [HideInInspector] public Color CurrentColor;
    [SerializeField] TextMeshProUGUI _levelTittleText;
    [SerializeField] Image _levelTittleImage;

    public void ClickedStage(string stageName, Color stageColor)
    {
        _stagePanel.SetActive(false);
        _levelPanel.SetActive(true);
        CurrentColor = stageColor;
        _levelTittleText.text = stageName;
        _levelTittleImage.color = CurrentColor;
        OnStageSelected?.Invoke();
    }
}
