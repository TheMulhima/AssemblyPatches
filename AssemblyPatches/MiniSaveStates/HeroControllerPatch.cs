using UnityEngine;
using MonoMod;
using System;
#pragma warning disable CS0626

namespace Patches.MiniSaveStates
{
    [MonoModPatch("global::HeroController")]
    public class HeroControllerPatch : global::HeroController
    {
        public extern void orig_Update();
        public void Update()
        {
            orig_Update();
            
            if (!MainMenuWarning.IsMiniSaveStatesActive) return;
            
            if (!SaveStateManager.LoadedKeyBinds) SaveStateManager.LoadKeybinds();
            
            if (Input.GetKeyDown(SaveStateManager.Keybinds.SaveStateButton))
            {
                SaveStateManager.SaveState();
            }
            else if (Input.GetKeyDown(SaveStateManager.Keybinds.LoadStateButton))
            {
                SaveStateManager.LoadState();
            }
        }
    }
}