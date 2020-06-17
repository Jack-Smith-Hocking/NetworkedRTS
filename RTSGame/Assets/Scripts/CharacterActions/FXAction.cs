using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableActions
{
    [CreateAssetMenu(fileName = "New JumpAction", menuName = "ScriptableObject/Actions/FX")]
    public class FXAction : ScriptableActions.Action
    {
        [Tooltip("The prefab that will act as an FX container")] public GameObject FXPrefab = null;
        [Tooltip("An offset to spawn the prefab at")] public Vector3 FXOffset = Vector3.zero;
        [Tooltip("A scale for the prefab when spawned")] public float FXScale = 1;

        [HideInInspector] public float Duration = 0;

        GameObject fxInst = null;
        ParticleSystem particleInst = null;
        AudioSource audioInst = null;
        Renderer rendererInst = null;

        List<Material> materials = new List<Material>(5);

        float startVolume = 0;
        float endTime = 0;

        public override void Perform(Transform parent)
        {
            if (FXPrefab && parent)
            {
                // Create an instance of the FXPrefab
                // Manipulate it appropriately 
                fxInst = Instantiate(FXPrefab);
                fxInst.transform.position = parent.position + FXOffset;
                fxInst.transform.parent = parent;

                fxInst.transform.localScale *= FXScale;

                // Get necessary components 
                rendererInst = fxInst.GetComponent<Renderer>();
                audioInst = fxInst.GetComponent<AudioSource>();
                particleInst = fxInst.GetComponent<ParticleSystem>();
            }

            if (particleInst)
            {
                ParticleSystem.MainModule main = particleInst.main;
            }
            if (audioInst)
            {
                startVolume = audioInst.volume;
            }

            endTime = Time.time + Duration;
        }

        public override bool ActionUpdate(Transform trans)
        {
            // Slowly decrease the volume of the audio and stop emitting particles at a point
            if (Duration > 0)
            {
                if (Time.time >= endTime)
                {
                    Destroy(fxInst);
                    return true;
                }
            }
            else
            {
                float timeRemaining = endTime - Time.time;
                if (timeRemaining < 1)
                {
                    if (particleInst)
                    {
                        particleInst.Stop();
                    }

                    if (audioInst)
                    {
                        audioInst.volume =  startVolume * timeRemaining;
                    }
                }

                return false;
            }

            return false;
        }

        public override void Cancel()
        {
            endTime = Time.time + 1.0f;
        }
        public void DeleteFX()
        {
            if (fxInst)
            {
                Destroy(fxInst);
            }
        }

        public void ShrinkFX(float scale, float time)
        {
            if (fxInst)
            {
                Tween.LocalScale(fxInst.transform, fxInst.transform.localScale * scale, time, 0);
            }
        }
    }
}

