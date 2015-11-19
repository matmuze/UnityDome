using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CameraController : MonoBehaviour
{

    public float DefaultDistance = 5.0f;

    public float AcrBallRotationSpeed = 0.1f;
    public float FpsRotationSpeed = 0.1f;
    public float TranslationSpeed = 10.0f;
    public float ScrollingSpeed = 1.0f;
    public float PannigSpeed = 0.1f;

    public float Distance;
    public float AngleY;
    public float AngleX;

    public bool FollowTarget;
    public Vector3 TargetPosition;
    public GameObject TargetGameObject;

    /*****/

    private bool forward;
    private bool backward;
    private bool right;
    private bool left;

    /*****/

    private Camera _camera;

    void OnEnable()
    {
        _camera = Camera.main;

#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update += Update;
        }
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update = null;
        }
#endif
    }

    private float deltaTime = 0;
    private float lastUpdateTime = 0;

    void Update()
    {
        _camera = Camera.main;
        deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
        lastUpdateTime = Time.realtimeSinceStartup;

        if (FollowTarget && TargetGameObject != null)
        {
            TargetPosition = TargetGameObject.transform.position;
        }

        _camera.transform.rotation = Quaternion.Euler(AngleX, AngleY, 0.0f);
        _camera.transform.position = TargetPosition - _camera.transform.forward * Distance;

        if (forward)
        {
            TargetPosition += _camera.transform.forward * TranslationSpeed * deltaTime;
            _camera.transform.position += _camera.transform.forward * TranslationSpeed * deltaTime;
        }

        if (backward)
        {
            TargetPosition -= _camera.transform.forward * TranslationSpeed * deltaTime;
            _camera.transform.position -= _camera.transform.forward * TranslationSpeed * deltaTime;
        }

        if (right)
        {
            TargetPosition += _camera.transform.right * TranslationSpeed * deltaTime;
            _camera.transform.position += _camera.transform.right * TranslationSpeed * deltaTime;
        }

        if (left)
        {
            TargetPosition -= _camera.transform.right * TranslationSpeed * deltaTime;
            _camera.transform.position -= _camera.transform.right * TranslationSpeed * deltaTime;
        }

        _camera.transform.position = _camera.transform.position;
        _camera.transform.rotation = _camera.transform.rotation;
    }

    void DoArcBallRotation()
    {
        AngleY += Event.current.delta.x * AcrBallRotationSpeed;
        AngleX += Event.current.delta.y * AcrBallRotationSpeed;

        var rotation = Quaternion.Euler(AngleX, AngleY, 0.0f);
        var position = TargetPosition + rotation * Vector3.back * Distance;

        _camera.transform.rotation = rotation;
        _camera.transform.position = position;
    }

    void DoFpsRotation()
    {
        AngleY += Event.current.delta.x * FpsRotationSpeed;
        AngleX += Event.current.delta.y * FpsRotationSpeed;

        var rotation = Quaternion.Euler(AngleX, AngleY, 0.0f);

        _camera.transform.rotation = rotation;
        TargetPosition = _camera.transform.position + _camera.transform.forward * Distance;
    }

    void DoPanning()
    {
        TargetPosition += _camera.transform.up * Event.current.delta.y * PannigSpeed;
        _camera.transform.position += _camera.transform.up * Event.current.delta.y * PannigSpeed;

        TargetPosition -= _camera.transform.right * Event.current.delta.x * PannigSpeed;
        _camera.transform.position -= _camera.transform.right * Event.current.delta.x * PannigSpeed;
    }
    
    void DoScrolling()
    {
        Distance += Event.current.delta.y * ScrollingSpeed;
        _camera.transform.position = TargetPosition - _camera.transform.forward * Distance;

        if (Distance < 0)
        {
            TargetPosition = _camera.transform.position + _camera.transform.forward * DefaultDistance;
            Distance = Vector3.Distance(TargetPosition, _camera.transform.position);
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
        else if (Event.current.type == EventType.mouseDrag && Event.current.button == 1)
        {
            DoFpsRotation();
        }
        else if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)
        {
            DoPanning();
        }
        else if (Event.current.type == EventType.ScrollWheel)
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
            _camera.transform.position = TargetPosition - _camera.transform.forward * Distance;
        }

        if (Event.current.keyCode == KeyCode.R)
        {
            Distance = DefaultDistance;
            TargetPosition = Vector3.zero;
            _camera.transform.position = TargetPosition - _camera.transform.forward * Distance;
        }

        if (Event.current.keyCode == KeyCode.W)
        {
            forward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.S)
        {
            backward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.A)
        {
            left = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.D)
        {
            right = Event.current.type == EventType.KeyDown;
        }
    }
}
