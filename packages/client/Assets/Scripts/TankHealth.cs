using System;
using System.Collections.Generic;
using UniRx;
using DefaultNamespace;
using mud.Client;
using mud.Network.schemas;
using mud.Unity;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using ObservableExtensions = UniRx.ObservableExtensions;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;
    private float m_CurrentHealth;

    public Slider m_Slider;
    public Image m_FillImage;
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab;
    public GameObject shell;
    private PlayerSync _player;
    private ParticleSystem m_ExplosionParticles;
    private bool m_Dead;
    private CompositeDisposable _disposable = new();

    private NetworkManager net;


    private void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionParticles.gameObject.SetActive(false);
        _player = GetComponent<PlayerSync>();
        net = NetworkManager.Instance;
        var healthTable = new TableId("", "Health");
        var healthUpdated = new Query().In(healthTable);
        var sub = ObservableExtensions.Subscribe(net.ds.RxQuery(healthUpdated).ObserveOnMainThread(), OnHealthChange);
        _disposable.Add(sub);
    }


    private void OnEnable()
    {
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        SetHealthUI();
    }

    // TODO: Callback for HealthTable update
    private void OnHealthChange((List<Record> SetRecords, List<Record> RemovedRecords) update)
    {
        Debug.Log("SET: " + JsonConvert.SerializeObject(update.SetRecords));
        Debug.Log("REMOVED: " + JsonConvert.SerializeObject(update.RemovedRecords));
        foreach (var setRecord in update.SetRecords)
        {
            if (setRecord.key != _player.key) continue;
            var currentValue = Convert.ToSingle(setRecord.value["value"]);
            if (currentValue >= 100) continue;
            var initialShellPosition = transform.position;
            initialShellPosition.y += 10;
            Instantiate(shell, initialShellPosition, Quaternion.LookRotation(Vector3.down));

            m_CurrentHealth = Convert.ToSingle(currentValue);
            SetHealthUI();
        }

        foreach (var removedRecord in update.RemovedRecords)
        {
            OnDeath();
        }
    }

    private void SetHealthUI()
    {
        // Adjust the value and colour of the slider.
        m_Slider.value = m_CurrentHealth;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }

    private void OnDeath()
    {
        m_Dead = true;
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);
        m_ExplosionParticles.Play();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
