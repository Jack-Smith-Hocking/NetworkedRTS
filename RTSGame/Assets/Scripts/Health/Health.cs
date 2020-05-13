using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using Pixelplacement;

public class Health : MonoBehaviour
{
    public enum DamageState
    {
        TAKE_DAMAGE, NO_DAMAGE, NO_HEAL
    }

    [Tooltip("Maximum health to have")] public float MaxHealth = 10;
    [Space]
    [Tooltip("Events to happen when hit, will receive the damage taken")] public UnityEvent OnTakeDamageEvent = null;
    [Tooltip("Event to be called when health is below or equal to zero")] public UnityEvent OnDeathEvent = null;

    [Space]

    [Header("Death Details")]
    [Tooltip("FX to be played when dead")] public ScriptableActions.FXAction DeathFX = null;
    [Tooltip("The position to play the DeathFX from")] public Transform DeathFXPosition = null;
    [Tooltip("The time it will take to scale")] public float DeathScaleTime = 1;
    [Tooltip("End scale to shrink to when dead")] public float DeathEndScale = 0.1f;

    [Header("Hit Details")]
    [Tooltip("FX to be played when hit")] public ScriptableActions.FXAction HitFX = null;
    [Tooltip("HitFX position")] public Transform HitFXPosition = null;
    [Tooltip("Whether to use the hit position or the hit point for HitFX")] public bool UseHitFXPoint = true;

    [Space]

    public DamageState HealthState = DamageState.TAKE_DAMAGE;

    public bool HasDied { get { return CurrentHealth <= 0; } }

    public float CurrentHealth { get; protected set; } = 0;
    /// <summary>
    /// (CurrentHealth/MaxHealth)
    /// </summary>
    public float HealthRatio { get { return (CurrentHealth / MaxHealth); } }

    /// <summary>
    /// Callbacks for getting hit
    /// </summary>
    public Action<float, Vector3, GameObject> OnTakeDamage = null;
    /// <summary>
    /// Callbacks for getting hit
    /// </summary>
    public Action<float> OnTakeDamageSimple = null;

    private GameObject currentHitPosition = null;

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;

        if (DeathFX) DeathFX = Instantiate(DeathFX);
        if (HitFX) HitFX = Instantiate(HitFX);
    }

    /// <summary>
    /// A simple 'TakeDamage' only takes a float damage input, will invoke OnTakeDamageSimple callback
    /// </summary>
    /// <param name="damage">Amount of damage to take, below zero will heal</param>
    public void TakeDamage(float damage)
    {
        // Check if damage/healing can be done
        if (HealthState == DamageState.NO_DAMAGE && damage > 0) return;
        if (HealthState == DamageState.NO_HEAL && damage < 0) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        OnTakeDamageSimple?.Invoke(damage);
        OnTakeDamageEvent?.Invoke();
    }
    /// <summary>
    /// A more complex 'TakeDamage' which will take the collision point and the object that dealt the damage, will invoke OnTakeDamage callback
    /// </summary>
    /// <param name="damage">Amount of damage to take, below zero will heal</param>
    /// <param name="collisionPoint">The point on the object that was hit when taking damage</param>
    /// <param name="damageDealer">The GameObject that dealt the damage</param>
    public void TakeDamage(float damage, Vector3 collisionPoint, GameObject damageDealer)
    {
        // Check if damage/healing can be done
        if (HealthState == DamageState.NO_DAMAGE && damage > 0) return;
        if (HealthState == DamageState.NO_HEAL && damage < 0) return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        OnTakeDamageSimple?.Invoke(damage);
        OnTakeDamage?.Invoke(damage, collisionPoint, damageDealer);
        OnTakeDamageEvent?.Invoke();

        // Apply HitFX at the point of the hit if there are any
        if (HitFX)
        {
            if (UseHitFXPoint)
            {
                if (currentHitPosition)
                {
                    Destroy(currentHitPosition);
                }

                currentHitPosition = new GameObject();
                currentHitPosition.transform.position = collisionPoint;
                currentHitPosition.transform.parent = transform;

                HitFX.Duration = 0.5f;
                HitFX.Perform(currentHitPosition.transform);
            }
            else
            {
                HitFX.DeleteFX();

                HitFX.Duration = 0.5f;
                HitFX.Perform((HitFXPosition ? HitFXPosition : transform));
            }
        }
    }

    public void Update()
    {
        // If has died, invoke OnDeathEvent and disable this script from updating
        if (HasDied)
        {
            OnDeathEvent?.Invoke();
            this.enabled = false;
        }

        // Update the HitFX if there is one
        if (HitFX)
        {
            HitFX.ActionUpdate(null);
        }    
    }

    public void DestroyHealthObject()
    {
        Destroy(gameObject);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public void Die()
    {
        // Scale the GameObject down to desired size and then destroy it
        Tween.LocalScale(transform, transform.localScale * DeathEndScale, DeathScaleTime, 0);

        if (DeathFX)
        {
            DeathFX.Perform((DeathFXPosition ? DeathFXPosition : transform));
        }

        Invoke("DestroyHealthObject", DeathScaleTime + 0.1f);
    }
}
