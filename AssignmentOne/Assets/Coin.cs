using UnityEngine;
using System.Collections;
public class Coin : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameMangerSingleton.Instance.Coins++;
            GetComponent<Animator>().SetBool("Status",true);
            StartCoroutine(DestroyAfterSeconds());
        }
    }


    IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(0.09f);
        Destroy(gameObject);
    }
}
