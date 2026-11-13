using System;
using UnityEngine;

public class PhotonAppIDPrompt : MonoBehaviour
{
	private const float RefHeight = 480f;

	private string _text = string.Empty;

	private Action _onComplete;

	private bool _focusRequested;

	private Texture2D _white;

	private bool _closing;

	private bool _finished;

	private bool _locked;

	private float _closeTime;

	private Action _pendingCallback;

	private void Awake()
	{
		LockUI();
	}

	public static void Show(Action onComplete)
	{
		PhotonAppIDPrompt photonAppIDPrompt = new GameObject("PhotonAppIDPrompt").AddComponent<PhotonAppIDPrompt>();
		photonAppIDPrompt._onComplete = onComplete;
		photonAppIDPrompt._text = NetworkManager.GetCurrentAppID();
		Debug.Log("[PhotonAppIDPrompt] shown");
	}

	private void OnGUI()
	{
		if (!_closing)
		{
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
			GUI.color = new Color(0f, 0f, 0f, 0.75f);
			GUI.DrawTexture(new Rect(0f, 0f, num2, height), _white);
			GUI.color = Color.white;
			float num3 = Mathf.Min(520f, num2 - 40f);
			Rect position = new Rect((num2 - num3) / 2f, 110f, num3, 230f);
			GUI.Box(position, "MULTIPLAYER APP ID (Photon)");
			GUI.Label(new Rect(position.x + 15f, position.y + 40f, num3 - 30f, 24f), "Paste your Photon App ID (tap the box):");
			GUI.SetNextControlName("appid");
			_text = GUI.TextField(new Rect(position.x + 15f, position.y + 70f, num3 - 30f, 40f), _text ?? string.Empty, 64);
			if (!_focusRequested)
			{
				_focusRequested = true;
				GUI.FocusControl("appid");
			}
			GUI.enabled = !string.IsNullOrEmpty(_text) && _text.Trim().Length >= 11;
			if (GUI.Button(new Rect(position.x + 15f, position.y + 150f, (num3 - 40f) / 2f, 45f), "CONFIRM"))
			{
				NetworkManager.SetCustomAppID(_text.Trim());
				Close(ok: true);
			}
			GUI.enabled = true;
			if (GUI.Button(new Rect(position.x + 25f + (num3 - 40f) / 2f, position.y + 150f, (num3 - 40f) / 2f, 45f), "CANCEL"))
			{
				Close(ok: false);
			}
			GUI.matrix = matrix;
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Close(ok: false);
		}
	}

	private void Close(bool ok)
	{
		if (!_closing)
		{
			_closing = true;
			_pendingCallback = (ok ? _onComplete : null);
			_onComplete = null;
			_closeTime = Time.realtimeSinceStartup + 0.35f;
		}
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
			Action pendingCallback = _pendingCallback;
			_pendingCallback = null;
			UnityEngine.Object.Destroy(base.gameObject);
			pendingCallback?.Invoke();
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
