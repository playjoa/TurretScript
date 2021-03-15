using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretScript : MonoBehaviour
{
    //Turret Transforms
    [SerializeField]
    private Transform turretRotation, bulletSpawnPoint;

    //Turret Stats
    [SerializeField]
    private float timeToSearch = 0.5f, fireRate = 0.25f, turretRange = 10;

    private float auxFireRate, raioAtuacao;
    
    //Current Target
    private GameObject targetToShot = null;

    //Possible Targets 
    private List<GameObject> PossibleTargets = new List<GameObject>();
    private Collider[] ObjectsWithinRange;
    private float[] distances;

    //Turret Movements
    private Vector3 movement;
    private float angle;

    private void Start()
    {
        InitializeVariables();
        InvokeRepeating("SeekTarget", 1f, timeToSearch);
    }

    //Chooses id in array with lower value 
    int LowerValue(float[] values)
    {
        int id = 0;
        float refLower = values[0];

        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] < refLower)
            {
                refLower = values[i];
                id = i;
            }
        }

        return id;
    }

    void InitializeVariables() 
    {
        auxFireRate = fireRate;
    }

    bool isOutOfRange(Vector3 targetPosition)
    {
        float currentDistance = Vector3.Distance(this.transform.position, targetPosition);

        if(currentDistance > turretRange)
            return true;
        else
            return false;
    }


    //This takes in account your enemies are being pooled, you can use other conditions to get a target
    void SeekTarget()
    {
        //if your current enemie has died or went out of range it seeks other
        if(!targetToShot.activeSelf || isOutOfRange(targetToShot.transform.position))
        {
            GetTarget();
        }
    }

    void GetTarget() 
    {
        //Preparing lists to receive targets in range
        PossibleTargets = new List<GameObject>();
        ObjectsWithinRange = Physics.OverlapSphere(this.transform.position, turretRange);

        foreach (Collider obj in ObjectsWithinRange)
        {
            //Here you can add more conditions, for example check if this enemy is dead
            if (obj.gameObject.CompareTag("enemy"))
                PossibleTargets.Add(obj.gameObject);
        }

        // No enemie is in range
        if (PossibleTargets.Count == 0)
        {
            targetToShot = null;
            return;
        }

        distances = new float[PossibleTargets.Count];

        //Getting distances from turret to target
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = Vector3.Distance(this.transform.position, PossibleTargets[i].transform.position);
        }

        //Choosing closest target
        int idObject = LowerValue(distances);

        targetToShot = PossibleTargets[idObject].gameObject;
    }

    void ShootAtTarget() 
    {
        //Enemie has died
        if (!targetToShot.activeSelf)
        {
            targetToShot = null;
            return;
        }

        auxFireRate -= Time.deltaTime;

        if (auxFireRate <= 0)
        {
            ShootAtTarget();
            auxFireRate = fireRate;
        }
    }

    void ShootAtTarget()
    {   
        //Instatiate your bullet prefab here at the desired bullet spawn
        //spawnerPooler.SpawnFromPool(tagAmmo, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
    }

    void AimAtTarget(Vector3 target) 
    {
        //Rotates gun towards enemies
        
        movement = new Vector3(target.x - this.transform.position.x, 0f, target.z - this.transform.position.z);
        movement.Normalize();

        angle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
        turretRotation.transform.rotation = Quaternion.Euler(0, angle, 0);

        bulletSpawnPoint.transform.LookAt(target);
    }

    private void Update()
    {
        if (targetToShot == null)
            return;

        AimAtTarget(targetToShot.transform.position);
        ShootAtTarget();
    }
}
