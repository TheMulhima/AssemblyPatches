using System;
using UnityEngine;

namespace Patches.MiniSaveStates;

[Serializable]
public struct SavedState
{
    public string saveScene;
    public PlayerData savedPlayerData;
    public SceneData savedSceneData;
    public Vector3 savePos;
}