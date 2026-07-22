using System.Collections.Generic;
using Connect.common;
using UnityEditor;
using UnityEngine;

namespace Connect.EditorTools
{
    /// <summary>
    /// Tool sinh level trong Editor. Menu: Tools > Connector > Level Generator.
    /// Sinh puzzle GIAI DUOC CHAC CHAN: cat tu 1 duong Hamilton phu kin luoi.
    /// Khong dung LevelGenerator runtime cua scene.
    /// </summary>
    public class LevelGeneratorTool : EditorWindow
    {
        // ---- Cau hinh ----
        private int _stageFrom = 1;
        private int _stageTo = 7;
        private int _levelsPerStage = 50;
        private int _maxColors = 13;          // so mau trong NodeColors (scene GamePlay)
        private int _seed = 0;                // 0 = deterministic theo stage/level
        private bool _overwrite = true;
        private LevelList _levelList;
        private Object _defaultLevel;         // giu lai o dau list (tuy chon)

        private const string LEVELS_FOLDER = "Assets/Common/Prefabs/Levels";

        private static readonly Vector2Int[] DIRS =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        [MenuItem("Tools/Connector/Level Generator")]
        public static void Open()
        {
            var w = GetWindow<LevelGeneratorTool>("Level Generator");
            w.minSize = new Vector2(360, 340);
        }

