using UnityEngine;
using UnityEngine.Animations.Rigging;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerRigController : MonoBehaviour
{
    [Header("Player")]
    internal IPlayerMotor  _playerMotor;
    IGrapplingHook _grapplingHook;

    [Header("Rigs")]
    [SerializeField] Rig _headRig;
    [SerializeField] Rig _rightArmRig;

    [Header("Body Parts")]
    [SerializeField] Transform Head;
    [SerializeField] Transform HeadTarget;
    [SerializeField] internal Transform R_Shoulder;
    [SerializeField] Transform R_Hand;
    [SerializeField] Transform R_Target;
    [SerializeField] internal Vector2 R_RangeOfMotion = new Vector2(-45f, 45f);
    internal float R_TotalRange { get => (R_RangeOfMotion.y - R_RangeOfMotion.x); }

    [Header("Settings")]
    [SerializeField] float _bodyRotationSpeed = 8f;
    [SerializeField] float _rightArmRotationSpeed = 8f;

    float r_armLength = 0f;
    Vector3 r_targetDefaultLocalPos;
    Vector3 _firedDir;

    void Awake()
    {
        _playerMotor = GetComponent<IPlayerMotor>();
        _grapplingHook = GetComponentInChildren<IGrapplingHook>();
    }

    void Start()
    {
        _headRig.weight = 1f;
        _rightArmRig.weight = 0f;

        r_armLength = Vector3.Distance(R_Shoulder.position, R_Hand.position);
        r_targetDefaultLocalPos = R_Target.localPosition;
    }

    void OnValidate()
    {
        R_RangeOfMotion.x = Mathf.Min(Mathf.Max(R_RangeOfMotion.x, -180f), 0f);
        R_RangeOfMotion.y = Mathf.Max(Mathf.Min(R_RangeOfMotion.y, 180f), 0f);
    }

    void OnEnable()
    {
        _grapplingHook.IdleEvent.AddListener(RightArmIdle);
        _grapplingHook.FiredEvent.AddListener(RightArmFired);
        //_grapplingHook.GrappledEvent.AddListener(RightArmGrappled);
    }

    void OnDisable()
    {
        _grapplingHook.IdleEvent.RemoveListener(RightArmIdle);
        _grapplingHook.FiredEvent.RemoveListener(RightArmFired);
        //_grapplingHook.GrappledEvent.RemoveListener(RightArmGrappled);
    }

    void Update()
    {
        HeadRig();
        if (_grapplingHook.IsFired)
            RotateBody();
        else if (_grapplingHook.IsGrappled)
            RightArmGrappled();
    }

    void HeadRig()
    {
        // Position target 5m infront of head
        HeadTarget.position = Head.position + (Head.position - _playerMotor.Viewpoint.position);

        // Adjust head rig weight based on dot product
        _headRig.weight = Mathf.Min(Vector3.Dot(_playerMotor.Viewpoint.forward, _playerMotor.Orientation.forward) + 1f, 1f);
    }

    public void RightArmIdle()
    {
        _rightArmRig.weight = 0f;
        R_Target.localPosition = r_targetDefaultLocalPos;
    }

    public void RightArmFired()
    {
        _rightArmRig.weight = 1f;
        _firedDir = _playerMotor.Viewpoint.forward;
    }

    public void RightArmGrappled() 
    {
        AimRightArm(_grapplingHook.GrapplePoint - R_Shoulder.position);
    }

    void AimRightArm(Vector3 direction)
    {
        if (direction.magnitude != 1f)
            direction.Normalize();

        Vector3 directionPlane = Vector3.ProjectOnPlane(direction, _playerMotor.Orientation.up);
        float angleDir = AngleDir(_playerMotor.Orientation.forward, directionPlane, _playerMotor.Orientation.up);
        float angle = Vector3.Angle(directionPlane, _playerMotor.Orientation.forward);

        // OUT OF RANGE (LEFT)
        if (angleDir == -1 && -angle < R_RangeOfMotion.x)
        {
            float deltaAngle = angle + R_RangeOfMotion.x;
            direction = Quaternion.AngleAxis(deltaAngle, _playerMotor.Orientation.up) * direction;
        }
        // OUT OF RANGE (RIGHT)
        else if (angleDir == 1 && angle > R_RangeOfMotion.y)
        {
            float deltaAngle = R_RangeOfMotion.y - angle;
            direction = Quaternion.AngleAxis(deltaAngle, _playerMotor.Orientation.up) * direction;
        }

        // Position
        Vector3 targetPosition = R_Shoulder.position + (direction * r_armLength);
        R_Target.position = Vector3.MoveTowards(R_Target.position, targetPosition, _rightArmRotationSpeed * Time.deltaTime);
        
        // Rotation (varies with each model)
        R_Target.right = direction;
    }

    void RotateBody()
    {
        // Rotate player
        if (!_playerMotor.IsMovementPressed) 
        {
            Quaternion targetRotation = transform.rotation * Quaternion.LookRotation(Vector3.ProjectOnPlane(_firedDir, _playerMotor.Orientation.up), _playerMotor.Orientation.up);
            _playerMotor.Orientation.rotation = Quaternion.RotateTowards(_playerMotor.Orientation.rotation, targetRotation, _bodyRotationSpeed * 100f * Time.deltaTime);
        }

        // Adjust arm while rotating
        AimRightArm(_firedDir);
    }

    float AngleDir(Vector3 forward, Vector3 targetDir, Vector3 up) {
		Vector3 perp = Vector3.Cross(forward, targetDir);
		float direction = Vector3.Dot(perp, up);
		
		if (direction > 0f)
			return 1f;
		else if (direction < 0f)
			return -1f;
		else
			return 0f;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerRigController))]
