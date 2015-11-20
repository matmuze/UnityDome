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

    [Range(0.01f,1)]
    public float PositionSmoothing;

    [Range(0.01f, 1)]
    public float RotationSmoothing;
    
    public Vector3 TargetPosition;

    public bool FollowTarget;
    public GameObject TargetGameObject;

    /*****/

    private bool forward;
    private bool backward;
    private bool right;
    private bool left;

    /*****/

    void OnEnable()
    {
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
        deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
        lastUpdateTime = Time.realtimeSinceStartup;

        if (FollowTarget && TargetGameObject != null)
        {
            TargetPosition = TargetGameObject.transform.position;
        }
        
        DoFpsRotation(AngleX, AngleY);

        if (forward)
        {
            TargetPosition += transform.forward * TranslationSpeed * deltaTime;
            transform.position += transform.forward * TranslationSpeed * deltaTime;
        }

        if (backward)
        {
            TargetPosition -= transform.forward * TranslationSpeed * deltaTime;
            transform.position -= transform.forward * TranslationSpeed * deltaTime;
        }

        if (right)
        {
            TargetPosition += transform.right * TranslationSpeed * deltaTime;
            transform.position += transform.right * TranslationSpeed * deltaTime;
        }

        if (left)
        {
            TargetPosition -= transform.right * TranslationSpeed * deltaTime;
            transform.position -= transform.right * TranslationSpeed * deltaTime;
        }

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position, PositionSmoothing);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, transform.rotation, RotationSmoothing);
    }

    void DoArcBallRotation(float angleX, float angleY)
    {
        transform.rotation = Quaternion.Euler(angleX, angleY, 0.0f);
        transform.position = TargetPosition + transform.rotation * Vector3.back * Distance;
    }

    void DoFpsRotation(float angleX, float angleY)
    {
        transform.position = TargetPosition - transform.forward * Distance;
        transform.rotation = Quaternion.Euler(angleX, angleY, 0.0f);
        TargetPosition = transform.position + transform.forward * Distance;
    }

    void DoPanning(float DeltaX, float DeltaY)
    {
        TargetPosition += transform.up * DeltaY * PannigSpeed;
        transform.position += transform.up * DeltaY * PannigSpeed;

        TargetPosition -= transform.right * DeltaX * PannigSpeed;
        transform.position -= transform.right * DeltaX * PannigSpeed;
    }
    
    void DoScrolling(float DeltaY)
    {
        Distance += Event.current.delta.y * ScrollingSpeed;
        transform.position = TargetPosition - transform.forward * Distance;

        if (Distance < 0)
        {
            TargetPosition = transform.position + transform.forward * DefaultDistance;
            Distance = Vector3.Distance(TargetPosition, transform.position);
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
            AngleY += Event.current.delta.x * AcrBallRotationSpeed;
            AngleX += Event.current.delta.y * AcrBallRotationSpeed;

            DoArcBallRotation(AngleX, AngleY);
        }
        else if (Event.current.type == EventType.mouseDrag && Event.current.button == 1)
        {
            AngleY += Event.current.delta.x * FpsRotationSpeed;
            AngleX += Event.current.delta.y * FpsRotationSpeed;
        }
        else if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)
        {
            DoPanning(Event.current.delta.x * PannigSpeed, Event.current.delta.y * PannigSpeed);
        }
        else if (Event.current.type == EventType.ScrollWheel)
        {
            DoScrolling(Event.current.delta.y * ScrollingSpeed);
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
