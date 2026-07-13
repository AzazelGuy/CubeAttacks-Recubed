using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (ShouldShow(property))
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return -EditorGUIUtility.standardVerticalSpacing;
    }

    private bool ShouldShow(SerializedProperty property)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        object target = property.serializedObject.targetObject;

        FieldInfo field = target.GetType().GetField(
            showIf.ConditionName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field == null)
        {
            Debug.LogWarning($"ShowIf: Campo '{showIf.ConditionName}' não encontrado.");
            return true;
        }

        if (field.FieldType != typeof(bool))
        {
            Debug.LogWarning($"ShowIf: '{showIf.ConditionName}' precisa ser um bool.");
            return true;
        }

        bool value = (bool)field.GetValue(target);

        return value == showIf.ExpectedValue;
    }
}