public class PlayerRigControllerEditor : Editor
{
    Color discColor = new Color(1f, 0f, 0f, 0.5f);
    Color lineColor = new Color(0f, 0f, 1f, 0.5f);
    Color leftHandlerColor = new Color(0f, 0f, 1f, 1f);
    Color rightHandlerColor = new Color(1f, 0f, 0f, 1f);
    Color selectedHandlerColor = new Color(0f, 1f, 0f, 1f);

    float discRadius = 1f;
    int hoverIndex = -1, nearestHandle = -1;

    public void OnSceneGUI()
    {
        PlayerRigController PRC = target as PlayerRigController;
        IPlayerMotor playerMotor = PRC.GetComponent<IPlayerMotor>();

        // Disc
        Handles.color = discColor;
        Vector3 from = Quaternion.AngleAxis(PRC.R_RangeOfMotion.x, playerMotor.Orientation.up) * playerMotor.Orientation.forward;
        Handles.DrawSolidArc(PRC.R_Shoulder.position, playerMotor.Orientation.up, from, PRC.R_TotalRange, 1f);

        // Forward Line
        Handles.color = lineColor;
        Handles.DrawDottedLine(PRC.R_Shoulder.position, PRC.R_Shoulder.position + (playerMotor.Orientation.forward * discRadius), 8f);

        UnityEngine.Event currentEvent = UnityEngine.Event.current;

        // Repaint
        if (currentEvent.type == EventType.Repaint)
        {
            hoverIndex = HandleUtility.nearestControl;

            Vector3 leftHandleDir = Quaternion.AngleAxis(PRC.R_RangeOfMotion.x, playerMotor.Orientation.up) * playerMotor.Orientation.forward;
            Vector3 rightHandleDir = Quaternion.AngleAxis(PRC.R_RangeOfMotion.y, playerMotor.Orientation.up) * playerMotor.Orientation.forward;

            Handles.color = hoverIndex == 11 ? selectedHandlerColor : leftHandlerColor;
            Handles.ArrowHandleCap(
                11,
                PRC.R_Shoulder.position + leftHandleDir * discRadius,
                Quaternion.LookRotation(leftHandleDir),
                0.1f,
                EventType.Repaint
            );
            Handles.color = hoverIndex == 12 ? selectedHandlerColor : rightHandlerColor;
            Handles.ArrowHandleCap(
                12,
                PRC.R_Shoulder.position + rightHandleDir * discRadius,
                Quaternion.LookRotation(rightHandleDir),
                0.1f,
                EventType.Repaint
            );
        }
        // Layout
        else if (currentEvent.type == EventType.Layout)
        {
            Vector3 leftHandleDir = Quaternion.AngleAxis(PRC.R_RangeOfMotion.x, playerMotor.Orientation.up) * playerMotor.Orientation.forward;
            Vector3 rightHandleDir = Quaternion.AngleAxis(PRC.R_RangeOfMotion.y, playerMotor.Orientation.up) * playerMotor.Orientation.forward;

            Handles.ArrowHandleCap(
                11,
                PRC.R_Shoulder.position + leftHandleDir * discRadius,
                Quaternion.LookRotation(leftHandleDir),
                0.1f,
                EventType.Layout
            );
            Handles.ArrowHandleCap(
                12,
                PRC.R_Shoulder.position + rightHandleDir * discRadius,
                Quaternion.LookRotation(rightHandleDir),
                0.1f,
                EventType.Layout
            );
        }
        // MouseDown
        else if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            nearestHandle = HandleUtility.nearestControl;
        }
        // MouseUp
        else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
        {
            nearestHandle = -1;
        }
        // MouseDrag
        else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0)
        {
            // Left
            if (nearestHandle == 11)
            {
                Vector3 closest = HandleUtility.ClosestPointToDisc(PRC.R_Shoulder.position, playerMotor.Orientation.up, 1f);
                Vector3 dirToClosest = (closest - PRC.R_Shoulder.position).normalized;
                PRC.R_RangeOfMotion.x = -Vector3.Angle(playerMotor.Orientation.forward, dirToClosest);
                EditorUtility.SetDirty(PRC);
                SceneView.currentDrawingSceneView.Repaint();
              }
            // Right
            else if (nearestHandle == 12)
            {
                Vector3 closest = HandleUtility.ClosestPointToDisc(PRC.R_Shoulder.position, playerMotor.Orientation.up, 1f);
                Vector3 dirToClosest = (closest - PRC.R_Shoulder.position).normalized;
                PRC.R_RangeOfMotion.y = Vector3.Angle(playerMotor.Orientation.forward, dirToClosest);
                EditorUtility.SetDirty(PRC);
                SceneView.currentDrawingSceneView.Repaint();
            }
        }
    }
}
#endif