using System;
using System.Collections.Generic;
using UniRx;
using mud.Client;
using mud.Network.schemas;
using mud.Unity;
using UnityEngine;
using ObservableExtensions = UniRx.ObservableExtensions;

public class HealthManager : MonoBehaviour
{
    private CompositeDisposable _disposable = new();
    private NetworkManager net;

    void Start()
    {
        net = NetworkManager.Instance;
        net.OnNetworkInitialized += SubscribeHealth;
    }

    void SubscribeHealth(NetworkManager nm)
    {
        var healthTable = new TableId("", "Health");
        var healthUpdated = new Query().In(healthTable);
        var sub = ObservableExtensions.Subscribe(net.ds.RxQuery(healthUpdated).ObserveOnMainThread(), OnHealthChange);
        _disposable.Add(sub);
    }

    // TODO: Callback for HealthTable update
    private void OnHealthChange((List<Record> SetRecords, List<Record> RemovedRecords) update)
    {
        foreach (var setRecord in update.SetRecords)
        {
            var tankHealth = FindTankHealthByKey(setRecord.key);
            if (tankHealth == null) continue;

            var currentValue = Convert.ToSingle(setRecord.value["value"]);
            tankHealth.SetHealth(currentValue);
        }

        foreach (var removedRecord in update.RemovedRecords)
        {
            var tankHealth = FindTankHealthByKey(removedRecord.key);
            tankHealth?.OnDeath();
        }
    }
    
    private TankHealth FindTankHealthByKey(string key)
    {
        // If there are many entities in the scene, it might be better to store the key-TankHealth pairs
        // in a Dictionary to improve lookup speed.
        foreach (var tankHealth in FindObjectsOfType<TankHealth>())
        {
            if (tankHealth._player.key == key)
            {
                return tankHealth;
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
