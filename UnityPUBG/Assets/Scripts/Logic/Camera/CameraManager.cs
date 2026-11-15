using Cinemachine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPUBG.Scripts.Entities;
using UnityPUBG.Scripts.Utilities;

namespace UnityPUBG.Scripts.Logic
{
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private CinemachineVirtualCamera dropShipCamera;

        private CinemachineVirtualCamera currentCamera;
        private ReadOnlyCollection<CinemachineVirtualCamera> virtualCameras;

        public Camera MainCamera => mainCamera;
        public CinemachineVirtualCamera PlayerCamera => playerCamera;
        public CinemachineVirtualCamera DropShipCamera => dropShipCamera;
        public CinemachineVirtualCamera CurrentCamera
        {
            get { return currentCamera; }
            set
            {
                currentCamera = value;

                if (virtualCameras != null)
                {
                    foreach (var virtualCamera in virtualCameras)
                    {
                        if (virtualCamera != null)
                        {
                            virtualCamera.enabled = false;
                        }
                    }
                }

                if (currentCamera == null)
                {
                    Debug.LogError("Cannot activate a camera because the requested Cinemachine camera is not assigned.");
                    return;
                }

                currentCamera.enabled = true;
            }
        }

        private void Awake()
        {
            if (mainCamera == null)
            {
                Debug.LogError($"{nameof(mainCamera)}가 할당되지 않았습니다");
            }
            if (playerCamera == null)
            {
                Debug.LogError($"{nameof(playerCamera)}가 할당되지 않았습니다");
            }
            if (dropShipCamera == null)
            {
                Debug.LogError($"{nameof(dropShipCamera)}가 할당되지 않았습니다");
            }

            virtualCameras = new ReadOnlyCollection<CinemachineVirtualCamera>(new List<CinemachineVirtualCamera>
            {
                playerCamera, dropShipCamera
            });

            if (PlayerCamera != null)
            {
                CurrentCamera = PlayerCamera;
            }

            if (EntityManager.Instance != null)
            {
                EntityManager.Instance.OnMyPlayerSpawn += SetupPlayerCameras;
            }
        }

        private void SetupPlayerCameras(object sender, EventArgs e)
        {
            var targetPlayer = EntityManager.Instance != null ? EntityManager.Instance.MyPlayer : null;
            if (targetPlayer == null)
            {
                Debug.LogWarning("Camera setup skipped because the local player has not spawned yet.");
                return;
            }

            if (PlayerCamera != null)
            {
                PlayerCamera.Follow = targetPlayer.transform;
                CurrentCamera = PlayerCamera;
            }

            if (minimapCamera != null)
            {
                var followCamera = minimapCamera.GetComponent<FollowCamera>();
                if (followCamera != null)
                {
                    followCamera.follow = targetPlayer.transform;
                }
                else
                {
                    Debug.LogWarning("Minimap camera is missing its FollowCamera component.");
                }
            }
        }
    }
}
