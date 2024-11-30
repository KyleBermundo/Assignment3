using benjohnson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : Singleton<ArtifactManager>
{
    List<Artifact> artifacts;

    [SerializeField] List<A_Base> startingArtifacts = new List<A_Base>();

    [Header("Components")]
    [SerializeField] GameObject visualizerGO;

    protected override void Awake()
    {
        base.Awake();

        artifacts = new List<Artifact>();
    }

    private void Start()
    {
        foreach (A_Base a in startingArtifacts)
            AddArtifact(a);
    }

    public void AddArtifact(A_Base artifact)
    {
        Artifact _a = new Artifact(artifact, Instantiate(visualizerGO, transform.position + new Vector3(artifacts.Count * 2.25f, 0), Quaternion.identity, transform).GetComponent<ArtifactVisualizer>());
        artifacts.Add(_a);

        // Trigger pickup
        artifact.OnPickup();
        _a.TryTrigger();
    }

    class Artifact
    {
        public A_Base artifact;
        public ArtifactVisualizer visualizer;

        public Artifact(A_Base artifact, ArtifactVisualizer visualizer)
        {
            this.artifact = artifact;
            this.visualizer = visualizer;
            visualizer.Visualize(artifact);
        }

        public void TryTrigger()
        {
            if (artifact.triggered)
            {
                artifact.triggered = false;
                visualizer.Trigger();
            }
        }
    }

    #region Triggers

    public void TriggerStartOfTurn()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnStartOfTurn();
            artifact.TryTrigger();
        }
    }

    public void TriggerEndOfTurn()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnEndOfTurn();
            artifact.TryTrigger();
        }
    }

    public void TriggerStartOfEnemyTurn()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnStartOfEnemyTurn();
            artifact.TryTrigger();
        }
    }

    public void TriggerEnterRoom()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnEnterRoom();
            artifact.TryTrigger();
        }
    }

    public void TriggerEnterBossRoom()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnEnterBossRoom();
            artifact.TryTrigger();
        }
    }

    public void TriggerClearRoom()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnClearRoom();
            artifact.TryTrigger();
        }
    }

    public void TriggerTakeDamage()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnTakeDamage();
            artifact.TryTrigger();
        }
    }

    public void TriggerDealDamage()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnDealDamage();
            artifact.TryTrigger();
        }
    }

    public void TriggerKillEnemy()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnKillEnemy();
            artifact.TryTrigger();
        }
    }

    public void TriggerChestOpen()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnChestOpen();
            artifact.TryTrigger();
        }
    }

    public void TriggerBossDefeated()
    {
        foreach (Artifact artifact in artifacts)
        {
            artifact.artifact.OnBossDefeated();
            artifact.TryTrigger();
        }
    }

    #endregion
}
