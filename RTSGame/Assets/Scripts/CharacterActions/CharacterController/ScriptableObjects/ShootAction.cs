using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptableActions.Character
{
    [CreateAssetMenu(fileName = "New ShootAction", menuName = "ScriptableObject/Actions/Character/CombatAction/Shoot")]
    public class ShootAction : CharacterAction
    {
        [Tooltip("The prefab to be shot")] public GameObject ProjectilePrefab = null;
        [Tooltip("The name of the Object in the ObjectManager to shoot from")] public string ShootPositionName;
        [Space]
        [Tooltip("The speed of the projectile")] public float ProjectileSpeed = 1;
        [Tooltip("The damage the projectile will do")] public float ProjectileDamage = 1;
        [Tooltip("The delay between shooting again")] public float ShootDelay = 1;

        protected GameObject shootPosition = null;
        protected float delayTime = 0;

        public override void InitialiseAction(PlayerInput playerInput, Animator anim, CharacterManager manager)
        {
            primaryBoundInput.PerformedActions += (InputAction.CallbackContext cc) => { Perform(currentTransform); };
            base.InitialiseAction(playerInput, anim, manager);

            if (characterManager && characterManager.PositionManager)
            {
                shootPosition = characterManager.PositionManager.GetPosition(ShootPositionName);

                if (!shootPosition)
                {
                    DebugManager.WarningMessage($"shootPosition is null on action '{name}' on GameObject: {characterManager.name}");
                }
            }
        }

        protected override void PerformAction(Transform trans)
        {
            UpdateAnimator();
            IsPerforming = true;

            // Instantiate projectile and attempt to fire it
            if (shootPosition && EventFired)
            {
                GameObject shootObject = Instantiate(ProjectilePrefab);
                shootObject.transform.position = shootPosition.transform.position;
                shootObject.transform.rotation = shootPosition.transform.rotation;

                delayTime = Time.time + ShootDelay;
            }
        }

        protected override bool CanPerform()
        {
            return base.CanPerform() && (Time.time >= delayTime);
        }
    }
}