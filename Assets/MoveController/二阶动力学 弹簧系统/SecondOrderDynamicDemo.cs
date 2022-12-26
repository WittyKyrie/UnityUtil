using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SecondOrderDynamicDemo : MonoBehaviour
{
    public float frequency, damping, response;
    public Transform target;

    private SecondOrderDynamics _secondOrderDynamics;

    private void Start()
    {
        _secondOrderDynamics = new SecondOrderDynamics(frequency, damping, response, target.position);
    }
    
    void Update()
    {
        if (_secondOrderDynamics == null ||
            !Mathf.Approximately(_secondOrderDynamics.frequency,frequency)||
            !Mathf.Approximately(_secondOrderDynamics.damping,damping) ||
            !Mathf.Approximately(_secondOrderDynamics.response, response))
        {
            _secondOrderDynamics = new SecondOrderDynamics(frequency, damping, response, target.position);
        }
        transform.position = _secondOrderDynamics.Update(Time.deltaTime, target.position);
    }
}

[CustomEditor(typeof(SecondOrderDynamicDemo))]
public class SecondOrderDynamicDemoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var currentObject = (SecondOrderDynamicDemo)serializedObject.targetObject;
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();

        var rect = EditorGUILayout.GetControlRect(false, 128);


        const int count = 100;
        var points = new Vector3[count];
        var pointsSmooth = new Vector3[count];

        for (var i = 0; i < count; i++)
        {
            if (i < 20)
            {
                points[i] = new Vector3(i / (float)count * rect.width, 128 - 0.2f * 64, 0);
            }
            else
            {
                points[i] = new Vector3(i / (float)count * rect.width, 128 - 0.8f * 64, 0);
            }
        }

        SecondOrderDynamics secondOrderDynamics = new SecondOrderDynamics(currentObject.frequency, currentObject.damping, currentObject.response, points[0]);
        for (int i = 0; i < count; i++)
        {
            pointsSmooth[i] = secondOrderDynamics.Update(0.016f, points[i]);
        }

        GUI.BeginClip(rect);
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(
            Texture2D.whiteTexture,
            1,
            points);

        Handles.color = Color.cyan;
        Handles.DrawAAPolyLine(
            Texture2D.whiteTexture,
            1,
            pointsSmooth);
        GUI.EndClip();
    }
}

