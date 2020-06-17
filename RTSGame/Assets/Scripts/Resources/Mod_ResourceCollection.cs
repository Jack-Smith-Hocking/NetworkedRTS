using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace RTS_System.Resource
{
    public class Mod_ResourceCollection : NetworkBehaviour
    {
        [Tooltip("The DisplayCounterUI to display the amount of resources")] public DisplayCounterUI ResourceDisplay = null;
        [Space]
        [Tooltip("Data to set use to set up collection")] public Mod_ResourceValue ResourceData;
        [Space]
        [Tooltip("Amount of resources to replenish ever #seconds")] public int ResourcesReplenished = 1;
        [Min(0.1f)] [Tooltip("The amount of time before resources will be replenished again")] public float ReplenishTime = 0.1f;
        [Space]
        [Tooltip("Event to be called when resources are changed")] public UnityEvent OnResourceChangeEvent;
        
        public bool IsEmpty { get { return ResourceData.RawValue == 0; } }

        private Mod_ResourceValue CollectedResource;
        private int MaxResources = 100;

        private void Awake()
        {
            MaxResources = ResourceData.RawValue;
            ResourceData.RawValue = Mathf.Clamp(ResourceData.RawValue, 0, MaxResources);
            
            if (ResourcesReplenished != 0)
            {
                InvokeRepeating("ServReplenishResources", 0, ReplenishTime);
            }
        }

        /// <summary>
        /// Will only happen on server, replenish the ResourcePile by some amount then update all clients
        /// </summary>
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

        /// <summary>
        /// Tell each client to set their current resource amount
        /// </summary>
        /// <param name="resourceCount">Amount to set to</param>
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

        /// <summary>
        /// Take resources away from the ResourcePile
        /// </summary>
        /// <param name="collectedAmount">Resources to take away</param>
        /// <returns>Return a data structure containing the type and amount of resources colelcted</returns>
        public Mod_ResourceValue CollectResources(int collectedAmount)
        {
            CollectedResource.ResourceType = ResourceData.ResourceType;

            // If the ResourcePile has enough resources, collect them
            if (ResourceData.RawValue >= collectedAmount)
            {
                CollectedResource.RawValue = collectedAmount;
                ResourceData.RawValue -= collectedAmount;

                RpcUpdateResourcePile(ResourceData.RawValue);
            }
            // Else if attempting collect more resources than the ResourcePile has left
            // Then collect the remaining amount
            else if (ResourceData.RawValue > 0)
            {
                CollectedResource.RawValue = ResourceData.RawValue;
                RpcUpdateResourcePile(ResourceData.RawValue);
            }
            else
            {
                CollectedResource.RawValue = 0;
            }

            // Return the amount collected and the type of the resource
            return CollectedResource;
        }
    }
}