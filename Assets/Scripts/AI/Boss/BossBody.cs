using System;
using UnityEngine;

public class BossBody : MonoBehaviour
{
    public static event Action HitEvent;

    public void Hit()
    {
        HitEvent?.Invoke();
    }
}