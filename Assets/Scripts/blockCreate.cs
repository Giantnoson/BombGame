using UnityEngine;

public class blockCreate : MonoBehaviour
{
    public GameObject prefab;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var go = Instantiate(prefab);
            var v3 = Input.mousePosition;
            v3.z = 19.5f;
            v3 = Camera.main.ScreenToWorldPoint(v3);
            print(v3);
            v3.x = Mathf.Ceil(v3.x) - 0.5f;
            v3.z = Mathf.Ceil(v3.z) - 0.5f;
            print(v3);
            go.transform.position = v3;
        }
    }
}