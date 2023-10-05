using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    public Animator Diamond3;
    public Animator Diamond8;
    public Animator SpadeAce;
    public Animator Spade5;
    public Animator SpadeJack; 

    void Awake()
    {
        Resources.UnloadUnusedAssets();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public IEnumerator Actvate()
    {
        Diamond3.SetTrigger("Shuffle");
        Diamond8.SetTrigger("Shuffle");
        SpadeAce.SetTrigger("Shuffle");
        Spade5.SetTrigger("Shuffle");
        SpadeJack.SetTrigger("Shuffle");

        yield return new WaitForSeconds(2f);
    }
}
