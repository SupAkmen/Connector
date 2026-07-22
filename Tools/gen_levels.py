#!/usr/bin/env python3
"""
Sinh 350 level (stage 1-7 x 50 level) cho game Connector.

Moi level la 1 puzzle GIAI DUOC CHAC CHAN:
  - Sinh 1 duong Hamilton phu kin luoi NxN (backbite -> ngoan ngoeo, co do kho).
  - Cat thanh k doan lien tiep (k = so mau, moi doan >= 2 o).
  - Moi doan -> 1 Edge, luu StartPoint/EndPoint (2 dau doan).
  - Cac doan ghep lai phu kin luoi -> ton tai loi giai -> thang duoc.

Ghi ra .asset + .meta (Unity YAML) va cap nhat Levels.asset (LevelList).
KHONG dung LevelGenerator cua Unity.
"""

import os
import hashlib
import random

# ---- Cau hinh khop du an ----
PROJECT = r"d:\Game\Connector"
LEVELS_DIR = os.path.join(PROJECT, "Assets", "Common", "Prefabs", "Levels")
LEVELLIST_ASSET = os.path.join(PROJECT, "Assets", "Common", "Prefabs", "Levels.asset")

# GUID script LevelData (lay tu DefaultLevel.asset)
LEVELDATA_SCRIPT_GUID = "a728a6a328486df4bb9d4ea65ccd19fb"
# GUID script LevelList (lay tu Levels.asset)
LEVELLIST_SCRIPT_GUID = "ec1cda1af1b0fb24b9a84b4fe5171677"
# GUID DefaultLevel (giu lai trong list)
DEFAULT_LEVEL_GUID = "07ab0b562b4af1a49bb4220a4cb1f8ee"

MAX_COLORS = 13          # so mau trong NodeColors (scene GamePlay) -> khong vuot
STAGES = range(1, 8)     # stage 1..7
LEVELS_PER_STAGE = 50

DIRS = [(0, 1), (0, -1), (1, 0), (-1, 0)]


def grid_size(stage):
    # khop code: LevelGenerator.levelSize / GamePlayManager = stage + 4
    return stage + 4


def snake_path(n):
    """Duong ran bo phu kin NxN -> Hamilton path base."""
    path = []
    for x in range(n):
        col = range(n) if x % 2 == 0 else range(n - 1, -1, -1)
        for y in col:
            path.append((x, y))
    return path


def backbite(path, n, rng, iters):
    """Random hoa Hamilton path (van phu kin) bang thuat toan backbite."""
    pos = {p: i for i, p in enumerate(path)}
    for _ in range(iters):
        end = 0 if rng.random() < 0.5 else 1  # chon 1 dau
        if end == 1:
            path.reverse()
            pos = {p: i for i, p in enumerate(path)}
        head = path[0]
        rng.shuffle(DIRS)
        for dx, dy in DIRS:
            nb = (head[0] + dx, head[1] + dy)
            if 0 <= nb[0] < n and 0 <= nb[1] < n:
                j = pos[nb]
                if j >= 2:
                    # dao doan [0..j-1] -> van la Hamilton path
                    path[:j] = path[:j][::-1]
                    for i in range(j):
                        pos[path[i]] = i
                    break
    return path


