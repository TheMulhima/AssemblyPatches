using System;
using System.Collections;
using System.IO;
using System.Reflection;
using InControl;
using UnityEngine;

namespace Patches.MiniSaveStates;

public static class SaveStateManager
{
    public static bool LoadedKeyBinds = false;
    
    private static object lockArea;
    private static readonly FieldInfo cameraGameplayScene = typeof(CameraController)
        .GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);
    private static FieldInfo cameraLockArea = typeof(CameraController)
        .GetField("currentLockArea", BindingFlags.Instance | BindingFlags.NonPublic);
    public static Keybinds Keybinds = new Keybinds();

    public static void LoadState()
    {
        SavedState savedState = new SavedState();
        try
        {
            savedState = JsonUtility.FromJson<SavedState>(
                File.ReadAllText(Application.persistentDataPath + "/minisavestates-saved.json")
            );
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        GameManager.instance.StartCoroutine(LoadStateCoro(savedState));
    }

    public static void SaveState()
    {
        var savedState = new SavedState
        {
            saveScene = GameManager.instance.GetSceneNameString(),
            savedPlayerData = PlayerData.instance,
            savedSceneData = SceneData.instance,
            savePos = HeroController.instance.gameObject.transform.position
        };
        try
        {
            File.WriteAllText(
                Application.persistentDataPath + "/minisavestates-saved.json",
                JsonUtility.ToJson(savedState)
            );
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        lockArea = cameraLockArea.GetValue(GameManager.instance.cameraCtrl);
    }

    public static string KeybindsJsonPath => Path.Combine(Application.persistentDataPath, "minisavestates.json");

    public static void LoadKeybinds()
    {
        try
        {
            Keybinds = JsonUtility.FromJson<Keybinds>(File.ReadAllText(KeybindsJsonPath));
            LoadedKeyBinds = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    public static void SaveKeybinds()
    {
        try
        {
            File.WriteAllText(KeybindsJsonPath,JsonUtility.ToJson(Keybinds, true));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private static IEnumerator LoadStateCoro(SavedState savedState)
    {
        var savedPd = savedState.savedPlayerData;
        var savedSd = savedState.savedSceneData;
        var saveScene = savedState.saveScene;
        var savePos = savedState.savePos;

        cameraLockArea = (
            cameraLockArea ??
            typeof(CameraController).GetField("currentLockArea", BindingFlags.Instance | BindingFlags.NonPublic)
        );
        GameManager.instance.ChangeToScene("Room_Sly_Storeroom", "", 0f);
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Room_Sly_Storeroom")
        {
            yield return null;
        }
        GameManager.instance.sceneData = (SceneData.instance = JsonUtility.FromJson<SceneData>(JsonUtility.ToJson(savedSd)));
        GameManager.instance.ResetSemiPersistentItems();
        yield return null;
        HeroController.instance.gameObject.transform.position = savePos;
        PlayerData.instance = (GameManager.instance.playerData = (HeroController.instance.playerData = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(savedPd))));
        GameManager.instance.ChangeToScene(saveScene, "", 0.4f);
        try
        {
            cameraLockArea.SetValue(GameManager.instance.cameraCtrl, lockArea);
            GameManager.instance.cameraCtrl.LockToArea(lockArea as CameraLockArea);
            cameraGameplayScene.SetValue(GameManager.instance.cameraCtrl, true);
        }
        catch (Exception message)
        {
            Debug.LogError(message);
        }
        yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == saveScene);
        HeroController.instance.playerData = PlayerData.instance;
        HeroController.instance.geoCounter.playerData = PlayerData.instance;
        HeroController.instance.geoCounter.TakeGeo(0);
        if (PlayerData.instance.MPCharge >= PlayerData.instance.maxMP)
        {
            int tmpMp = PlayerData.instance.MPCharge;
            HeroController.instance.TakeMP(PlayerData.instance.MPCharge);
            yield return null;
            HeroController.instance.AddMPChargeSpa(tmpMp);
        }
        else
        {
            HeroController.instance.AddMPChargeSpa(1);
            yield return null;
            HeroController.instance.TakeMP(1);
        }
        if (PlayerData.instance.MPReserveMax > 0)
        {
            int tmpReserve = PlayerData.instance.MPReserve;
            HeroController.instance.TakeReserveMP(PlayerData.instance.MPReserve);
            yield return null;
            HeroController.instance.AddMPChargeSpa(tmpReserve);
        }

        //Console.AddLine("LoadStateCoro end of func: " + data.savedPd.hazardRespawnLocation.ToString());
        //HeroController.instance.SetHazardRespawn(savedPd.hazardRespawnLocation, savedPd.hazardRespawnFacingRight);
        HeroController.instance.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
        HeroAnimationController component = HeroController.instance.GetComponent<HeroAnimationController>();
        typeof(HeroAnimationController).GetField("pd", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(component, PlayerData.instance);

        HeroController.instance.TakeHealth(1);
        HeroController.instance.AddHealth(1);
        GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        HeroController.instance.TakeHealth(1);
        HeroController.instance.AddHealth(1);

        GameManager.instance.inputHandler.RefreshPlayerData();
        yield break;
    }
}