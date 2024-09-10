using UnityEngine;
using System.Collections;

public class InitiateMultiDisplay : MonoBehaviour
{
    void Start ()
    {
            // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
            // Check if additional displays are available and activate each.
    
        /*for (int i = 1; i < Display.displays.Length; i++)
            {
                Display.displays[i].Activate();
            }*/
        if(Application.platform != RuntimePlatform.WindowsEditor)
        {
            Display.displays[1].Activate();
        }
    }   
}
