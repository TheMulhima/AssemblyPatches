using MonoMod;
using Patches.MiniSaveStates;
using Patches.ScreenShake;
using UnityEngine;

namespace Patches;

[MonoModPatch("global::GameManager")]
public class MainMenuWarning : global::GameManager
{
    public string WarningText;
    public const bool IsMiniSaveStatesActive = true;
    public const bool IsScreenShakeModifierActive = true;
    
    public extern void orig_Start();
    
    public extern void orig_OnApplicationQuit();

    public void Start()
    {
        orig_Start();
        if (IsMiniSaveStatesActive && IsScreenShakeModifierActive)
        {
            WarningText = "MiniSaveStates and ScreenShakeModifier Active";
        }
        else if (IsMiniSaveStatesActive)
        {
            WarningText = "MiniSaveStates Active";
        }
        else if (IsScreenShakeModifierActive)
        {
            WarningText = "ScreenShakeModifier Active";
        }
    }

    public void OnApplicationQuit()
    {
        orig_OnApplicationQuit();
        if (IsMiniSaveStatesActive)
        {
            SaveStateManager.SaveKeybinds();
        }
        if (IsScreenShakeModifierActive)
        {
            ScreenShakeModifier.SaveMultiplier();
        }
    }

    private void OnGUI()
    {
        if (this.GetSceneNameString() == Constants.MENU_SCENE)
        {
            var oldBackgroundColor = GUI.backgroundColor;
            var oldContentColor = GUI.contentColor;
            var oldColor = GUI.color;
            var oldMatrix = GUI.matrix;

            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.identity,
                new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f)
            );

            GUI.Label(
                new Rect(20f, 20f, 200f, 200f),
                WarningText,
                new GUIStyle
                {
                    fontSize = 30,
                    normal = new GUIStyleState
                    {
                        textColor = Color.white,
                    }
                }
            );

            GUI.backgroundColor = oldBackgroundColor;
            GUI.contentColor = oldContentColor;
            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }
    }
}