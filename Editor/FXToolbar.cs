using UnityEngine;
using UnityEditor;

namespace FX
{
    public class FXToolbar
    {
        public static bool ShowAddresses;

        [MenuItem("FX/Show Addresses")]
        public static void ToggleAddresses()
        {
            // Invert the current state of the toggle
            ShowAddresses = !ShowAddresses;

            // Update the checkmark in the menu
            Menu.SetChecked("FX/Show Addresses", ShowAddresses);
            EditorPrefs.SetBool("FX: Show Addresses" ,ShowAddresses);


            // Perform any necessary actions when the state changes.
            // This could be calling some method in your FXManager or elsewhere,
            // depending on how you're implementing this feature.
            // FXManager.Instance.ShowAddresses(showAddresses);
        }
    }
}