        private void OnEnable()
        {
            // tu tim LevelList neu chua gan
            if (_levelList == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:LevelList");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _levelList = AssetDatabase.LoadAssetAtPath<LevelList>(path);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sinh level (puzzle giai duoc)", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _levelList = (LevelList)EditorGUILayout.ObjectField("Level List", _levelList, typeof(LevelList), false);

            _defaultLevel = EditorGUILayout.ObjectField("Default Level (giu dau list)", _defaultLevel, typeof(LevelData), false);
            EditorGUILayout.Space();

            _stageFrom = EditorGUILayout.IntSlider("Stage tu", _stageFrom, 1, 7);
            _stageTo = EditorGUILayout.IntSlider("Stage den", _stageTo, 1, 7);
            _levelsPerStage = EditorGUILayout.IntField("Level moi stage", _levelsPerStage);
            _maxColors = EditorGUILayout.IntField("So mau toi da", _maxColors);
            _seed = EditorGUILayout.IntField("Seed (0 = theo level)", _seed);
            _overwrite = EditorGUILayout.Toggle("Ghi de neu ton tai", _overwrite);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Luoi moi stage = stage + 4 (5x5 .. 11x11).\n" +
                "Level dau stage it mau, cuoi stage nhieu mau.\n" +
                "Moi level cat tu duong Hamilton phu kin -> chac chan giai duoc.",
                MessageType.Info);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_levelList == null))
            {
                if (GUILayout.Button("Sinh level", GUILayout.Height(32)))
                {
                    Generate();
                }
            }
            if (_levelList == null)
            {
                EditorGUILayout.HelpBox("Gan Level List truoc khi sinh.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Xoa het level trong list (tru Default)"))
            {
                if (EditorUtility.DisplayDialog("Xac nhan",
                    "Xoa het level asset (tru Default Level) va lam sach list?", "Xoa", "Huy"))
                {
                    ClearLevels();
                }
            }
        }

        // ---------------- Sinh ----------------

        private void Generate()
        {
            if (!AssetDatabase.IsValidFolder(LEVELS_FOLDER))
            {
                CreateFolderRecursive(LEVELS_FOLDER);
            }

            var entries = new List<LevelData>();
            if (_defaultLevel is LevelData dl)
            {
                entries.Add(dl);
            }

            int total = 0;
            int sFrom = Mathf.Min(_stageFrom, _stageTo);
            int sTo = Mathf.Max(_stageFrom, _stageTo);

            try
            {
                AssetDatabase.StartAssetEditing();

                for (int stage = sFrom; stage <= sTo; stage++)
                {
                    for (int level = 1; level <= _levelsPerStage; level++)
                    {
                        string name = "Level" + stage + level;
                        string assetPath = LEVELS_FOLDER + "/" + name + ".asset";

                        EditorUtility.DisplayProgressBar("Sinh level",
                            name, (float)total / ((sTo - sFrom + 1) * _levelsPerStage));

                        var data = BuildLevel(stage, level, name);

                        var existing = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
                        if (existing != null)
                        {
                            if (_overwrite)
                            {
                                existing.LevelName = data.LevelName;
                                existing.Edges = data.Edges;
                                EditorUtility.SetDirty(existing);
                                entries.Add(existing);
                            }
                            else
                            {
                                entries.Add(existing);
                            }
                        }
                        else
                        {
                            AssetDatabase.CreateAsset(data, assetPath);
                            entries.Add(data);
                        }
                        total++;
                    }
                }

                // cap nhat LevelList
                _levelList.Levels = entries;
                EditorUtility.SetDirty(_levelList);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[LevelGenerator] Da sinh {total} level, list co {entries.Count} muc.");
            EditorUtility.DisplayDialog("Xong",
                $"Da sinh {total} level.\nLevel List: {entries.Count} muc.", "OK");
        }

        private LevelData BuildLevel(int stage, int level, string name)
        {
            int n = stage + 4; // khop code runtime
            int wanted = NumColors(stage, level, n);

            // Thu nhieu duong -> chon ket qua sach dau tien:
            //  - khong hinh vuong 2x2 kin (luat game)
            //  - moi doan >= 2 o
            //  - so mau <= _maxColors
            List<List<Vector2Int>> best = null;
            for (int attempt = 0; attempt < 100; attempt++)
            {
                int levelSeed = _seed != 0
                    ? (_seed * 1000000 + stage * 100000 + level * 1000 + attempt)
                    : (stage * 100000 + level * 1000 + attempt);
                var rng = new System.Random(levelSeed);

                List<Vector2Int> path = SnakePath(n);
                Backbite(path, n, rng, n * n);

                List<List<Vector2Int>> segs = GreedySplit(path, n);

                bool tooShort = false;
                foreach (var s in segs) { if (s.Count < 2) { tooShort = true; break; } }
                if (tooShort) continue;
                if (segs.Count > _maxColors) continue;

                if (best == null || segs.Count < best.Count)
                {
                    best = segs;
                    if (segs.Count <= wanted) break; // du tot -> dung
                }
            }
            if (best == null)
            {
                best = GreedySplit(SnakePath(n), n); // fallback cuc hiem
            }

            var data = ScriptableObject.CreateInstance<LevelData>();
            data.LevelName = name;
            data.Edges = new List<Edge>();
            foreach (var seg in best)
            {
                // Luu CA DUONG (moi diem cua doan) de dung cho goi y (hint).
                // StartPoint = Points[0], EndPoint = Points[cuoi] van dung nhu cu.
                var e = new Edge { Points = new List<Vector2Int>(seg) };
                data.Edges.Add(e);
            }
            return data;
        }

        /// <summary>
        /// Neu them o p vao segment cells thi co tao hinh vuong 2x2 kin khong?
        /// </summary>
        private static bool SegMakesBox(HashSet<Vector2Int> cells, Vector2Int p, int n)
        {
            var corners = new[]
            {
                new Vector2Int(p.x - 1, p.y - 1),
                new Vector2Int(p.x - 1, p.y),
                new Vector2Int(p.x, p.y - 1),
                new Vector2Int(p.x, p.y),
            };
            foreach (var c in corners)
            {
                var sq = new[]
                {
                    new Vector2Int(c.x, c.y),
                    new Vector2Int(c.x + 1, c.y),
                    new Vector2Int(c.x, c.y + 1),
                    new Vector2Int(c.x + 1, c.y + 1),
                };

                bool inGrid = true;
                foreach (var s in sq)
                    if (s.x < 0 || s.x >= n || s.y < 0 || s.y >= n) { inGrid = false; break; }
                if (!inGrid) continue;

                bool contains = System.Array.IndexOf(sq, p) >= 0;
                if (!contains) continue;

                bool othersIn = true;
                foreach (var s in sq)
                    if (s != p && !cells.Contains(s)) { othersIn = false; break; }
                if (othersIn) return true;
            }
            return false;
        }

        /// <summary>
        /// Cat path thanh cac doan, cat ngay truoc o lam tao box 2x2.
        /// </summary>
        private static List<List<Vector2Int>> GreedySplit(List<Vector2Int> path, int n)
        {
            var segs = new List<List<Vector2Int>>();
            var cur = new List<Vector2Int> { path[0] };
            var curSet = new HashSet<Vector2Int> { path[0] };
            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int p = path[i];
                if (cur.Count >= 3 && SegMakesBox(curSet, p, n))
                {
                    segs.Add(cur);
                    cur = new List<Vector2Int> { p };
                    curSet = new HashSet<Vector2Int> { p };
                }
                else
                {
                    cur.Add(p);
                    curSet.Add(p);
                }
            }
            segs.Add(cur);
            return segs;
        }

        // ---------------- Thuat toan (port tu Python) ----------------

        private static List<Vector2Int> SnakePath(int n)
        {
            var path = new List<Vector2Int>(n * n);
            for (int x = 0; x < n; x++)
            {
                if (x % 2 == 0)
                    for (int y = 0; y < n; y++) path.Add(new Vector2Int(x, y));
                else
                    for (int y = n - 1; y >= 0; y--) path.Add(new Vector2Int(x, y));
            }
            return path;
        }

        private static void Backbite(List<Vector2Int> path, int n, System.Random rng, int iters)
        {
            var pos = new Dictionary<Vector2Int, int>();
            for (int i = 0; i < path.Count; i++) pos[path[i]] = i;

            var dirs = new List<Vector2Int>(DIRS);

            for (int it = 0; it < iters; it++)
            {
                if (rng.NextDouble() < 0.5)
                {
                    path.Reverse();
                    for (int i = 0; i < path.Count; i++) pos[path[i]] = i;
                }

                Vector2Int head = path[0];
                Shuffle(dirs, rng);

                foreach (var d in dirs)
                {
                    Vector2Int nb = head + d;
                    if (nb.x < 0 || nb.x >= n || nb.y < 0 || nb.y >= n) continue;
                    int j = pos[nb];
                    if (j >= 2)
                    {
                        path.Reverse(0, j);
                        for (int i = 0; i < j; i++) pos[path[i]] = i;
                        break;
                    }
                }
            }
        }

        private int NumColors(int stage, int level, int n)
        {
            int lo = Mathf.Max(2, n - 2);
            int hi = Mathf.Min(_maxColors, Mathf.Min(n + 1, (n * n) / 2));
            if (hi < lo) hi = lo;
            float t = (level - 1) / (float)(_levelsPerStage - 1);
            int k = Mathf.RoundToInt(lo + t * (hi - lo));
            return Mathf.Clamp(k, lo, hi);
        }

        private static void Shuffle<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // ---------------- Tien ich ----------------

        private void ClearLevels()
        {
            var keep = _defaultLevel as LevelData;
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { LEVELS_FOLDER });
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var g in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(g);
                    var d = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                    if (keep != null && d == keep) continue;
                    if (d != null && d.LevelName == "DefaultLevel") continue;
                    AssetDatabase.DeleteAsset(path);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (_levelList != null)
            {
                _levelList.Levels = new List<LevelData>();
                if (keep != null) _levelList.Levels.Add(keep);
                EditorUtility.SetDirty(_levelList);
                AssetDatabase.SaveAssets();
            }
            Debug.Log("[LevelGenerator] Da xoa level (giu Default).");
        }

        private static void CreateFolderRecursive(string folder)
        {
            string[] parts = folder.Split('/');
            string cur = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
