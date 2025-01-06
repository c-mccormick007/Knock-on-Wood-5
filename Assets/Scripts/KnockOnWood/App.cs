using Fusion;
using FusionHelpers;
using TMPro;
using bandcProd.UIHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace bandcProd
{
    public class App : MonoBehaviour
    {
        [SerializeField] private Panel _uiMenu;
        [SerializeField] private Panel _uiSinglePlayerMenu;
        [SerializeField] private Panel _uiMultiplePlayerMenu;
		[SerializeField] private TextMeshProUGUI _progress;
        [SerializeField] private string _room;
        [SerializeField] private string _roomName;
        [SerializeField] private FusionSession _gameManagerPrefab;
        [SerializeField] private GameObject _gameManager;
        [SerializeField] private INetworkSceneManager _sceneManager;

        private FusionLauncher.ConnectionStatus _status = FusionLauncher.ConnectionStatus.Disconnected;
        private GameMode _gameMode = GameMode.Shared;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            //DontDestroyOnLoad(this);
        }
        private void Start()
        {
            OnConnectionStatusUpdate(null, FusionLauncher.ConnectionStatus.Disconnected, "");
        }

        public void OnEnterRoom()
        {
        }
		private void OnConnectionStatusUpdate(NetworkRunner runner, FusionLauncher.ConnectionStatus status, string reason)
        {
            if (!this)
                return;

            Debug.Log(status);

            if (status != _status)
            {
                switch (status)
                {
                    case FusionLauncher.ConnectionStatus.Disconnected:
                        Debug.Log("Disconnected! " + reason);
                        break;
                    case FusionLauncher.ConnectionStatus.Failed:
                        Debug.Log("Error! " + reason);
                        break;
                }
            }

            _status = status;
            UpdateUI();
        }

		private void UpdateUI()
		{
            bool running = false;

			switch (_status)
			{
				case FusionLauncher.ConnectionStatus.Disconnected:
					_progress.text = "Disconnected!";
					break;
				case FusionLauncher.ConnectionStatus.Failed:
					_progress.text = "Failed!";
					break;
				case FusionLauncher.ConnectionStatus.Connecting:
					_progress.text = "Connecting";
					break;
				case FusionLauncher.ConnectionStatus.Connected:
					_progress.text = "Connected";
					break;
				case FusionLauncher.ConnectionStatus.Loading:
					_progress.text = "Loading";
					break;
				case FusionLauncher.ConnectionStatus.Loaded:
					running = true;
					break;
			}
		}
        private bool GateUI(Panel ui)
        {
            if (!ui.isVisible)
                return false;
            return true;
        }
        public void PopulateMenuPanel()
        {
            _uiMenu.ToggleActivationTrue();
            _uiMenu.TogglePanel();
        }
        public void PopulateMultiplayerPanel()
        {
            SceneManager.LoadScene(1);
        }
    }
}