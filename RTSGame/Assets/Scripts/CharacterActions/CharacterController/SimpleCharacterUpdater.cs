using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableActions.Character
{
    public class SimpleCharacterUpdater : NetworkBehaviour
    {
        [Tooltip("Character to manage")] public CharacterManager Character = null;
        public bool AllowInputs = true;

        public void Update()
        {
            if (!isLocalPlayer) return;

            Character?.UpdateCharacter(AllowInputs);
            Character?.UpdateCamera();
        }
    }
}
