using System;
using UnityEngine;

namespace Patches;

[Serializable]
public class Keybinds
{
    public string LoadStateButton = "f1";
    public string SaveStateButton = "f2";
}

[Serializable]
public class Multiplier
{
    public float multiplier = 1f;
}

[Serializable]
public struct SavedState
{
    public string saveScene;
    public PlayerData savedPlayerData;
    public SceneData savedSceneData;
    public Vector3 savePos;
}