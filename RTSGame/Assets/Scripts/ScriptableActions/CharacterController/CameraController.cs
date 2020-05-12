using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptableActions.Character
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("The camera to be controller by input")] public Camera CameraToAffect = null;
        [Tooltip("The game object to focus on")] public Transform ObjectToFocusOn = null;
        [Tooltip("The object to rotate (ObjectToFocusOn and this will both be rotated)")] public Transform ObjectToRotate = null;
        [Tooltip("The offset from the player the camera should use")] public Vector3 CameraOffset = Vector3.zero;
        [Tooltip("Layers to ignore when repositioning camera")] public LayerMask IgnoreLayers;
        [Space]
        [Header("Camera Distance")]
        [Tooltip("Distance away from the player that the camera should try and be")] public float StartCameraDistance = 10;
        [Tooltip("The max distance the camera can be from the subject")] public float MaxCameraDistance = 100;
        [Tooltip("The closest the camera can get to it's subject")] public float MinCameraDistance = 1;
        [Tooltip("Colliding distance offset")] public float CollidingDistanceOffsset = 0.5f;
        [Space]
        [Header("Sensitivity Settings")]
        [Tooltip("Multiplier for how quickly the player can look around (x is horizontal speed, y is vertical speed, z is zoom speed")] public Vector3 Sensitivity = Vector3.zero;
        [Tooltip("Multiplier to look sensitivity for when using a Gamepad")] public float GamepadSensitivityMultiplier = 10;
        [Tooltip("How quickly the character will face the camera's forward")] public float CharacterRotationSpeed = 10;
        [Tooltip("How quickly the camera will zoom in and out")] public float ZoomSpeed = 10;
        [Space]
        [Header("Input Details")]
        [Tooltip("The PlayerInput to get inputs from")] public PlayerInput PlayerInput = null;
        [Space]
        [Tooltip("Name of the action to control looking in the ActionMap being used")] public string LookActionName = "Look";
        [Tooltip("Name of the action to control whether looking is active (say, left mouse click)")] public string LookActiveActionName = "LeftMouseClick";
        [Tooltip("Name of the action to control whether you can rotate the player (say, left right click)")] public string RotateActionName = "RightMouseClick";
        [Tooltip("Name of the action to control zooming")] public string ZoomActionName = "Zoom";
        [Space]
        #region Restrictions
        [Header("Restrictions")]
        [Tooltip("Whether or not to rotate the character horizontally normally")] public bool RotateObjectHorizontal = false;
        [Tooltip("Whether or not to rotate the character vertically normally")] public bool RotateObjectVertical = true;
        [Tooltip("Stop the player looking vertically")] public bool RestrictXLook = false;
        [Tooltip("Stop the player looking horizontally")] public bool RestrictYLook = false;
        [Tooltip("Stop the player from zooming")] public bool RestrictZoom = false;
        [Space]
        [Tooltip("Max vertical lenience when looking")] public float MaxVerticalLookAngle = 90;
        [Tooltip("Min vertical lenience when looking")] public float MinVerticalLookAngle = -90;
        #endregion
        [Space]
        [Header("Invert Settings")]
        [Tooltip("Inverts the horizontal rotation of the camera")] public bool InvertCameraX = false;
        [Tooltip("Inverts the vertical rotation of the camera")] public bool InvertCameraY = false;
        [Tooltip("Inverts the zoom direction")] public bool InvertZoom = false;

        private Transform cameraTransform = null;

        private Vector2 lookDirection = Vector2.zero;
        private Vector2 lookData = Vector2.zero;

        private Vector3 targetVector = Vector3.zero;
        private Vector3 targetPosition = Vector3.zero;
        private Vector3 objectLookDir = Vector3.zero;
        private Vector3 offseetPosition { get { return (ObjectToFocusOn.position + CameraOffset); } }

        private LayerMask raycastLayers;

        private Quaternion objectTargetDir;

        private float currentCameraDistance = 0;
        private float targetCameraDistance = 0;

        private bool isGamepad = false;
        private bool rotateCharacter = true;
        private bool updateDistance = true;

        private BoundInput lookInput = new BoundInput();
        private BoundInput ifActiveInput = new BoundInput();
        private BoundInput rotatePlayerInput = new BoundInput();
        private BoundInput zoomPlayerInput = new BoundInput();

        // Start is called before the first frame update
        public void Start()
        {
            currentCameraDistance = Mathf.Clamp(StartCameraDistance, MinCameraDistance, MaxCameraDistance);
            targetCameraDistance = currentCameraDistance;

            if (CameraToAffect == null)
            {
                CameraToAffect = Camera.main;
                Debug.LogWarning($"No camera was set on the CameraController on '{gameObject.name}' GameObject, set to use the main camera");
            }
            if (ObjectToFocusOn == null)
            {
                ObjectToFocusOn = transform;
                Debug.LogWarning($"No transform was set to orbit on the '{gameObject.name}' GameObject");
            }

            if (CameraToAffect != null)
            {
                cameraTransform = CameraToAffect.transform;
            }

            // Binds the look input, determines the method in which the camera is rotated (eg. mouse or joystick)
            lookInput.Bind(PlayerInput, LookActionName, true);
            // Binds the lookActive input, which will determine whether the camera will be rotated
            ifActiveInput.Bind(PlayerInput, LookActiveActionName, true);

            // Add a callback for the zoom input that will slowly increase/decrease the currentCameraDistance
            zoomPlayerInput.PerformedActions += (InputAction.CallbackContext cc) =>
            {
                if (!RestrictZoom)
                {
                    if (!updateDistance) targetCameraDistance = currentCameraDistance;

                    targetCameraDistance += zoomPlayerInput.CurrentVec2Val.y * Sensitivity.z * Time.deltaTime * (InvertZoom ? 1 : -1);
                    targetCameraDistance = Mathf.Clamp(targetCameraDistance, MinCameraDistance, MaxCameraDistance);
                }
            };
            zoomPlayerInput.Bind(PlayerInput, ZoomActionName, true);

            // Binds the input that toggles whether the focus object will be rotated or not
            rotatePlayerInput.PerformedActions += (InputAction.CallbackContext cc) => { rotateCharacter = !rotateCharacter; };
            rotatePlayerInput.Bind(PlayerInput, RotateActionName, true);

            raycastLayers = ~IgnoreLayers;
        }

        public void UpdateCamera()
        {
            isGamepad = PlayerInput.currentControlScheme == "Gamepad";
            
            // Rotate the ObjectToFocusOn and ObjectToRotate GameObjects
            if (rotateCharacter)
            {
                RotatePlayer(ObjectToFocusOn);
                RotatePlayer(ObjectToRotate);
            }

            // Get the look delta when needed
            if (LookActiveActionName.Length == 0 || ifActiveInput.CurrentBoolVal)
            {
                GetLookData(lookInput.CurrentVec2Val);
            }

            // Check if the camera is colliding with anything and if it needs to be repositioned 
            CheckLineOfSight();
            CalcCameraPosition(Vector3.forward);
        }

        /// <summary>
        /// Get the look delta and manipulate it appropriately to move the camera by an amount
        /// </summary>
        /// <param name="lookData"></param>
        void GetLookData(Vector3 lookData)
        {
            lookDirection.y += (RestrictXLook ? 0 : lookData.x * Sensitivity.x * Time.fixedDeltaTime) * (isGamepad ? GamepadSensitivityMultiplier : 1) * (InvertCameraX ? -1 : 1);

            lookDirection.x -= (RestrictYLook ? 0 : lookData.y * Sensitivity.y * Time.fixedDeltaTime) * (isGamepad ? GamepadSensitivityMultiplier : 1) * (InvertCameraY ? -1 : 1);
            lookDirection.x = Mathf.Clamp(lookDirection.x, MinVerticalLookAngle, MaxVerticalLookAngle);
        }

        /// <summary>
        /// Calculate the position the camera wants to try and move towards
        /// </summary>
        /// <param name="forward"></param>
        void CalcCameraPosition(Vector3 forward)
        {
            if (updateDistance)
            {
                currentCameraDistance = Mathf.MoveTowards(currentCameraDistance, targetCameraDistance, ZoomSpeed * Time.deltaTime);
            }

            targetVector = Quaternion.Euler(lookDirection.x, lookDirection.y, 0) * -forward;
            targetVector = targetVector.normalized * currentCameraDistance;

            targetPosition = ObjectToFocusOn.position + targetVector;

            cameraTransform.position = targetPosition;

            cameraTransform.LookAt(offseetPosition);
        }

        /// <summary>
        /// Rotate a transform to be facing relative to the camera 
        /// </summary>
        /// <param name="rotateTransform">Transform to be rotated</param>
        void RotatePlayer(Transform rotateTransform)
        {
            if (!rotateTransform) return;

            // What we do here is, get the amount the camera has been rotated
            // Manipulate the horizontal and vertical rotation depending on available options
            // Then we lerp towards this rotation on the Transform

            objectLookDir = new Vector3(lookDirection.x, lookDirection.y, 0);

            if (!RotateObjectVertical)
            {
                objectLookDir.x = 0;
            }
            if (!RotateObjectHorizontal)
            {
                objectLookDir.y = rotateTransform.rotation.eulerAngles.y;
            }

            objectTargetDir = Quaternion.Euler(objectLookDir.x, objectLookDir.y, 0);
            rotateTransform.rotation = Quaternion.Lerp(rotateTransform.rotation, objectTargetDir, CharacterRotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Determines whether the CameraController is colliding with anything
        /// </summary>
        /// <returns></returns>
        bool CheckLineOfSight()
        {
            RaycastHit hit;
            Ray ray = new Ray(offseetPosition, -cameraTransform.forward);

            bool hasCollided = Physics.Raycast(ray, out hit, MaxCameraDistance, raycastLayers, QueryTriggerInteraction.Ignore);

            if (hasCollided)
            {
                Vector3 pos = hit.point;

                // Calculates distance to nearest collision
                float dis = Vector3.Distance(ObjectToFocusOn.transform.position, pos);

                // If the distance is less than or equal to the currentCameraDistance (plus some small buffer) we want to move the camera
                if (dis <= currentCameraDistance + 1)
                {
                    // Move the camera to be some # units away from the point that it collided with
                    pos = hit.point - (ray.direction * CollidingDistanceOffsset);
                    dis = Vector3.Distance(ObjectToFocusOn.transform.position, pos);

                    // If the current targetCameraDistance is greater than the distance from the collision
                    // We want to set the targetCameraDistance to the distance from the collision so that it won't rubber band back into the collision
                    if (targetCameraDistance > dis)
                    {
                        currentCameraDistance = dis;
                        updateDistance = false;
                    }
                }
                else
                {
                    hasCollided = false;
                }
 
            }

            updateDistance = !hasCollided;
            if (targetCameraDistance < currentCameraDistance) updateDistance = true;

            return hasCollided;
        }

        public void SetSensitivityX(float x)
        {
            Sensitivity.x = x;
        }
        public void SetSensitivityY(float y)
        {
            Sensitivity.y = y;
        }

        /// <summary>
        /// Flips the x or y looking axis
        /// </summary>
        /// <param name="xy">False will flip x, true will flip y</param>
        public void FlipInvert(bool xy)
        {
            InvertCameraX = xy ? InvertCameraX : !InvertCameraX;
            InvertCameraY = xy ? !InvertCameraY : InvertCameraY;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(targetPosition, 1);
        }
    }
}

