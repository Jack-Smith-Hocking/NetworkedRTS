using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RTS_System.Resource
{
    public class Mod_ResourceCollection : NetworkBehaviour
    {
        public DisplayCounterUI ResourceDisplay = null;
        [Space]
        public UnityEvent OnResourceChangeEvent;
        [Space]
        public Mod_ResourceValue ResourceData;
        [Space]
        public int ResourcesReplenished = 1;
        [Min(0.1f)] public float ReplenishTime = 0.1f;
        [Space]
        public Mod_ResourceValue CollectedResource;
        public int MaxResources = 100;

        public bool IsEmpty { get { return ResourceData.RawValue == 0; } }

        private void Awake()
        {
            MaxResources = ResourceData.RawValue;
            ResourceData.RawValue = Mathf.Clamp(ResourceData.RawValue, 0, MaxResources);
            
            if (ResourcesReplenished != 0)
            {
                InvokeRepeating("ServReplenishResources", 0, ReplenishTime);
            }
        }

        [ServerCallback]
        void ServReplenishResources()
        {
            if (ResourceData.RawValue < MaxResources && ResourceData.RawValue > 0)
            {
                ResourceData.RawValue += ResourcesReplenished;
                ResourceData.RawValue = Mathf.Clamp(ResourceData.RawValue, 0, MaxResources);

                RpcUpdateResourcePile(ResourceData.RawValue);
            }
        }

        [ClientRpc]
        public void RpcUpdateResourcePile(int resourceCount)
        {
            ResourceData.RawValue = resourceCount;

            if (ResourceDisplay)
            {
                ResourceDisplay.UpdateDisplay(resourceCount, MaxResources);
            }

            OnResourceChangeEvent.Invoke();
        }

        public Mod_ResourceValue CollectResources(int collectedAmount)
        {
            CollectedResource.ResourceType = ResourceData.ResourceType;

            if (ResourceData.RawValue >= collectedAmount)
            {
                CollectedResource.RawValue = collectedAmount;
                ResourceData.RawValue -= collectedAmount;

                RpcUpdateResourcePile(ResourceData.RawValue);
            }
            else if (ResourceData.RawValue > 0)
            {
                CollectedResource.RawValue = ResourceData.RawValue;
                RpcUpdateResourcePile(ResourceData.RawValue);
            }
            else
            {
                CollectedResource.RawValue = 0;
            }

            return CollectedResource;
        }
    }
}