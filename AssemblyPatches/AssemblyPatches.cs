using UnityEngine;
using MonoMod;
using Patches.Modifiers;

#pragma warning disable CS0626

namespace Patches;

[MonoModPatch("global::GameManager")]
public class GameManagerPatch : global::GameManager
{
    #if SCREENSHAKEONLY
    public const bool IsMiniSaveStatesActive = false;
    public const bool IsScreenShakeModifierActive = true;
    #endif
    #if MINISAVESTATESONLY
    public const bool IsMiniSaveStatesActive = true;
    public const bool IsScreenShakeModifierActive = false;
    #endif
    #if SCREENSHAKEANDMINISAVESTATES
    public const bool IsMiniSaveStatesActive = true;
    public const bool IsScreenShakeModifierActive = true;
    #endif
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

            string WarningText = string.Empty;
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

    public void Update()
    {
        if (!IsMiniSaveStatesActive) return;
        if (Input.GetKeyDown(SaveStateManager.Keybinds.SaveStateButton))
        {
            SaveStateManager.SaveState();
        }
        else if (Input.GetKeyDown(SaveStateManager.Keybinds.LoadStateButton))
        {
            SaveStateManager.LoadState();
        }
    }

    public extern void orig_Start();

    public void Start()
    {
        orig_Start();
        if (IsMiniSaveStatesActive) SaveStateManager.LoadKeybinds();
        if (IsScreenShakeModifierActive) ScreenShakeModifier.EditScreenShake();
    }
}