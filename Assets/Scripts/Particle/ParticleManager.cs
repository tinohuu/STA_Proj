using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public Transform ParticleGroup;
    [SerializeField] Transform triggerGroup;
    [SerializeField] Transform forceFieldGroup;
    [SerializeField] GameObject coinPrefab;

    ParticleSystemForceField[] fields;
    Collider[] triggers;

    public static ParticleManager Instance;
    
    void Awake()
    {
        if (!Instance) Instance = this;
    }

    void Start()
    {
        fields = forceFieldGroup.GetComponentsInChildren<ParticleSystemForceField>();
        triggers = triggerGroup.GetComponentsInChildren<Collider>();
    }

    public GameObject CreateCoin(Vector3 pos)
    {
        return CreateParticle(coinPrefab, pos);
    }

    public GameObject CreateParticle(GameObject prefab, Vector3 pos)
    {
        //List<Transform> particles = new List<Transform>();
        //GameObject particleObj = Resources.Load<GameObject>("Crops/HarvestParticles/FXHarvest" + cropName);
        GameObject obj = Instantiate(prefab, ParticleGroup);

        ParticleSystem particle;
        if (obj.transform.childCount > 0)
            particle = obj.transform.GetChild(0).GetComponentInChildren<ParticleSystem>();
        else
            particle = obj.transform.GetComponentInChildren<ParticleSystem>();
        //particle.transform.localScale = Vector3.one * Mathf.Abs(crop.Scale);
        particle.transform.position = pos;
        particle.transform.parent.SetParent(ParticleGroup);

        // Embed force fields and triggers
        var externalForcesModule = particle.externalForces;
        foreach (var field in fields) externalForcesModule.AddInfluence(field);
        var triggerModule = particle.trigger;
        foreach (var trigger in triggers) triggerModule.AddCollider(trigger);

        // Set sorting order
        var rendererModule = particle.GetComponent<ParticleSystemRenderer>();
        rendererModule.sortingLayerName = "Environment";
        rendererModule.sortingOrder = 1;

        return obj;
    }
}
