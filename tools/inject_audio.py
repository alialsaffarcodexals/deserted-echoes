import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
# Config
MUSIC_GUID = 'ee7c4b3255753ca4eb009f8386ed9044'
MUSIC_CLIP_FILEID = 8300000
MUSIC_MIXER_FILEID = 4126225567085503574
MIXER_GUID = '09627e7058d6bc541b9966b80f3f8626'
SFX_MIXER_FILEID = 4084303064337712047
SFX_MIXER_GUID = MIXER_GUID

SCENES = [
    'Assets/Scenes/level-01.unity',
    'Assets/Scenes/level-02.unity',
    'Assets/Scenes/Open-World.unity',
]
PREFAB = 'Assets/Prefabs/PauseMenuCanvas.prefab'
MAIN_MENU = 'Assets/Scenes/main-menu.unity'

BASE_ID = 991000001

def ensure_unique_ids(text, base):
    # find a base such that base, base+1, base+2 are not substrings of text
    cur = base
    while any(str(cur + i) in text for i in range(3)):
        cur += 10
    return cur


def make_blocks(go_id, tr_id, audio_id):
    go_block = f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {audio_id}}}
  m_Layer: 0
  m_Name: negev_desert_music
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
"""

    tr_block = f"""--- !u!4 &{tr_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
"""

    audio_block = f"""--- !u!82 &{audio_id}
AudioSource:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  serializedVersion: 4
  m_audioMixerGroup: {{fileID: {MUSIC_MIXER_FILEID}, guid: {MIXER_GUID}, type: 2}}
  m_audioClip: {{fileID: {MUSIC_CLIP_FILEID}, guid: {MUSIC_GUID}, type: 3}}
  m_PlayOnAwake: 1
  m_Volume: 1
  m_Priority: 128
  m_Pitch: 1
  Loop: 1
  Mute: 0
  Spatialize: 0
  SpatializePostEffects: 0
  Priority: 128
  DopplerLevel: 1
  MinDistance: 1
  MaxDistance: 500
  Pan2D: 0
  rolloffMode: 0
  BypassEffects: 0
  BypassListenerEffects: 0
  BypassReverbZones: 0
"""
    return go_block + tr_block + audio_block


def append_music_to_scene(scene_path, base_id):
    p = ROOT / scene_path
    text = p.read_text(encoding='utf-8')
    base = ensure_unique_ids(text, base_id)
    go_id = base
    tr_id = base + 1
    audio_id = base + 2
    blocks = make_blocks(go_id, tr_id, audio_id)
    # Append blocks at end
    if not text.endswith('\n'):
        text += '\n'
    text += '\n' + blocks + '\n'
    # Update SceneRoots m_RootOrder: find last occurrence of !u!1660057539
    idx = text.rfind('!u!1660057539')
    if idx != -1:
        # try both m_RootOrder and m_Roots (some Unity versions/files use m_Roots)
        for root_key in ('m_RootOrder:', 'm_Roots:'):
            root_order_idx = text.find(root_key, idx)
            if root_order_idx != -1:
                before = text[:root_order_idx]
                after = text[root_order_idx:]
                lines = after.splitlines(True)
                insert_at = 0
                in_list = False
                for i, line in enumerate(lines):
                    if line.strip().startswith(root_key[:-1]):
                        in_list = True
                        insert_at = i + 1
                        continue
                    if in_list:
                        stripped = line.lstrip()
                        if stripped.startswith('- {fileID:'):
                            insert_at = i + 1
                            continue
                        if not stripped.startswith('-') and stripped.strip() != '':
                            break
                insert_line = f'  - {{fileID: {go_id}}}\n'
                lines.insert(insert_at, insert_line)
                new_after = ''.join(lines)
                text = before + new_after
                break
        else:
            print(f"Warning: could not locate m_RootOrder/m_Roots in {scene_path}")
    else:
        print(f"Warning: could not locate SceneRoots block in {scene_path}")
    p.write_text(text, encoding='utf-8', newline='\n')
    print(f'Patched {scene_path}: added GO {go_id}, transform {tr_id}, audio {audio_id}')
    return base + 10


def set_sfx_mixer_for_audiocomponent(file_path, component_fileid):
    p = ROOT / file_path
    text = p.read_text(encoding='utf-8')
    anchor = f'&{component_fileid}'
    idx = text.find(anchor)
    if idx == -1:
        print(f'Warning: component &{component_fileid} not found in {file_path}')
        return
    # find start of component block (line starting with '--- !u!82 &...')
    start = text.rfind('--- !u!82', 0, idx)
    if start == -1:
        start = text.rfind('\n--- !u!82', 0, idx)
    if start == -1:
        print(f'Warning: could not find AudioSource block start for &{component_fileid} in {file_path}')
        return
    # find end of block (next '\n--- !u!')
    end = text.find('\n--- !u!', idx)
    if end == -1:
        end = len(text)
    block = text[start:end]
    # search for OutputAudioMixerGroup or m_OutputAudioMixerGroup
    if 'm_OutputAudioMixerGroup:' in block:
        new_block = block.replace('m_OutputAudioMixerGroup: {fileID: 0}', f'm_OutputAudioMixerGroup: {{fileID: {SFX_MIXER_FILEID}, guid: {SFX_MIXER_GUID}, type: 2}}')
        new_block = new_block.replace('m_OutputAudioMixerGroup: {fileID: 0}\n', f'm_OutputAudioMixerGroup: {{fileID: {SFX_MIXER_FILEID}, guid: {SFX_MIXER_GUID}, type: 2}}\n')
    elif 'OutputAudioMixerGroup:' in block:
        new_block = block.replace('OutputAudioMixerGroup: {fileID: 0}', f'm_OutputAudioMixerGroup: {{fileID: {SFX_MIXER_FILEID}, guid: {SFX_MIXER_GUID}, type: 2}}')
    else:
        # insert after serializedVersion line
        lines = block.splitlines(True)
        insert_at = 0
        for i, line in enumerate(lines):
            if line.strip().startswith('serializedVersion:'):
                insert_at = i + 1
                break
        lines.insert(insert_at, f'  m_OutputAudioMixerGroup: {{fileID: {SFX_MIXER_FILEID}, guid: {SFX_MIXER_GUID}, type: 2}}\n')
        new_block = ''.join(lines)
    new_text = text[:start] + new_block + text[end:]
    p.write_text(new_text, encoding='utf-8', newline='\n')
    print(f'Patched {file_path}: component &{component_fileid} routed to SFX mixer')


if __name__ == '__main__':
    cur_base = BASE_ID
    for scene in SCENES:
        cur_base = append_music_to_scene(scene, cur_base)

    # Update prefab and main-menu
    set_sfx_mixer_for_audiocomponent(PREFAB, 6800239648091958070)
    set_sfx_mixer_for_audiocomponent(MAIN_MENU, 380660471)

    print('Done')
