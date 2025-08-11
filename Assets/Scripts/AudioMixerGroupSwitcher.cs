using System;
using Player;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace DefaultNamespace
{
    public class AudioMixerGroupSwitcher : MonoBehaviour
    {
        [SerializeField] private AudioSource backgroundSource;
        [SerializeField] private AudioMixerGroup underwaterGroup;
        [SerializeField] private AudioMixerGroup basicGroup;

        [Inject] private PlayerController player;
        
        private void Update()
        {
            AudioMixerGroup targetGroup = player.isDiving ? underwaterGroup : basicGroup;

            if (backgroundSource.outputAudioMixerGroup != targetGroup)
            {
                backgroundSource.outputAudioMixerGroup = targetGroup;
            }
        }
    }
}