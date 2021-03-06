﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class ScriptableObjectWindow : EditorWindow
{
    [MenuItem("Window/Scriptable objects")]
    public static void Init()
    {
        GetWindow<ScriptableObjectWindow>("Scriptable objects", true);
    }

    private void OnEnable()
    {
        CreateNewObjectMenu();
    }

    private static void CreateNewObjectMenu()
    {
        newObjectMenu = new GenericMenu();

        var referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (var i = 0; i < referencedAssemblies.Length; i++)
        {
            var assemblyName = referencedAssemblies[i].FullName;
            if(!(assemblyName.StartsWith("Assembly") && (assemblyName.StartsWith("Assembly-CSharp-Editor") ||
                                                         assemblyName.StartsWith("Assembly-CSharp-Editor-firstpass") ||
                                                         assemblyName.StartsWith("Assembly-UnityScript-Editor") ||
                                                         assemblyName.StartsWith(
                                                             "Assembly-UnityScript-Editor-firstpass") ||
                                                         assemblyName.StartsWith("Assembly-Boo-Editor") ||
                                                         assemblyName.StartsWith("Assembly-Boo-Editor-firstpass"))))
                foreach (var t in referencedAssemblies[i].GetTypes())
                    if(typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
                        if(t.Namespace == null || !(t.Namespace.StartsWith("UnityEditorInternal") ||
                                                    t.Namespace.StartsWith("UnityEditor") ||
                                                    t.Namespace.StartsWith("UnityEngine") ||
                                                    t.Namespace.StartsWith("Unity") ||
                                                    t.Namespace.StartsWith("TreeEditor")))
                            newObjectMenu.AddItem(new GUIContent(t.FullName.Replace('.', '/')), false,
                                CreateNewObjectHandler, t);
        }
    }

    private static void CreateNewObjectHandler(object data)
    {
        CreateInstance((Type) data);
    }

    private void OnInspectorUpdate()
    {
        dirty = true;
        Repaint();
    }

    private void OnGUI()
    {
        if(dirty)
        {
            dirty = false;

            if(sceneScriptableObjects == null)
                sceneScriptableObjects = new Dictionary<string, ScriptableObject[]>();
            else
                sceneScriptableObjects.Clear();

            numSceneObjects = 0;

            if(orphanObjectSet == null)
                orphanObjectSet = new HashSet<ScriptableObject>();
            else
                orphanObjectSet.Clear();

            if(assetObjectSet == null)
                assetObjectSet = new HashSet<ScriptableObject>();
            else
                assetObjectSet.Clear();

            if(fullSerializedComponentCache == null)
                fullSerializedComponentCache = new Dictionary<Component, SerializedObject>();
            if(fullSerializedObjectCache == null)
                fullSerializedObjectCache = new Dictionary<Object, SerializedObject>();

            foreach (ScriptableObject o in FindObjectsOfType(typeof(ScriptableObject)))
                if(!AssetDatabase.Contains(o))
                    orphanObjectSet.Add(o);

            if(allSerializedObjects == null)
                allSerializedObjects = new Dictionary<string, SerializedObject[]>();
            else
                allSerializedObjects.Clear();

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var ro = scene.GetRootGameObjects();
                if(components == null)
                    components = new List<Component>();
                else
                    components.Clear();

                for (var j = 0; j < ro.Length; j++)
                {
                    var go = ro[j];
                    components.AddRange(go.GetComponentsInChildren<Component>(true));
                }

                SerializedObject[] aso;
                if(!allSerializedObjects.ContainsKey(scene.name))
                {
                    aso = allSerializedObjects[scene.name] = new SerializedObject[components.Count];
                }
                else
                {
                    var prevSerializedObjects = allSerializedObjects[scene.name];
                    if(prevSerializedObjects.Length != components.Count)
                        aso = allSerializedObjects[scene.name] = new SerializedObject[components.Count];
                    else
                        aso = prevSerializedObjects;
                }

                for (var j = 0; j < components.Count; j++)
                {
                    var c = components[j];
                    SerializedObject cachedSO;
                    if(fullSerializedComponentCache.TryGetValue(c, out cachedSO))
                    {
                        cachedSO.Update();
                        aso[j] = cachedSO;
                    }
                    else
                    {
                        fullSerializedComponentCache[c] = aso[j] = new SerializedObject(c);
                    }
                }
            }

            var allSerializedObjectsEnumerator = allSerializedObjects.GetEnumerator();
            try
            {
                while (allSerializedObjectsEnumerator.MoveNext())
                {
                    var sceneSerializedObjects = allSerializedObjectsEnumerator.Current;
                    if(finalResult == null)
                        finalResult = new List<ScriptableObject>();
                    else
                        finalResult.Clear();

                    if(checkedObjects == null)
                        checkedObjects = new HashSet<Object>();
                    else
                        checkedObjects.Clear();

                    if(currentSerializedObjects == null)
                    {
                        currentSerializedObjects = new List<SerializedObject>(sceneSerializedObjects.Value);
                    }
                    else
                    {
                        currentSerializedObjects.Clear();
                        for (var i = 0; i < sceneSerializedObjects.Value.Length; i++)
                            currentSerializedObjects.Add(sceneSerializedObjects.Value[i]);
                    }

                    while (currentSerializedObjects.Count > 0)
                    {
                        if(nextSerializedObjects == null)
                            nextSerializedObjects = new List<SerializedObject>();
                        else
                            nextSerializedObjects.Clear();
                        for (var i = 0; i < currentSerializedObjects.Count; i++)
                        {
                            var so = currentSerializedObjects[i];
                            if(!checkedObjects.Contains(so.targetObject))
                            {
                                checkedObjects.Add(so.targetObject);
                                var sp = so.GetIterator();
                                do
                                {
                                    if(sp.propertyType == SerializedPropertyType.ObjectReference)
                                    {
                                        var scriptableObjectReference = sp.objectReferenceValue as ScriptableObject;
                                        if(scriptableObjectReference != null)
                                            if(!assetObjectSet.Contains(scriptableObjectReference))
                                            {
                                                if(AssetDatabase.Contains(scriptableObjectReference))
                                                {
                                                    assetObjectSet.Add(scriptableObjectReference);
                                                }
                                                else
                                                {
                                                    numSceneObjects++;
                                                    finalResult.Add(scriptableObjectReference);
                                                    orphanObjectSet.Remove(scriptableObjectReference);
                                                }
                                            }

                                        if(sp.objectReferenceValue != null)
                                        {
                                            var or = sp.objectReferenceValue;
                                            SerializedObject cachedSO;
                                            if(fullSerializedObjectCache.TryGetValue(or, out cachedSO))
                                            {
                                                cachedSO.Update();
                                                nextSerializedObjects.Add(cachedSO);
                                            }
                                            else
                                            {
                                                var newSO = new SerializedObject(or);
                                                nextSerializedObjects.Add(newSO);
                                                fullSerializedObjectCache[or] = newSO;
                                            }
                                        }
                                    }
                                } while (sp.Next(true));
                            }
                        }

                        var unusedList = currentSerializedObjects;// Reuse list for GC benefit
                        currentSerializedObjects = nextSerializedObjects;
                        nextSerializedObjects = unusedList;
                    }

                    sceneScriptableObjects[sceneSerializedObjects.Key] = finalResult.ToArray();
                }
            }
            finally
            {
                allSerializedObjectsEnumerator.Dispose();
            }

            if(assetObjects == null || assetObjects.Length != assetObjectSet.Count)
                assetObjects = new ScriptableObject[assetObjectSet.Count];
            if(orphanObjects == null || orphanObjects.Length != orphanObjectSet.Count)
                orphanObjects = new ScriptableObject[orphanObjectSet.Count];

            assetObjectSet.CopyTo(assetObjects);
            orphanObjectSet.CopyTo(orphanObjects);
        }

        var lineHeight = EditorGUIUtility.singleLineHeight;

        var wantedHeightReal = lineHeight * (sceneScriptableObjects.Count + numSceneObjects + 1 + orphanObjects.Length +
                                             1 + assetObjects.Length);
        scrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPosition,
            new Rect(0, 0, position.width, wantedHeightReal));

        index = 0;
        indent = 0f;

        ScriptableObjectsField(sceneScriptableObjects, false);
        ScriptableObjectsField("Unreferenced", orphanObjects, false);
        NewScriptableObjectField();
        ScriptableObjectsField("Assets", assetObjects, true);

        GUI.EndScrollView();
    }

    private void ScriptableObjectsField(Dictionary<string, ScriptableObject[]> objects, bool isAsset)
    {
        var enumerator = objects.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                var kvp = enumerator.Current;
                ScriptableObjectsField(kvp.Key, kvp.Value, isAsset);
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    private void NewScriptableObjectField()
    {
        indent = 14f;
        var lineHeight = EditorGUIUtility.singleLineHeight;
        if(GUI.Button(new Rect(indent, index++ * lineHeight, position.width, lineHeight), "+"))
            newObjectMenu.ShowAsContext();

        indent = 0f;
    }

    private void ScriptableObjectsField(string name, ScriptableObject[] objects, bool isAsset)
    {
        var lineHeight = EditorGUIUtility.singleLineHeight;
        EditorGUI.LabelField(lastRect = new Rect(indent, index++ * lineHeight, position.width, lineHeight), name,
            EditorStyles.boldLabel);
        indent = 14f;
        for (var i = 0; i < objects.Length; i++)
            ScriptableObjectField(objects[i], isAsset);
        indent = 0f;
    }

    private void ScriptableObjectField(ScriptableObject o, bool isAsset)
    {
        BeginDrag();
        var lineHeight = EditorGUIUtility.singleLineHeight;
        var bw = 64f;
        if(isAsset)
        {
            EditorGUI.ObjectField(
                lastRect = new Rect(indent, index * lineHeight, position.width - (indent + bw), lineHeight), o,
                typeof(ScriptableObject), true);
            if(GUI.Button(new Rect(position.width - bw, index++ * lineHeight, bw, lineHeight), "Delete"))
                if(EditorUtility.DisplayDialog("Delete asset?",
                    "Are you sure you want to delete the asset from disk and memory?", "Delete", "Cancel"))
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(o));
        }
        else
        {
            EditorGUI.ObjectField(
                lastRect = new Rect(indent, index * lineHeight, position.width - (indent + bw), lineHeight), o,
                typeof(ScriptableObject), true);
            if(GUI.Button(new Rect(position.width - bw, index++ * lineHeight, bw, lineHeight), "To asset"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Save to asset",
                    string.Format("{0}.asset", o.GetType().Name), "asset", "Enter a file name to save into");
                if(path.Length != 0) AssetDatabase.CreateAsset(o, path);
            }
        }

        EndDrag(o);
    }

    private void BeginDrag()
    {
        if(Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            Event.current.Use();
        }
    }

    private void EndDrag(Object o)
    {
        if(Event.current.type == EventType.MouseDrag)
            if(lastRect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new[] {o};
                DragAndDrop.StartDrag(string.Format("Dragging {0}", o));
                Event.current.Use();
            }
    }

    private Dictionary<string, SerializedObject[]> allSerializedObjects;

    private ScriptableObject[] assetObjects;
    private HashSet<ScriptableObject> assetObjectSet;
    private HashSet<Object> checkedObjects;
    private List<Component> components;
    private List<SerializedObject> currentSerializedObjects;

    [NonSerialized] private bool dirty = true;
    private List<ScriptableObject> finalResult;
    private Dictionary<Component, SerializedObject> fullSerializedComponentCache;
    private Dictionary<Object, SerializedObject> fullSerializedObjectCache;
    private float indent;

    private int index;
    private Rect lastRect;
    private List<SerializedObject> nextSerializedObjects;
    private int numSceneObjects;
    private ScriptableObject[] orphanObjects;

    // Caches
    private HashSet<ScriptableObject> orphanObjectSet;
    private Dictionary<string, ScriptableObject[]> sceneScriptableObjects;

    public Vector2 scrollPosition = Vector2.zero;
    private static GenericMenu newObjectMenu;
}