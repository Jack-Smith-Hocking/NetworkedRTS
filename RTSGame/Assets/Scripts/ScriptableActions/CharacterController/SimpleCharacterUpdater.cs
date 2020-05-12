using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableActions.Character
{
    public class SimpleCharacterUpdater : MonoBehaviour
    {
        public CharacterManager Character = null;
        public bool AllowInputs = true;

        public void Update()
        {
            Character?.UpdateCharacter(AllowInputs);
            Character?.UpdateCamera();
        }
    }
}
