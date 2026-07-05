using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoolTools.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoolTools.Utilities
{
    [CreateAssetMenu(fileName = "New Object Pool Config", menuName = "Gameplay/Pool Config", order = 0)]
    public class ObjectPoolConfig : ScriptableObject
    {
        [Serializable]
        public struct PoolData
        {
            public string Key;
            public PoolableObject Prefab;
            public int Size;
        }

        public string PoolName;
        public List<PoolData> PoolDataList = new ();
        [InspectorDisabled] public ObjectPool Pool;

        private void OnValidate()
        {
            RefreshPool();
        }
        
        public void RefreshPool()
        {
            if (PoolDataList == null) return;
            
            for (int i = 0; i < PoolDataList.Count; i++)
            {
                var data = PoolDataList[i];
                if (data.Prefab == null) continue;
                
                data.Key = data.Prefab.name;

                PoolDataList[i] = data;
            }
        }
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(ObjectPoolConfig))]
    public class ObjectPoolConfigEditor : Editor
    {
        private ObjectPoolConfig _poolConfig;
        
        private void OnEnable()
        {
            _poolConfig = (ObjectPoolConfig) target;
        }
        
        public override void OnInspectorGUI()
        {
            // Check for duplicate _poolConfig.PoolDataList PoolData keys.
            // If one is found, draw a warning helpbox.
            
            if (_poolConfig.PoolDataList == null) return;
            
            var keys = new List<string>();
            var duplicateKeys = new List<string>();
            
            foreach (var poolData in _poolConfig.PoolDataList)
            {
                if (keys.Contains(poolData.Key))
                {
                    duplicateKeys.Add(poolData.Key);
                }
                else
                {
                    keys.Add(poolData.Key);
                }
            }
            
            if (duplicateKeys.Count > 0)
            {
                EditorGUILayout.HelpBox("Duplicate PoolData keys found:\n" + string.Join("\n", duplicateKeys.ToArray()), MessageType.Warning);
            }
            
            // Make a button to remove one of the duplicate entry then save the asset with the changes
            
            if (duplicateKeys.Count > 0)
            {
                if (GUILayout.Button("Remove Duplicate Keys"))
                {
                    foreach (var duplicateKey in duplicateKeys)
                    {
                        for (int i = _poolConfig.PoolDataList.Count - 1; i >= 0; i--)
                        {
                            if (_poolConfig.PoolDataList[i].Key == duplicateKey)
                            {
                                _poolConfig.PoolDataList.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    
                    EditorUtility.SetDirty(_poolConfig);
                    AssetDatabase.SaveAssets();
                }
            }

            base.OnInspectorGUI();
        }
    }
    
    // Create a custom property drawer for the PoolData struct
    [CustomPropertyDrawer(typeof(ObjectPoolConfig.PoolData))]
    public class PoolDataDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            
            var sizeProperty = property.FindPropertyRelative("Size");
            
            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            // Calculate Rects
            var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var prefabRect = new Rect(position.x, labelRect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width * 0.75f, EditorGUIUtility.singleLineHeight * 1.25f);
            // var keyRect = new Rect(prefabRect.xMax + 5, prefabRect.y, 50, EditorGUIUtility.singleLineHeight);
            var sizeRect = new Rect(prefabRect.xMax + 20f, prefabRect.y, position.width * 0.25f - 30, EditorGUIUtility.singleLineHeight * 1.25f);
            
            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(labelRect, label.text, EditorStyles.boldLabel);
            // EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("KeyFromPrefabName"), GUIContent.none);
            EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("Prefab"), GUIContent.none);
            
            var previousWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 30f;
            EditorGUI.PropertyField(sizeRect, sizeProperty, new GUIContent("Size"));
            EditorGUIUtility.labelWidth = previousWidth;
            
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // reutrn singleLineHeight * 2
            return EditorGUIUtility.singleLineHeight * 2 * 1.25f;
        }
    }
#endif
}