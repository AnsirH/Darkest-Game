using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomProp : MonoBehaviour
{
    [Header("오브젝트 목록")]
    public GameObject[] props;

    private void OnEnable()
    {
        ActivateRandomProp();
    }

    void ActivateRandomProp()
    {
        if (props.Length == 0) { return; }

        int randomIndex = Random.Range(0, props.Length);

        for (int i = 0; i < props.Length; i++) { props[i].SetActive(false); }

        SetRandomRotation(props[randomIndex]);

        props[randomIndex].SetActive(true);
    }

    void SetRandomRotation(GameObject go)
    {
        int randomRotation = Random.Range(0, 360);

        go.transform.Rotate(Vector3.up, randomRotation);
    }
}
