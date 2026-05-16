"""
fix_zorder_restore.py
1. PauseMenuCanvas.prefab: move ClosePauseButton RT to LAST in PausePanel's m_Children
   so it sits on top of the "Overlay" Image and receives raycasts first.
2. level-01.unity: restore the 3 custom modifications the user set for ClosePauseButton RT
   (m_LocalScale.x=1.6, m_AnchoredPosition.x=255, m_AnchoredPosition.y=274).
"""

import re, os

BASE = r"C:/Users/alooo/Desktop/Deserted Echoes/Deserted Echoes/Assets"
PREFAB = os.path.join(BASE, "Prefabs", "PauseMenuCanvas.prefab")
SCENE  = os.path.join(BASE, "Scenes",  "level-01.unity")

# ── 1. Reorder PausePanel children in PauseMenuCanvas.prefab ─────────────────
with open(PREFAB, "r", encoding="utf-8") as f:
    content = f.read()

# PausePanel RT is &8122102656036329934
# Current children block (ClosePauseButton is first — we want it last)
OLD_CHILDREN = """\
  m_Children:
  - {fileID: 5107421210615095101}
  - {fileID: 1535329917247666656}
  - {fileID: 8385422714597249422}
  - {fileID: 5091459142212323169}
  - {fileID: 1704266180273580696}
  - {fileID: 6634358564410737254}"""

NEW_CHILDREN = """\
  m_Children:
  - {fileID: 1535329917247666656}
  - {fileID: 8385422714597249422}
  - {fileID: 5091459142212323169}
  - {fileID: 1704266180273580696}
  - {fileID: 6634358564410737254}
  - {fileID: 5107421210615095101}"""

assert OLD_CHILDREN in content, "PausePanel children block not found — check prefab YAML"
content = content.replace(OLD_CHILDREN, NEW_CHILDREN, 1)

with open(PREFAB, "w", encoding="utf-8", newline="\n") as f:
    f.write(content)

print("✓ PauseMenuCanvas.prefab: ClosePauseButton moved to last child of PausePanel")

# ── 2. Restore user's custom modifications in level-01.unity ─────────────────
GUID = "5d8e1ef0e3d67d442bade31fddee2bd8"
RT   = "5107421210615095101"

with open(SCENE, "r", encoding="utf-8") as f:
    scene = f.read()

# Check whether the 3 modifications are already present
if f"fileID: {RT}" in scene:
    print("ℹ  level-01.unity already has modifications for ClosePauseButton RT — skipping restore")
else:
    # Insert the 3 modifications just before the m_IsActive entry for PausePanel (5107421210615095172)
    ANCHOR = (
        f"    - target: {{fileID: 5107421210615095172, guid: {GUID}, type: 3}}\n"
        f"      propertyPath: m_IsActive\n"
        f"      value: 0\n"
        f"      objectReference: {{fileID: 0}}"
    )
    INSERT = (
        f"    - target: {{fileID: {RT}, guid: {GUID}, type: 3}}\n"
        f"      propertyPath: m_LocalScale.x\n"
        f"      value: 1.6\n"
        f"      objectReference: {{fileID: 0}}\n"
        f"    - target: {{fileID: {RT}, guid: {GUID}, type: 3}}\n"
        f"      propertyPath: m_AnchoredPosition.x\n"
        f"      value: 255\n"
        f"      objectReference: {{fileID: 0}}\n"
        f"    - target: {{fileID: {RT}, guid: {GUID}, type: 3}}\n"
        f"      propertyPath: m_AnchoredPosition.y\n"
        f"      value: 274\n"
        f"      objectReference: {{fileID: 0}}\n"
        + ANCHOR
    )
    assert ANCHOR in scene, "Anchor text not found in level-01.unity — cannot restore modifications"
    scene = scene.replace(ANCHOR, INSERT, 1)
    with open(SCENE, "w", encoding="utf-8", newline="\n") as f:
        f.write(scene)
    print("✓ level-01.unity: restored m_LocalScale.x=1.6, m_AnchoredPosition=(255,274) for ClosePauseButton RT")

print("Done.")
