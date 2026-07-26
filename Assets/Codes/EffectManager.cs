using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Effect")]
    [SerializeField] private GameObject disappearEffectPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private float effectDuration = 1.5f;

    private readonly Queue<GameObject> effectPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreatePool();
    }

    private void CreatePool()
    {

        for (int i = 0; i < initialPoolSize; i++)
            CreateNewEffect();
    }

    private GameObject CreateNewEffect()
    {
        GameObject effect = Instantiate(disappearEffectPrefab, transform);
        effect.SetActive(false);
        effectPool.Enqueue(effect);

        return effect;
    }

    public void PlayDisappearEffect(Vector3 position)
    {
        if (disappearEffectPrefab == null)
            return;

        GameObject effect = GetEffect();

        effect.transform.SetParent(null);
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);

        RestartParticles(effect);
        StartCoroutine(ReturnEffectToPool(effect));
    }

    private GameObject GetEffect()
    {
        if (effectPool.Count == 0)
            CreateNewEffect();

        return effectPool.Dequeue();
    }

    private void RestartParticles(GameObject effect)
    {
        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    private IEnumerator ReturnEffectToPool(GameObject effect)
    {
        yield return new WaitForSeconds(effectDuration);

        if (effect == null)
            yield break;

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        effect.SetActive(false);
        effect.transform.SetParent(transform);
        effectPool.Enqueue(effect);
    }
}
