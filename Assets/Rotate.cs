using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    public Vector3 rotation;
    [SerializeField, Range(1, 20)]
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        rotation = RandomUnitVector();
        speed = Random.Range(5f, 20f);
    }

    // Update is called once per frame
    void Update()
    {
        RotateObject();
    }
    //a function that rotates the object by the rotation vector multiplied by speed
    public void RotateObject()
    {
        transform.Rotate(rotation * speed * Time.deltaTime);
    }

    //a function that creates a random unit vector
    public Vector3 RandomUnitVector()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        return new Vector3(x, y, z).normalized;
    }

}
