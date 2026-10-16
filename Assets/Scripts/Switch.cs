using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Switch : MonoBehaviour
{
    public UnityEvent onActivate; 
    
    public bool triggerOnce = true;
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        Activate();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player")) return;

        Activate();
    }

    public void Activate()
    {
        if (triggerOnce && isActivated) return;
        isActivated = true;
        onActivate.Invoke(); 
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.green;
    }
}
