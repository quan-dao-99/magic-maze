using System;
using Photon.Pun;
using UnityEngine;

namespace LiftStudio
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimatorManager : MonoBehaviourPun
    {
        [SerializeField] private float directionDampTime = 0.25f;

        private Animator _animator;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Direction = Animator.StringToHash("Direction");
        private static readonly int Jump = Animator.StringToHash("Jump");

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
            
            if (!_animator) return;

            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Base Layer.Run"))
            {
                if (Input.GetButtonDown("Fire2"))
                {
                    _animator.SetTrigger(Jump);
                }
            }
            
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");
            if (v < 0)
            {
                v = 0;
            }

            _animator.SetFloat(Speed, h * h + v * v);
            _animator.SetFloat(Direction, h, directionDampTime, Time.deltaTime);
        }
    }
}