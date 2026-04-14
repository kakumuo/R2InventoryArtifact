using UnityEngine;

namespace R2InventoryArtifact.Hooks
{
    /// <summary>
    /// Alternate InventoryHook. 
    /// Implementation requires pulling full set of inventory and comparing it to updated set of inventory after a change. 
    /// NOTE:
    /// - would need to implement check to ensure there is no recursion
    /// - would need to ignore all changes not related to items or equipment
    /// </summary>
    public class InventoryHookB : MonoBehaviour
    {

    }
}