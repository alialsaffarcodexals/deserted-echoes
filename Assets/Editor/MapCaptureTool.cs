using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class MapCaptureTool : EditorWindow
{
    private static readonly string[] ScenesToProcess = new string[]
    {
        "Assets/Scenes/level-05.unity",
        "Assets/Scenes/level-06.unity",
        "Assets/Scenes/level-07.unity",
        "Assets/Scenes/level-08.unity",
        "Assets/Scenes/level-11.unity",
        "Assets/Scenes/House-Interior.unity"
    };

    private static readonly string OutputFolder = "Assets/Textures/UI/";

    [MenuItem("Tools/Capture All Maps")]
    public static void CaptureAll()
    {
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        foreach (string scenePath in ScenesToProcess)
        {
            CaptureScene(scenePath);
        }

        AssetDatabase.Refresh();
        Debug.Log("All maps captured successfully.");
    }

    private static void CaptureScene(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        // Find bounds
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"No renderers found in {scenePath}");
            return;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            if (r.gameObject.layer == LayerMask.NameToLayer("UI")) continue;
            bounds.Encapsulate(r.bounds);
        }

        // Setup Camera
        GameObject camObj = new GameObject("TempCaptureCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.transform.rotation = Quaternion.Euler(90, 0, 0); // Top-down for 3D or identity for 2D?
        
        // Check if 2D based on Z range
        float zRange = bounds.max.z - bounds.min.z;
        if (zRange < 5f) // Likely 2D
        {
            cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.orthographicSize = bounds.size.y / 2f;
            cam.aspect = bounds.size.x / bounds.size.y;
        }
        else // Likely 3D
        {
            cam.transform.position = new Vector3(bounds.center.x, bounds.max.y + 100f, bounds.center.z);
            cam.transform.rotation = Quaternion.Euler(90, 0, 0);
            cam.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.z) / 2f;
            cam.aspect = bounds.size.x / bounds.size.z;
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));

        // Render
        int width = 4096;
        int height = Mathf.RoundToInt(width / cam.aspect);
        if (height % 2 != 0) height++;

        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        // Save
        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
        string savePath = $"{OutputFolder}Map_{sceneName}.png";
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);

        // Cleanup
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenShot);
        Object.DestroyImmediate(camObj);

        Debug.Log($"Captured: {savePath}");
    }
}
