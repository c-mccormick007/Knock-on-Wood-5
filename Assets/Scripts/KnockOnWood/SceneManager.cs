using UnityEngine;
using FusionHelpers;
using Fusion;
using System;


namespace bandcProd
{
    public class SceneManager : NetworkSceneManagerDefault
    {
        private SceneRef _loadedScene = SceneRef.None;
        public Action<NetworkRunner, FusionLauncher.ConnectionStatus, string> onStatusUpdate { get; set; }
    }
}