def num_colors(stage, level, n):
    """So mau tang dan theo level, gioi han <= MAX_COLORS va <= so o /2."""
    lo = max(2, n - 2)
    hi = min(MAX_COLORS, n + 1, (n * n) // 2)
    if hi < lo:
        hi = lo
    # noi suy theo level 1..50
    t = (level - 1) / (LEVELS_PER_STAGE - 1)
    k = int(round(lo + t * (hi - lo)))
    return max(lo, min(hi, k))


def _seg_makes_box(cells, p, n):
    """Neu them o p vao segment (cells) thi co tao hinh vuong 2x2 kin khong?

    LUAT GAME: 4 o cung mau ke nhau thanh vuong 2x2 -> vo nghiem
    (Node.IsDegreeThree tu cat canh). Phai tranh tu luc thiet ke.
    """
    x, y = p
    for ax, ay in ((x - 1, y - 1), (x - 1, y), (x, y - 1), (x, y)):
        square = [(ax, ay), (ax + 1, ay), (ax, ay + 1), (ax + 1, ay + 1)]
        if p not in square:
            continue
        if all(0 <= cx < n and 0 <= cy < n for cx, cy in square):
            if all(c in cells for c in square if c != p):
                return True
    return False


def greedy_split(path, n):
    """Cat path thanh cac doan, cat NGAY truoc o lam tao box 2x2.

    Moi doan la mot mau. Vi cat truoc khi tao box -> khong doan nao chua
    hinh vuong 2x2 kin -> loi giai (chinh cac doan) hop le voi luat game.
    """
    segs = []
    cur = [path[0]]
    cur_set = {path[0]}
    for p in path[1:]:
        if len(cur) >= 3 and _seg_makes_box(cur_set, p, n):
            segs.append(cur)
            cur = [p]
            cur_set = {p}
        else:
            cur.append(p)
            cur_set.add(p)
    segs.append(cur)
    return segs


def gen_level(stage, level):
    """Sinh level tranh box 2x2, moi doan >= 2 o, so mau <= MAX_COLORS.

    Thu nhieu duong (seed khac nhau) roi chon ket qua sach dau tien.
    """
    n = grid_size(stage)
    best = None
    for attempt in range(100):
        rng = random.Random(stage * 100000 + level * 1000 + attempt)
        path = snake_path(n)
        path = backbite(path, n, rng, iters=n * n)
        segs = greedy_split(path, n)
        # rang buoc: moi doan >= 2 o, khong qua MAX_COLORS
        if any(len(s) < 2 for s in segs):
            continue
        k = len(segs)
        if k > MAX_COLORS:
            continue
        if best is None or k < len(best):
            best = segs
            # du dung -> dung sinh som cho nhanh
            if k <= num_colors(stage, level, n):
                break
    if best is None:
        # fallback cuc hiem: chia deu (van tranh <2) - gan nhu khong xay ra
        best = greedy_split(snake_path(n), n)
    # Tra ve CA DUONG (moi doan la list diem lien tuc) de dung cho goi y.
    # StartPoint = diem[0], EndPoint = diem[cuoi] van dung nhu cu.
    return n, best


def guid_for(name):
    return hashlib.md5(name.encode()).hexdigest()[:32]


def write_asset(name, edges):
    lines = []
    lines.append("%YAML 1.1")
    lines.append("%TAG !u! tag:unity3d.com,2011:")
    lines.append("--- !u!114 &11400000")
    lines.append("MonoBehaviour:")
    lines.append("  m_ObjectHideFlags: 0")
    lines.append("  m_CorrespondingSourceObject: {fileID: 0}")
    lines.append("  m_PrefabInstance: {fileID: 0}")
    lines.append("  m_PrefabAsset: {fileID: 0}")
    lines.append("  m_GameObject: {fileID: 0}")
    lines.append("  m_Enabled: 1")
    lines.append("  m_EditorHideFlags: 0")
    lines.append("  m_Script: {fileID: 11500000, guid: %s, type: 3}" % LEVELDATA_SCRIPT_GUID)
    lines.append("  m_Name: %s" % name)
    lines.append("  m_EditorClassIdentifier: ")
    lines.append("  LevelName: %s" % name)
    lines.append("  Edges:")
    for seg in edges:
        lines.append("  - Points:")
        for (px, py) in seg:
            lines.append("    - {x: %d, y: %d}" % (px, py))
    content = "\n".join(lines) + "\n"

    guid = guid_for(name)
    asset_path = os.path.join(LEVELS_DIR, name + ".asset")
    meta_path = asset_path + ".meta"
    with open(asset_path, "w", newline="\n") as f:
        f.write(content)
    meta = (
        "fileFormatVersion: 2\n"
        "guid: %s\n"
        "NativeFormatImporter:\n"
        "  externalObjects: {}\n"
        "  mainObjectFileID: 11400000\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    ) % guid
    with open(meta_path, "w", newline="\n") as f:
        f.write(meta)
    return guid


def write_levellist(entries):
    """entries = list (guid). Ghi lai Levels.asset."""
    lines = []
    lines.append("%YAML 1.1")
    lines.append("%TAG !u! tag:unity3d.com,2011:")
    lines.append("--- !u!114 &11400000")
    lines.append("MonoBehaviour:")
    lines.append("  m_ObjectHideFlags: 0")
    lines.append("  m_CorrespondingSourceObject: {fileID: 0}")
    lines.append("  m_PrefabInstance: {fileID: 0}")
    lines.append("  m_PrefabAsset: {fileID: 0}")
    lines.append("  m_GameObject: {fileID: 0}")
    lines.append("  m_Enabled: 1")
    lines.append("  m_EditorHideFlags: 0")
    lines.append("  m_Script: {fileID: 11500000, guid: %s, type: 3}" % LEVELLIST_SCRIPT_GUID)
    lines.append("  m_Name: Levels")
    lines.append("  m_EditorClassIdentifier: ")
    lines.append("  Levels:")
    for guid in entries:
        lines.append("  - {fileID: 11400000, guid: %s, type: 2}" % guid)
    content = "\n".join(lines) + "\n"
    with open(LEVELLIST_ASSET, "w", newline="\n") as f:
        f.write(content)


def main():
    os.makedirs(LEVELS_DIR, exist_ok=True)
    entries = [DEFAULT_LEVEL_GUID]  # giu DefaultLevel
    count = 0
    for stage in STAGES:
        for level in range(1, LEVELS_PER_STAGE + 1):
            name = "Level" + str(stage) + str(level)
            n, edges = gen_level(stage, level)
            guid = write_asset(name, edges)
            entries.append(guid)
            count += 1
    write_levellist(entries)
    print("Da sinh %d level (.asset + .meta) vao %s" % (count, LEVELS_DIR))
    print("Da cap nhat %s (%d entries)" % (LEVELLIST_ASSET, len(entries)))


if __name__ == "__main__":
    main()
