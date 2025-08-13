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

        private void Awake()
        {
            player.hasDive += SwitchUnderwaterGroup;
        }

        public void SwitchUnderwaterGroup(bool isDiving)
        {
            AudioMixerGroup targetGroup = isDiving ? underwaterGroup : basicGroup;

            if (backgroundSource.outputAudioMixerGroup != targetGroup)
            {
                backgroundSource.outputAudioMixerGroup = targetGroup;
            }
        }
    }
}