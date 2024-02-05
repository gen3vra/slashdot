using System.Collections;
using UnityEngine;

public class SecretCredits : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(14.777f);
        Destroy(gameObject);
    }
}
