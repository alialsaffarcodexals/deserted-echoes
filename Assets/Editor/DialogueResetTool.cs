using UnityEditor;
using UnityEngine;

public static class DialogueResetTool
{
    private const string PREFIX = "dlg_seen_";

    [MenuItem("Tools/Dialogue/Reset Level-01 Dialogue")]
    private static void ResetLevel01()
    {
        PlayerPrefs.DeleteKey(PREFIX + "level_01_start");
        PlayerPrefs.Save();
        Debug.Log("[DialogueResetTool] Level-01 dialogue reset — will show again on next entry.");
        EditorUtility.DisplayDialog("Dialogue Reset", "Level-01 dialogue has been reset.\nIt will show again the next time you play Level-01.", "OK");
    }

    [MenuItem("Tools/Dialogue/Reset Level-02 Dialogue")]
    private static void ResetLevel02()
    {
        PlayerPrefs.DeleteKey(PREFIX + "level_02_start");
        PlayerPrefs.Save();
        Debug.Log("[DialogueResetTool] Level-02 dialogue reset — will show again on next entry.");
        EditorUtility.DisplayDialog("Dialogue Reset", "Level-02 dialogue has been reset.\nIt will show again the next time you play Level-02.", "OK");
    }

    [MenuItem("Tools/Dialogue/Reset All Dialogues")]
    private static void ResetAll()
    {
        PlayerPrefs.DeleteKey(PREFIX + "level_01_start");
        PlayerPrefs.DeleteKey(PREFIX + "level_02_start");
        PlayerPrefs.DeleteKey(PREFIX + "game_start");
        PlayerPrefs.Save();
        Debug.Log("[DialogueResetTool] All dialogues reset — each will show again on next entry.");
        EditorUtility.DisplayDialog("Dialogue Reset", "All dialogues have been reset.\nEach will show again on next entry.", "OK");
    }
}
