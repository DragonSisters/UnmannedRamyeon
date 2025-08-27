using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotUIController : MonoBehaviour
{
    [SerializeField] private GameObject pot;
    [SerializeField] private List<SpriteRenderer> ingredientsInPot;

    private Vector2 pot_originalPos = new Vector2(0, 0);
    [SerializeField] private Animation potAnim;
    private float potAnimTime = 0.5f;

    private Queue<IEnumerator> potQueue = new Queue<IEnumerator>();
    private bool isPotQueueRunning = false;

    public void EnqueuePotRoutine(IEnumerator routine)
    {
        potQueue.Enqueue(routine);
        if (!isPotQueueRunning)
            StartCoroutine(ProcessPotQueue());
    }

    private IEnumerator ProcessPotQueue()
    {
        isPotQueueRunning = true;
        while (potQueue.Count > 0)
        {
            yield return StartCoroutine(potQueue.Dequeue());
        }
        isPotQueueRunning = false;
    }

    public IEnumerator BringPot()
    {
        pot.transform.position = pot_originalPos;
        pot.SetActive(true);
        potAnim.Play("pot_slideIn");

        yield return new WaitForSeconds(potAnimTime);
    }

    public IEnumerator RemovePot()
    {
        potAnim.Play("pot_slideOut");
        yield return new WaitForSeconds(potAnimTime);

        foreach (SpriteRenderer spriteRenderer in ingredientsInPot)
        {
            spriteRenderer.gameObject.SetActive(false);
        }

        pot.SetActive(false);
    }
}
