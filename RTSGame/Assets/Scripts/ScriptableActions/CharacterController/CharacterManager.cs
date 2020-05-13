using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace ScriptableActions.Character
{
    public class CharacterManager : MonoBehaviour
    {
        [Tooltip("Character name used for switching characters")] public string CharacterName;
        [Space]
        [Tooltip("The PlayerInput that will be used to get inputs from")] public PlayerInput PlayerInput = null;
        public Animator Animator = null;
        [Tooltip("Controls camera interaction")] public CameraController CameraController = null;
        [Tooltip("The CharacterController that this CharacterManager will affect")] public CharacterController CharacterController = null;
        [Tooltip("The manager for positions (such as shoot position, VFX position etc)")] public ObjectManager PositionManager = null;

        [Space]
        [Tooltip("Setting this to off will mean the character isn't pulled down by gravity")] public bool UseGravity = true;
        [Tooltip("Multiplies the Physics.Gravity value for gravity calculations")] public float GravityMultiplier = 1;
        [Tooltip("How quickly the player will lose horizontal motion while in the air")] public float AirResistance = 0;
        [Space]

        [Header("Actions")]
        [Tooltip("Whether or not the character will be able to move in the vertical plane (excluding gravity)")] public bool TravelVertically = false;
        [Tooltip("Controls walking")] public WalkAction WalkAction = null;
        [Tooltip("Controls running")] public WalkAction RunAction = null;
        [Tooltip("Controls jumping")] public JumpAction JumpAction = null;

        [Header("Restrictions")]
        [Tooltip("Whether the character can move on the x-axis (relative to camera)")] public bool MoveX = true;
        [Tooltip("Whether the character can move on the y-axis (relative to camera)")] public bool MoveY = true;
        [Tooltip("Whether the character can move on the Z-axis (relative to camera)")] public bool MoveZ = true;

        [Space]
        [Tooltip("If set to off, the character will not be able to manually move while in the air")] public bool MoveInAir = false;
        [Space]

        public List<CharacterAction> CharacterActions = new List<CharacterAction>();

        public bool IsGrounded { get; protected set; } = false;
        public Vector3 Gravity { get { return (Physics.gravity * GravityMultiplier); } }

        protected Dictionary<string, System.Action> animationEventListeners = new Dictionary<string, System.Action>();
        protected AnimatorCallback animatorCallback = null;

        protected Vector3 moveForce = Vector3.zero;
        protected Vector3 horizontalMove = Vector3.zero;

        protected bool characterActive = true;

        // Start is called before the first frame update
        void Start()
        {
            // Set up Actions
            if (WalkAction != null)
            {
                WalkAction = Instantiate(WalkAction);
                InitialiseAction(WalkAction);
            }
            if (RunAction != null)
            {
                RunAction = Instantiate(RunAction);
                InitialiseAction(RunAction);
            }
            if (JumpAction != null)
            {
                JumpAction = Instantiate(JumpAction);
                InitialiseAction(JumpAction);
            }

            InitialiseActions();

            if (Animator)
            {
                // Sets up callback detection for animator events
                animatorCallback = Animator.GetComponent<AnimatorCallback>();
                if (animatorCallback)
                {
                    animatorCallback.OnAnimationEvents += PerformEvent;
                }
            }
        }

        void InitialiseActions()
        {
            // Loop through CharacterActions and set them all up
            for (int i = 0; i < CharacterActions.Count; i++)
            {
                CharacterActions[i] = Instantiate(CharacterActions[i]);
                InitialiseAction(CharacterActions[i]);
            }
        }
        void InitialiseAction(CharacterAction action)
        {
            if (action == null) return;

            action.InitialiseAction(PlayerInput, Animator, this);

            // Add the Action to the animatorEventListeners if necessary, this will mean they will be 'pinged' when one of their 'AnimationEventNames' are passed through the event call
            if (action.AnimatorEventName.Length > 0)
            {
                if (animationEventListeners.ContainsKey(action.AnimatorEventName)) animationEventListeners[action.AnimatorEventName] += action.AnimationEventFired;
                else animationEventListeners.Add(action.AnimatorEventName, action.AnimationEventFired);
            }
        }

        /// <summary>
        /// Applies gravity to the moveForce Vector3
        /// </summary>
        /// <param name="updateMove">Whether or not this call of UpdateGravity should call the characterController.Move() function</param>
        public void UpdateGravity(bool updateMove)
        {
            if (!UseGravity) return;

            if (!IsGrounded) // Check if not on the ground
            {
                moveForce += Gravity * Time.deltaTime;
            }
            else // Reset y movement
            {
                if (JumpAction == null || !JumpAction.IsPerforming)
                {
                    moveForce.y = -0.5f;
                }
            }

            if (updateMove)
            {
                UpdateMove();
            }
        }

        /// <summary>
        /// Updates all Actions this CharacterManager handles
        /// </summary>
        /// <param name="performActions">Whether or not Actions should accept inputs</param>
        public void UpdateCharacter(bool performActions)
        {
            moveForce.x = 0;
            moveForce.z = 0;

            if (TravelVertically) moveForce.y = 0;

            performActions = performActions && characterActive;
            
            UpdateGravity(!performActions);

            bool updateWalkAnim = (WalkAction == null) ? false : (RunAction == null) ? true : (WalkAction.MoveForce.sqrMagnitude >= RunAction.MoveForce.sqrMagnitude);
   
            // Only update horizontal movement if on the ground
            if (IsGrounded && (JumpAction == null || !JumpAction.IsPerforming) || MoveInAir)
            {
                // Determine whether the WalkAction or the RunAction should be responsible for updating the Animator this frame
                if (WalkAction)
                {
                    WalkAction.AnimatorCanUpdate = updateWalkAnim && IsGrounded; 
                    WalkAction.CanPerformAction = performActions;
                }
                if (RunAction)
                {
                    RunAction.AnimatorCanUpdate = !updateWalkAnim && IsGrounded; 
                    RunAction.CanPerformAction = performActions;
                }

                WalkAction?.ActionUpdate(transform);
                RunAction?.ActionUpdate(transform);

                horizontalMove = updateWalkAnim ?
                    ((WalkAction != null) ? WalkAction.MoveForce : Vector3.zero) :
                    ((RunAction != null) ? RunAction.MoveForce : Vector3.zero);

                if (!MoveX)
                {
                    horizontalMove.x = 0;
                }
                if (!MoveZ)
                {
                    horizontalMove.z = 0;
                }

                horizontalMove = transform.TransformDirection(horizontalMove);

                if (!TravelVertically)
                {
                    horizontalMove.y = 0;
                }

            }
            else
            {
                horizontalMove = Vector3.Lerp(horizontalMove, Vector3.zero, AirResistance * Time.deltaTime);
            }

            moveForce += horizontalMove;
            
            if (JumpAction != null && MoveY)
            {
                if (JumpAction.IsPerforming)
                {
                    moveForce += JumpAction.MoveForce;
                    JumpAction.AnimatorCanUpdate = true;
                    JumpAction?.ActionUpdate(transform);
                }
            }

            // Update the Actions in CharacterActions
            Helper.LoopListForEach<CharacterAction>(CharacterActions, (CharacterAction action) => { action.AnimatorCanUpdate = true; action.CanPerformAction = performActions; action.ActionUpdate(transform); });

            UpdateMove();
        }

        void UpdateMove()
        {
            CharacterController?.Move(moveForce * Time.deltaTime);

            IsGrounded = CharacterController.isGrounded;
        }

        public void UpdateCamera()
        {
            if (CameraController)
            {
                if (!characterActive)
                {
                    CameraController.RestrictXLook = true;
                    CameraController.RestrictYLook = true;
                }
                CameraController.UpdateCamera();
            }
        }

        public void ToggleCharacter(bool toggle)
        {
            characterActive = toggle;
        }

        /// <summary>
        /// Catches AnimationEvents
        /// </summary>
        /// <param name="eventName"></param>
        protected void PerformEvent(string eventName)
        {
            if (animationEventListeners.ContainsKey(eventName)) animationEventListeners[eventName]?.Invoke();
        }

        /// <summary>
        /// Safely unbind all callbacks from inputs when the script is destroyed 
        /// </summary>
        private void OnDestroy()
        {
            if (WalkAction) WalkAction.UnbindInputs();
            if (RunAction) RunAction.UnbindInputs();
            if (JumpAction) JumpAction.UnbindInputs();

            Helper.LoopListForEach(CharacterActions, (CharacterAction action) => { action.UnbindInputs(); });
        }
    }
}

