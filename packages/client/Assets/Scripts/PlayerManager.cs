using System;
using System.Collections.Generic;
using DefaultNamespace;
using IWorld.ContractDefinition;
using mud.Client;
using mud.Network.schemas;
using mud.Unity;
using UniRx;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

public class PlayerManager : MonoBehaviour
{
	private CompositeDisposable _disposers = new();
	public GameObject playerPrefab;
	private NetworkManager net;

	// Start is called before the first frame update
	void Start()
	{
		net = NetworkManager.Instance;
		net.OnNetworkInitialized += Spawn;
	}

	async void Spawn(NetworkManager nm)
	{
		var addressKey = net.addressKey;
		var playerTable = new TableId("", "Player");
		var currentPlayer = net.ds.GetValue(playerTable, addressKey);
		if (currentPlayer == null)
		{
			await nm.worldSend.TxExecute<SpawnFunction>(0, 0);
		}

		var playerQuery = new Query().In(playerTable);
		var playerSub = ObservableExtensions.Subscribe(net.ds.RxQuery(playerQuery).ObserveOnMainThread(), OnUpdatePlayers);
		_disposers.Add(playerSub);
	}

	// TODO: Callback for PlayerTable update
	private void OnUpdatePlayers((List<Record> SetRecords, List<Record> RemovedRecords) update)
	{
		foreach (var setRecord in update.SetRecords)
		{
			var currentValue = setRecord.value;
			if (currentValue == null) continue;
			var positionTable = new TableId("", "Position");
			var playerPosition = net.ds.GetValue(positionTable, setRecord.key); 
			if (playerPosition == null) continue; 
			var playerSpawnPoint = new Vector3(Convert.ToSingle(playerPosition.value["x"]), 0, Convert.ToSingle(playerPosition.value["y"]));
			var player = Instantiate(playerPrefab, playerSpawnPoint, Quaternion.identity);
			// add to CameraControl's Targets array
			var cameraControl = GameObject.Find("CameraRig").GetComponent<CameraControl>();
			cameraControl.m_Targets.Add(player.transform);
			player.GetComponent<PlayerSync>().key = setRecord.key;
			if (setRecord.key != net.addressKey) continue;
			PlayerSync.localPlayerKey = setRecord.key;
		}
	}

	private void OnDestroy()
	{
		_disposers?.Dispose();
	}
}
