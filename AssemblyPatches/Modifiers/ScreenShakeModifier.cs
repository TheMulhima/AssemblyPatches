using System;
using System.IO;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Patches.Modifiers;

public static class ScreenShakeModifier
{
    public static void EditScreenShake()
    {
        LoadMultiplier();
        var fsm = GameCameras.instance.cameraShakeFSM;

        foreach (var state in fsm.FsmStates)
        {
            foreach (var action in state.Actions)
            {
                if (Constants.GAME_VERSION != "1.4.3.2")
                {
                    if (action is iTweenShakePosition iTweenShakePosition)
                    {
                        iTweenShakePosition.vector = iTweenShakePosition.vector.Value * Multiplier.multiplier;
                    }
                }
                else
                {
                    var type = action.GetType();
                    if (type.FullName == "HutongGames.PlayMaker.Actions.ShakePosition")
                    {
                        var extentsFieldInfo = type.GetField("extents", BindingFlags.Instance | BindingFlags.Public);
                        var extents = (FsmVector3) extentsFieldInfo.GetValue(action);
                        extentsFieldInfo.SetValue(action, (FsmVector3) (extents.Value * Multiplier.multiplier));
                    }
                }
            }
        }
    }

    public static Multiplier Multiplier = new Multiplier();
    public static string MultiplierPath => Path.Combine(Application.persistentDataPath, "screenShakeModifier.json");
    
    public static void LoadMultiplier()
    {
        try
        {
            if (!File.Exists(MultiplierPath))
            {
                File.WriteAllText(MultiplierPath, JsonUtility.ToJson(Multiplier, true));
            }
            
            Multiplier = JsonUtility.FromJson<Multiplier>(File.ReadAllText(MultiplierPath));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}