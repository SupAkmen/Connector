using System.Collections.Generic;
using UnityEngine;

namespace Connect.common
{
    [CreateAssetMenu(fileName = "Level", menuName = "SO/AllLevelData")]
    public class LevelList : ScriptableObject
    {
        public List<LevelData> levels;
    }


}
