using MonoMod;

namespace Patches.ScreenShake;

[MonoModPatch("global::GameCameras")]
public class GameCamerasPatch : global::GameCameras
{
    public extern void orig_Start();

    public void Start()
    {
        orig_Start();
        if (MainMenuWarning.IsScreenShakeModifierActive)
        {
            ScreenShakeModifier.EditScreenShake(this);
        }
    }
}