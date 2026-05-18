from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
SCENE_UPDATES = {
    'Assets/Scenes/level-01.unity': 991000001,
    'Assets/Scenes/level-02.unity': 991000011,
    'Assets/Scenes/Open-World.unity': 991000021,
}

for scene, go_id in SCENE_UPDATES.items():
    p = ROOT / scene
    text = p.read_text(encoding='utf-8')
    idx = text.rfind('!u!1660057539')
    if idx == -1:
        print(f'SceneRoots not found in {scene}')
        continue
    # try to find m_Roots or m_RootOrder
    for root_key in ('m_Roots:', 'm_RootOrder:'):
        kidx = text.find(root_key, idx)
        if kidx != -1:
            before = text[:kidx]
            after = text[kidx:]
            lines = after.splitlines(True)
            # collect existing fileIDs in that list
            existing = set()
            in_list = False
            insert_at = 0
            for i, line in enumerate(lines):
                if line.strip().startswith(root_key[:-1]):
                    in_list = True
                    insert_at = i + 1
                    continue
                if in_list:
                    stripped = line.lstrip()
                    if stripped.startswith('- {fileID:'):
                        # parse fileID
                        fid = stripped.split(':',1)[1].strip().strip('}').strip()
                        try:
                            existing.add(int(fid))
                        except:
                            pass
                        insert_at = i + 1
                        continue
                    if not stripped.startswith('-') and stripped.strip() != '':
                        break
            if go_id in existing:
                print(f'{go_id} already present in {scene} under {root_key}')
                break
            insert_line = f'  - {{fileID: {go_id}}}\n'
            lines.insert(insert_at, insert_line)
            new_after = ''.join(lines)
            new_text = before + new_after
            p.write_text(new_text, encoding='utf-8', newline='\n')
            print(f'Inserted {go_id} into {scene} under {root_key}')
            break
    else:
        print(f'No m_Roots/m_RootOrder found in {scene}')
