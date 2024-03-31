using UnityEngine;

namespace FX
{
    public class FXGameObjectsEnabled : FXBaseWithEnabled
    {
        public GameObject[] gameObjects;


        protected override void OnFXEnabled(bool state)
        {
            foreach (var obj in gameObjects) {
                obj.SetActive(state);
            }
        }

    }
}


