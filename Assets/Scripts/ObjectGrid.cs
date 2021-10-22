using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrid : MonoBehaviour
{

    public GameObject thing;
    public Material[] materials;

    public bool centered = false;

    [Range(0f, 10f)] public float widthSpacing = 1f;

    [Range(0f, 10f)] public float heightSpacing = 1f;

    [Range(2, 100)]
    public int rows = 10;
    [Range(2, 100)]
    public int cols = 10;

    List<GameObject> things;

    public void CreateObjects()
    {
        EmptyGrid();
        things.Clear();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var go = Instantiate(thing, this.transform);
                var xp = centered ? i - rows * .5f : i;
                var zp = centered ? j - cols * .5f : j;
                var pos = new Vector3(transform.position.x + xp * widthSpacing,
                                       transform.position.y,
                                       transform.position.z + zp * heightSpacing);
                go.transform.position = pos;
                go.GetComponent<MeshRenderer>().material = materials[Random.Range(0, materials.Length)];
                if (Random.Range(0f, 1f) < .3f)
                {
                    go.AddComponent<PerObjectMaterialProperties>();
                }
                things.Add(go);
            }
        }
    }

    void EmptyGrid()
    {
        foreach (GameObject go in things)
        {
#if UNITY_EDITOR
            DestroyImmediate(go);
#else
            Destroy(go);
#endif
        }
    }

}
