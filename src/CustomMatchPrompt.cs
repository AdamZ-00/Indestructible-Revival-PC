using UnityEngine;

public class CustomMatchPrompt : MonoBehaviour
{
	public enum PromptState
	{
		EnteringCode,
		WaitingHost,
		ConnectingGuest,
		Starting
	}

	private const float RefHeight = 480f;

	private SelectManager _selectManager;

	private int _targetPlayers = 2;

	private string _roomCode = "7777";

	private PromptState _state;

	private string _statusMessage = string.Empty;

	private Texture2D _white;

	private bool _focusRequested;

	private bool _closing;

	private bool _finished;

	private bool _locked;

	private float _closeTime;

	public static void Show(SelectManager selectManager, int targetPlayers)
	{
		CustomMatchPrompt customMatchPrompt = new GameObject("CustomMatchPrompt").AddComponent<CustomMatchPrompt>();
		customMatchPrompt._selectManager = selectManager;
		customMatchPrompt._targetPlayers = targetPlayers;
		customMatchPrompt._roomCode = PlayerPrefs.GetString("CustomMatch_RoomCode", "7777");
		Debug.Log("[CustomMatchPrompt] Opened for " + targetPlayers + " players");
	}

	private void Awake()
	{
		LockUI();
	}

	private void OnGUI()
	{
		if (_closing)
		{
			return;
		}
		GUI.depth = -1000;
		float num = (float)Screen.height / 480f;
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num, num, 1f));
		float num2 = (float)Screen.width / num;
		float height = 480f;
		if (_white == null)
		{
			_white = new Texture2D(1, 1);
			_white.SetPixel(0, 0, Color.white);
			_white.Apply();
		}
		GUI.color = new Color(0f, 0f, 0f, 0.82f);
		GUI.DrawTexture(new Rect(0f, 0f, num2, height), _white);
		GUI.color = Color.white;
		float num3 = Mathf.Min(540f, num2 - 40f);
		Rect position = new Rect((num2 - num3) / 2f, 90f, num3, 270f);
		if (_state == PromptState.EnteringCode)
		{
			GUI.Box(position, "PLAY WITH FRIENDS (PRIVATE ROOM)");
			GUI.Label(new Rect(position.x + 20f, position.y + 35f, num3 - 40f, 22f), "Room code (share the same code with your friend):");
			GUI.SetNextControlName("roomcode");
			_roomCode = GUI.TextField(new Rect(position.x + 20f, position.y + 60f, num3 - 40f, 38f), _roomCode ?? string.Empty, 16);
			if (!_focusRequested)
			{
				_focusRequested = true;
				GUI.FocusControl("roomcode");
			}
			GUI.enabled = !string.IsNullOrEmpty(_roomCode) && _roomCode.Trim().Length >= 1;
			float num4 = (num3 - 50f) / 2f;
			if (GUI.Button(new Rect(position.x + 20f, position.y + 115f, num4, 46f), "CREATE ROOM"))
			{
				StartHosting();
			}
			if (GUI.Button(new Rect(position.x + 30f + num4, position.y + 115f, num4, 46f), "JOIN ROOM"))
			{
				StartJoining();
			}
			GUI.enabled = true;
			if (GUI.Button(new Rect(position.x + 20f, position.y + 180f, num3 - 40f, 40f), "BACK"))
			{
				Close();
			}
		}
		else if (_state == PromptState.WaitingHost)
		{
			GUI.Box(position, "PRIVATE ROOM CREATED: " + _roomCode.Trim().ToUpper());
			int num5 = 1;
			if (PhotonNetwork.room != null)
			{
				num5 = PhotonNetwork.room.playerCount;
			}
			GUI.Label(new Rect(position.x + 20f, position.y + 45f, num3 - 40f, 28f), "Waiting for your friend in room \"" + _roomCode.Trim().ToUpper() + "\"...");
			GUI.Label(new Rect(position.x + 20f, position.y + 80f, num3 - 40f, 28f), "Players connected: " + num5 + " / " + _targetPlayers);
			if (!string.IsNullOrEmpty(_statusMessage))
			{
				GUI.Label(new Rect(position.x + 20f, position.y + 115f, num3 - 40f, 28f), _statusMessage);
			}
			if (GUI.Button(new Rect(position.x + 20f, position.y + 180f, num3 - 40f, 45f), "CANCEL"))
			{
				GCMatchmaker.CancelMatch();
				_state = PromptState.EnteringCode;
				_statusMessage = string.Empty;
			}
		}
		else if (_state == PromptState.ConnectingGuest)
		{
			GUI.Box(position, "JOINING ROOM: " + _roomCode.Trim().ToUpper());
			int num6 = 1;
			if (PhotonNetwork.room != null)
			{
				num6 = PhotonNetwork.room.playerCount;
				GUI.Label(new Rect(position.x + 20f, position.y + 45f, num3 - 40f, 28f), "Connected to room \"" + _roomCode.Trim().ToUpper() + "\"!");
				GUI.Label(new Rect(position.x + 20f, position.y + 80f, num3 - 40f, 28f), "Players connected: " + num6 + " / " + _targetPlayers);
			}
			else
			{
				GUI.Label(new Rect(position.x + 20f, position.y + 50f, num3 - 40f, 28f), "Searching for room \"" + _roomCode.Trim().ToUpper() + "\"...");
			}
			if (!string.IsNullOrEmpty(_statusMessage))
			{
				GUI.Label(new Rect(position.x + 20f, position.y + 115f, num3 - 40f, 28f), _statusMessage);
			}
			if (GUI.Button(new Rect(position.x + 20f, position.y + 180f, num3 - 40f, 45f), "CANCEL"))
			{
				GCMatchmaker.CancelMatch();
				_state = PromptState.EnteringCode;
				_statusMessage = string.Empty;
			}
		}
		else if (_state == PromptState.Starting)
		{
			GUI.Box(position, "ALL PLAYERS ARE READY!");
			GUI.Label(new Rect(position.x + 20f, position.y + 80f, num3 - 40f, 35f), "Starting match...");
		}
		GUI.matrix = matrix;
	}

	private void StartHosting()
	{
		string text = _roomCode.Trim().ToUpper();
		PlayerPrefs.SetString("CustomMatch_RoomCode", text);
		PlayerPrefs.Save();
		GCMatchmaker.CurrentCustomRoomCode = text;
		_state = PromptState.WaitingHost;
		_statusMessage = "Creating room...";
		if (_selectManager != null)
		{
			_selectManager.StartGame();
		}
	}

	private void StartJoining()
	{
		string text = _roomCode.Trim().ToUpper();
		PlayerPrefs.SetString("CustomMatch_RoomCode", text);
		PlayerPrefs.Save();
		GCMatchmaker.CurrentCustomRoomCode = text;
		_state = PromptState.ConnectingGuest;
		_statusMessage = "Connecting to Photon...";
		if (_selectManager != null)
		{
			_selectManager.JoinCustom(text);
		}
	}

	public void OnMatchReady()
	{
		_state = PromptState.Starting;
		UnlockUI();
		Invoke("CloseImmediate", 0.5f);
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (_state == PromptState.EnteringCode)
			{
				Close();
				return;
			}
			GCMatchmaker.CancelMatch();
			_state = PromptState.EnteringCode;
		}
	}

	private void Close()
	{
		if (!_closing)
		{
			_closing = true;
			_closeTime = Time.realtimeSinceStartup + 0.35f;
		}
	}

	private void CloseImmediate()
	{
		Finish();
	}

	private void LateUpdate()
	{
		if (_closing && Time.realtimeSinceStartup >= _closeTime)
		{
			Finish();
		}
	}

	private void Finish()
	{
		if (!_finished)
		{
			_finished = true;
			UnlockUI();
			Object.Destroy(base.gameObject);
		}
	}

	private void LockUI()
	{
		if (!_locked && UIManager.instance != null)
		{
			UIManager.instance.LockInput();
			_locked = true;
		}
	}

	private void UnlockUI()
	{
		if (_locked && UIManager.instance != null)
		{
			UIManager.instance.UnlockInput();
		}
		_locked = false;
	}

	private void OnDestroy()
	{
		UnlockUI();
	}
}
