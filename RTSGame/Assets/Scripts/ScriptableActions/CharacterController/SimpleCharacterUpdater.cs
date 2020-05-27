using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableActions.Character
{
    public class SimpleCharacterUpdater : NetworkBehaviour
    {
        public CharacterManager Character = null;
        public bool AllowInputs = true;

        private void Start()
        {
        }

        public void Update()
        {
            if (!isLocalPlayer) return;

            Character?.UpdateCharacter(AllowInputs);
            Character?.UpdateCamera();
        }
    }
}
