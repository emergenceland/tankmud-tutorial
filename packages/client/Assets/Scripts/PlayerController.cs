#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DefaultNamespace;
using IWorld.ContractDefinition;
using mud.Client;
using mud.Unity;
using UniRx;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

public class PlayerController : MonoBehaviour
{
	private Camera _camera;
	private Vector3? _destination;

	public GameObject destinationMarker;
	private GameObject _destinationMarker;

	private bool _hasDestination;
	private IDisposable? _disposer;
	private TankShooting _target;
	private PlayerSync _player;

	void Start()
	{
		_camera = Camera.main;
		var target = FindObjectOfType<TankShooting>();
		if (target == null) return;
		_target = target;

		_player = GetComponent<PlayerSync>();

		var sub = PositionTable.OnRecordInsert().Merge(PositionTable.OnRecordUpdate());
		_disposer = ObservableExtensions.Subscribe(sub.ObserveOnMainThread(), OnChainPositionUpdate);
	}

	// TODO: Callback for Position table update
	private void OnChainPositionUpdate(PositionTableUpdate update)
	{
		if (_player.key == null || update.Key != _player.key) return;
		if (_player.IsLocalPlayer()) return;
		var currentValue = update.TypedValue.Item1;
		if (currentValue == null) return;
		var x = Convert.ToSingle(currentValue.x);
		var y = Convert.ToSingle(currentValue.y);
		_destination = new Vector3(x, 0, y);
	}


	// TODO: Send tx
	private async UniTaskVoid SendMoveTxAsync(int x, int y)
	{
		try
		{
			await NetworkManager.Instance.worldSend.TxExecute<MoveFunction>(x, y);
		}
		catch (Exception ex)
		{
			// Handle your exception here
			Debug.LogException(ex);
		}
	}

	void Update()
	{
		var pos = transform.position;
		if (_destination.HasValue && Vector3.Distance(pos, _destination.Value) < 0.5)
		{
			_destination = null;
			if (_destinationMarker != null)
			{
				Destroy(_destinationMarker);
			}
		}
		else
		{
			if (_destination != null)
			{
				var newPosition = Vector3.Lerp(transform.position, _destination.Value, Time.deltaTime);
				var currentTransform = transform;
				currentTransform.position = newPosition;

				// Determine the new rotation
				var lookRotation = Quaternion.LookRotation(_destination.Value - currentTransform.position);
				var newRotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime);
				transform.rotation = newRotation;
			}
		}

		// TODO: Early return if not local player
		if (!_player.IsLocalPlayer() || _target.RangeVisible) return;
		if (Input.GetMouseButtonDown(0))
		{
			var ray = _camera.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out var hit)) return;
			if (hit.collider.name != "floor-large") return;

			var dest = hit.point;
			dest.x = Mathf.Floor(dest.x);
			dest.y = Mathf.Floor(dest.y);
			dest.z = Mathf.Floor(dest.z);
			_destination = dest;

			if (_destinationMarker != null)
			{
				Destroy(_destinationMarker);
			}

			_destinationMarker = Instantiate(destinationMarker, dest, Quaternion.identity);
			SendMoveTxAsync(Convert.ToInt32(dest.x), Convert.ToInt32(dest.z)).Forget();
		}
	}

	private void OnDestroy()
	{
		_disposer?.Dispose();
	}
}
