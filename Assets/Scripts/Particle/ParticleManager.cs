using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public Transform ParticleGroup;
    public Transform LeftSide;
    public Transform RightSide;
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

        List<ParticleSystem> particles = new List<ParticleSystem>();
        //ParticleSystem particle;
        if (obj.transform.childCount > 0)
        {
            particles.AddRange(obj.transform.GetComponentsInChildren<ParticleSystem>(false).ToList());
        }
        else
            particles.Add(obj.transform.GetComponentInChildren<ParticleSystem>());


        //particle.transform.localScale = Vector3.one * Mathf.Abs(crop.Scale);
        obj.transform.position = pos;
        obj.transform.parent.SetParent(ParticleGroup);

        foreach (var particle in particles)
        {
            // Embed force fields and triggers
            var externalForcesModule = particle.externalForces;
            foreach (var field in fields) externalForcesModule.AddInfluence(field);
            var triggerModule = particle.trigger;
            foreach (var trigger in triggers) triggerModule.AddCollider(trigger);

            // Set sorting order
            var rendererModule = particle.GetComponent<ParticleSystemRenderer>();
            rendererModule.sortingLayerName = "Environment";
            rendererModule.sortingOrder = 3;
        }

        return obj;
    }
}
