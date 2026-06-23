using Connect.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonManager : MonoBehaviour
{
    [SerializeField] Button _button;
    [SerializeField] Image _image;
    [SerializeField] TextMeshProUGUI _levelText;
    [SerializeField] Color _inactiveColor;

    bool IsLevelUnlocked;
    int currentLevel;

    private void Awake()
    {
        _button.onClick.AddListener(Clicked);
    }

    private void OnEnable()
    {
        if (MainMenuManager.instance == null)
        {
            return;
        }
        MainMenuManager.instance.OnStageSelected += LevelOpened;
    }
    private void OnDisable()
    {
        if (MainMenuManager.instance == null)
        {
            return;
        }
        MainMenuManager.instance.OnStageSelected -= LevelOpened;
    }

    // Lay ra ten level sau do tach phan level ra (Level_15 => part [0]= "Level", part[1] = "15" ) sau do chuyen chuoi _levelText sang so nguyen de kiem tra xem da duoc mo khoa chua 
    void LevelOpened()
    {
        string gameObjectName = gameObject.name;
        string[] parts = gameObjectName.Split('_');
        _levelText.text = parts[parts.Length - 1];
        currentLevel  = int.Parse(_levelText.text);
        IsLevelUnlocked = GameManager.Instance.IsLevelUnlock(currentLevel);
        Debug.Log(IsLevelUnlocked);

        _image.color = IsLevelUnlocked ? MainMenuManager.instance.CurrentColor : _inactiveColor ;

    }

    void Clicked()
    {
        if (!IsLevelUnlocked)
        {
            return;
        }

        GameManager.Instance.CurrentLevel = currentLevel;
        GameManager.Instance.GoToGamePlay();
    }
}
