using UnityEngine;
using System.Collections;
public class Coin : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ValueSingleton.Instance.coin++;
            GetComponent<Animator>().SetBool("Status",true);
            StartCoroutine(DestroyAfterSeconds());
        }
    }


    IEnumerator DestroyAfterSeconds()
    {
        AudioSingleton.Instance.PlayCoin();
        yield return new WaitForSeconds(0.09f);
        Destroy(gameObject);
    }
}
