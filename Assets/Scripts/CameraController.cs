﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class CameraController : MonoBehaviour {

    public float DefaultDistance = 5.0f;

    public float AcrBallRotationSpeed = 0.25f;
    public float FpsRotationSpeed = 0.25f;
    public float TranslationSpeed = 20.0f;
    public float ScrollingSpeed = 2.0f;
    public float PannigSpeed = 0.25f;

    public Vector3 TargetPosition;

    [HideInInspector]
    public GameObject TargetGameObject;

    /*****/

    private bool forward;
    private bool backward;
    private bool right;
    private bool left;

    [HideInInspector]
    public float Distance;

    
    public float EulerAngleX;

    
    public float EulerAngleY;

    /*****/
    void DoArcBallRotation()
    {
        EulerAngleX += Event.current.delta.x * AcrBallRotationSpeed;
        EulerAngleY += Event.current.delta.y * AcrBallRotationSpeed;

        var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);
        var position = TargetPosition + rotation * Vector3.back * Distance;

        transform.rotation = rotation;
        transform.position = position;
    }

    void DoFpsRotation()
    {
        EulerAngleX += Event.current.delta.x * FpsRotationSpeed;
        EulerAngleY += Event.current.delta.y * FpsRotationSpeed;

        var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);

        transform.rotation = rotation;
        TargetPosition = transform.position + transform.forward * Distance;
    }

    void DoPanning()
    {
        TargetPosition += transform.up * Event.current.delta.y * PannigSpeed;
        transform.position += transform.up * Event.current.delta.y * PannigSpeed;

        TargetPosition -= transform.right * Event.current.delta.x * PannigSpeed;
        transform.position -= transform.right * Event.current.delta.x * PannigSpeed;
    }


    void DoScrolling()
    {
        Distance += Event.current.delta.y * ScrollingSpeed;
        transform.position = TargetPosition - transform.forward * Distance;

        if (Distance < 0)
        {
            TargetPosition = transform.position + transform.forward * DefaultDistance;
            Distance = Vector3.Distance(TargetPosition, transform.position);
        }
    }

    private void Update()
    {
        Camera.main.transform.position = transform.position;
        Camera.main.transform.rotation = transform.rotation;

        if (Input.GetKey(KeyCode.W))
        {
            TargetPosition += gameObject.transform.forward * TranslationSpeed * Time.deltaTime;
            transform.position += gameObject.transform.forward * TranslationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            TargetPosition -= gameObject.transform.forward * TranslationSpeed * Time.deltaTime;
            transform.position -= gameObject.transform.forward * TranslationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            TargetPosition -= gameObject.transform.right * TranslationSpeed * Time.deltaTime;
            transform.position -= gameObject.transform.right * TranslationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            TargetPosition += gameObject.transform.right * TranslationSpeed * Time.deltaTime;
            transform.position += gameObject.transform.right * TranslationSpeed * Time.deltaTime;
        }
    }

    private void OnGUI()
    {
#if UNITY_EDITOR
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
        {
            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
        }
#endif

        if (Event.current.alt && Event.current.type == EventType.mouseDrag && Event.current.button == 0)
        {
            DoArcBallRotation();
        }

        if (Event.current.type == EventType.mouseDrag && Event.current.button == 1)
        {
            DoFpsRotation();
        }

        if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)
        {
            DoPanning();
        }

        if (Event.current.type == EventType.ScrollWheel)
        {
            DoScrolling();
        }

        if (Event.current.keyCode == KeyCode.F)
        {
            if (TargetGameObject != null)
            {
                TargetPosition = TargetGameObject.transform.position;
            }

            Distance = DefaultDistance;
            transform.position = TargetPosition - transform.forward * Distance;
        }

        if (Event.current.keyCode == KeyCode.R)
        {
            Distance = DefaultDistance;
            TargetPosition = Vector3.zero;
            transform.position = TargetPosition - transform.forward * Distance;
        }        
    }
}
