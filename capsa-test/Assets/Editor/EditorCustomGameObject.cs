using UnityEditor;
using UnityEngine;

public class EditorCustomGameObject : Editor
{
    [MenuItem("JMD_EDITOR/Active Toggle _`")]
    static void ToggleActivationSelection()
    {
        if (Selection.gameObjects != null)
        {
            if (Selection.gameObjects.Length > 1)
            {
                var go = Selection.gameObjects;
                for (int i = 0; i < go.Length; i++)
                {
                    if (go != null)
                        go[i].SetActive(!go[i].activeSelf);
                }
                Undo.RegisterCompleteObjectUndo(go, "Set Active Multiple GO");
            }
            else
            {
                var go = Selection.activeGameObject;
                if (go != null)
                {
                    go.SetActive(!go.activeSelf);
                    Undo.RegisterCompleteObjectUndo(go, "Set Active -> " + go.name);
                }
            }
        }
    }

    [MenuItem("JMD_EDITOR/PositionZero &_`")]
    static void PositionToZero()
    {
        if (Selection.gameObjects != null)
        {
            if (Selection.gameObjects.Length > 1)
            {
                var go = Selection.gameObjects;
                foreach (GameObject g in go)
                {
                    if (go != null)
                        g.transform.position = Vector3.zero;
                }
                Undo.RegisterCompleteObjectUndo(go, "Set Multiple Position GO to Zero");
            }
            else
            {
                var go = Selection.activeGameObject;
                if (go != null)
                {
                    go.transform.position = Vector3.zero;
                    Undo.RegisterCompleteObjectUndo(go, "Set Position Zero -> " + go.name);
                }
            }
        }
    }

    [MenuItem("JMD_EDITOR/Create EmptyGO %#`", false, 10)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("GameObject");
        go.transform.position = Vector3.zero;
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        if (Selection.activeGameObject != null)
            go.transform.SetParent(Selection.activeGameObject.transform);
        Selection.activeObject = go;
    }

    [MenuItem("JMD_EDITOR/Auto Anchor #_`")]
    static void uGUIAnchorAroundObject()
    {
        var o = Selection.activeGameObject;
        if (o != null && o.GetComponent<RectTransform>() != null)
        {
            var r = o.GetComponent<RectTransform>();
            var p = o.transform.parent.GetComponent<RectTransform>();

            var offsetMin = r.offsetMin;
            var offsetMax = r.offsetMax;
            var _anchorMin = r.anchorMin;
            var _anchorMax = r.anchorMax;

            var parent_width = p.rect.width;
            var parent_height = p.rect.height;

            var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x / parent_width),
                                        _anchorMin.y + (offsetMin.y / parent_height));
            var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x / parent_width),
                                        _anchorMax.y + (offsetMax.y / parent_height));

            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;

            r.offsetMin = new Vector2(0, 0);
            r.offsetMax = new Vector2(0, 0);
            r.pivot = new Vector2(0.5f, 0.5f);

        }
    }

    private delegate void ChangePrefab(GameObject go);
    private const int SelectionThresholdForProgressBar = 20;
    private static bool showProgressBar;
    private static int changedObjectsCount;

    [MenuItem("JMD_EDITOR/Apply Changes To Selected Prefabs %j", false, 100)]
    private static void ApplyPrefabs()
    {
        SearchPrefabConnections(ApplyToSelectedPrefabs);
    }

    [MenuItem("JMD_EDITOR/Revert Changes Of Selected Prefabs", false, 100)]
    private static void ResetPrefabs()
    {
        SearchPrefabConnections(RevertToSelectedPrefabs);
    }

    [MenuItem("JMD_EDITOR/Apply Changes To Selected Prefabs #_2", true)]
    [MenuItem("JMD_EDITOR/Revert Changes Of Selected Prefabs #_3", true)]
    private static bool IsSceneObjectSelected()
    {
        return Selection.activeTransform != null;
    }

    private static void SearchPrefabConnections(ChangePrefab changePrefabAction)
    {
        GameObject[] selectedTransforms = Selection.gameObjects;
        int numberOfTransforms = selectedTransforms.Length;
        showProgressBar = numberOfTransforms >= SelectionThresholdForProgressBar;
        changedObjectsCount = 0;
        try
        {
            for (int i = 0; i < numberOfTransforms; i++)
            {
                var go = selectedTransforms[i];
                if (showProgressBar)
                {
                    EditorUtility.DisplayProgressBar("Update prefabs", "Updating prefab " + go.name + " (" + i + "/" + numberOfTransforms + ")",
                        (float)i / (float)numberOfTransforms);
                }
                IterateThroughObjectTree(changePrefabAction, go);
            }
        }
        finally
        {
            if (showProgressBar)
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.LogFormat("{0} Prefab(s) updated", changedObjectsCount);
        }
    }

    private static void IterateThroughObjectTree(ChangePrefab changePrefabAction, GameObject go)
    {
        var prefabType = PrefabUtility.GetPrefabType(go);
        if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
        {
            var prefabRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
            if (prefabRoot != null)
            {
                changePrefabAction(prefabRoot);
                changedObjectsCount++;
                return;
            }
        }
        var transform = go.transform;
        var children = transform.childCount;
        for (int i = 0; i < children; i++)
        {
            var childGo = transform.GetChild(i).gameObject;
            IterateThroughObjectTree(changePrefabAction, childGo);
        }
    }

    private static void ApplyToSelectedPrefabs(GameObject go)
    {
        var prefabAsset = PrefabUtility.GetPrefabParent(go);
        if (prefabAsset == null)
        {
            return;
        }
        PrefabUtility.ReplacePrefab(go, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
    }

    private static void RevertToSelectedPrefabs(GameObject go)
    {
        PrefabUtility.ReconnectToLastPrefab(go);
        PrefabUtility.RevertPrefabInstance(go);
    }

    public static string fileName = "Screenshot ";
    public static int startNumber = 1;

    [MenuItem("JMD_EDITOR/Take Screenshot of Game View #_4")]
    static void TakeScreenshot()
    {
        int number = startNumber;
        string name = "" + number;

        while (System.IO.File.Exists(fileName + name + ".jpg"))
        {
            number++;
            name = "" + number;
        }

        startNumber = number + 1;

        ScreenCapture.CaptureScreenshot(fileName + name + ".jpg");
    }

    [MenuItem("JMD_EDITOR/ActiveToggle  #_q")]
    static void ToggleLockInspector()
    {
        if (ActiveEditorTracker.sharedTracker.isLocked == false)
        {
            ActiveEditorTracker.sharedTracker.isLocked = true;
        }
        else
        {
            ActiveEditorTracker.sharedTracker.isLocked = false;
        }

    }

    [MenuItem("JMD_EDITOR/Clear All Playerprefs")]
    static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

}