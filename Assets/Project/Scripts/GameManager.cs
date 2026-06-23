using Connect.common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Connect.Core
{
    public class GameManager : MonoBehaviour
    {
        #region START_METHOD
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Init();
                DontDestroyOnLoad(gameObject);
                return;
            }
            else
            {
                Destroy(gameObject);
            }

        }
        private void Init()
        {
            CurrentStage = 1;
            CurrentLevel = 1;

            Levels = new Dictionary<string, LevelData>();

            foreach(var item in _allLevels.levels)
            {
                Levels[item.LevelName] = item;
            }
        }
        #endregion



        #region GAME_VARIABLES
        [HideInInspector] public int CurrentStage;
        [HideInInspector] public int CurrentLevel;
        [HideInInspector] public string StageName;

        public bool IsLevelUnlock(int level)
        {
            string levelName = "Level" + CurrentStage.ToString() + level.ToString();
            if(level == 1)
            {
                PlayerPrefs.SetInt(levelName, 1);
                return true;
            }

            if(PlayerPrefs.HasKey(levelName))
            {
                return PlayerPrefs.GetInt(levelName) == 1;
            }

            PlayerPrefs.SetInt(levelName, 0);
            return false;
        }

        public void UnlockLevel()
        {
            CurrentLevel++;

            if(CurrentLevel > 50)
            {
                CurrentLevel = 1;
                CurrentStage++;

                if(CurrentStage == 8)
                {
                    CurrentStage = 1;
                    GoToMainMenu();
                }
            }

            string levelName = "Level" + CurrentStage.ToString() + CurrentLevel.ToString();

            PlayerPrefs.SetInt(levelName, 1);

        }

        #endregion


        #region LEVEL_DATA
        [SerializeField] private LevelData DefaultLevel;
        [SerializeField] private LevelList _allLevels;

        private Dictionary<string, LevelData> Levels;

        public LevelData GetLevel()
        {
            string levelName = "Level" + CurrentStage.ToString() + CurrentLevel.ToString();
            if (Levels.ContainsKey(levelName))
            {
                return Levels[levelName];
            }
            return DefaultLevel;
        }
        #endregion

        #region SCENE_LOAD
        private const string MAINMENU = "MainMenu";
        private const string GAMEPLAY = "GamePlay";

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(MAINMENU);
        }
        public void GoToGamePlay()
        {
            SceneManager.LoadScene(GAMEPLAY);
        }
        #endregion


    }

}