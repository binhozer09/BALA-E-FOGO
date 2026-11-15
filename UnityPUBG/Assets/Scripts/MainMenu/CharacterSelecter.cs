using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPUBG.Scripts.Logic;

namespace UnityPUBG.Scripts.MainMenu
{
    public class CharacterSelecter : MonoBehaviour
    {
        /// <summary>
        /// 캐릭터 3D 모델 프리팹 리스트들
        /// </summary>
        public List<GameObject> CharacterPrefabList = new List<GameObject>();
        public int RotateSpeed;
        /// <summary>
        /// 플레이어 캐릭터 프리팹
        /// </summary>
        public GameObject PlayerCharacter;

        /// <summary>
        /// 캐릭터 리스트
        /// </summary>
        private List<GameObject> characterList = new List<GameObject>();
        /// <summary>
        /// 각도
        /// </summary>
        private float deg;
        /// <summary>
        /// 드래그 중인가
        /// </summary>
        private bool isDrag = false;
        /// <summary>
        /// 마우스 최근 위치
        /// </summary>
        private Vector3 lastpos;
        /// <summary>
        /// 최근 Selecter 회전율
        /// </summary>
        private Vector3 lastrot;
        /// <summary>
        /// 선택된 캐릭터 이름
        /// </summary>
        private string selectedCharacterName;

        [SerializeField]
        private Transform CharacterSpawnPosition;

        #region 유니티 메시지
        private void Start()
        {
            putCharacter();
            setSelectedCharacter();

            //나중에 주석 해제할 것
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {            
            if (CharacterSpawnPosition == null)
            {
                return;
            }

            Vector3 nowpos;

#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    lastpos = Input.GetTouch(0).position;
                    lastrot = CharacterSpawnPosition.transform.rotation.eulerAngles;
                }

                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    nowpos = Input.GetTouch(0).position;

                    int x = (int)((nowpos - lastpos).x / Screen.width * RotateSpeed);

                    CharacterSpawnPosition.transform.rotation = Quaternion.Euler
                            (Vector3.up * deg * x + lastrot);
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    lastrot = CharacterSpawnPosition.transform.rotation.eulerAngles;
                    setSelectedCharacter();
                }
            }
#else

            // 마우스가 눌림
            if (Input.GetMouseButtonDown(0))
            {
                isDrag = true;
                lastrot = CharacterSpawnPosition.transform.rotation.eulerAngles;
                lastpos = Input.mousePosition;
            }

            // 마우스가 떼짐
            if (Input.GetMouseButtonUp(0))
            {
                isDrag = false;
                lastrot = CharacterSpawnPosition.transform.rotation.eulerAngles;
                setSelectedCharacter();

                //Debug.Log(selectedCharacterName);
            }

            if (isDrag)
            {
                nowpos = Input.mousePosition;

                int x = (int)((nowpos - lastpos).x / Screen.width * RotateSpeed);

                CharacterSpawnPosition.transform.rotation = Quaternion.Euler
                    (Vector3.up * deg * x + lastrot);
            }
#endif
        }
        #endregion

        /// <summary>
        /// 캐릭터를 스폰함.
        /// </summary>
        public void SpawnMyCharacter(Vector3 spawnPos)
        {
            spawnMyCharacter(spawnPos);
        }

        /// <summary>
        /// 리스트에 있는 캐릭터 들을 원형으로 배치
        /// </summary>
        private void putCharacter()
        {
            if (CharacterSpawnPosition == null)
            {
                Debug.LogError("CharacterSpawnPosition is not assigned.");
                return;
            }

            if (CharacterPrefabList == null || CharacterPrefabList.Count == 0)
            {
                Debug.LogError("CharacterPrefabList is empty. Add at least one character preview prefab.");
                return;
            }

            float num = CharacterPrefabList.Count;

            //원주
            float circlelen = 3 * num;

            //반지름
            float r = circlelen / (2 * Mathf.PI);

            //원의 중심
            Vector3 centerpos = CharacterSpawnPosition.transform.position - Vector3.forward * r;

            //중심 변경
            CharacterSpawnPosition.transform.position = centerpos;

            //1개 각도
            deg = 360f / num;

            //1개 각도(라디안)
            float rad = Mathf.Deg2Rad * (360f / num);

            for (int i = 0; i < num; i++)
            {
                GameObject tmp = Instantiate(CharacterPrefabList[i]);

                //위치 설정
                tmp.transform.position = centerpos
                    + Vector3.right * Mathf.Cos(rad * i) * r
                    + Vector3.back * Mathf.Sin(rad * i) * r;

                //회전 설정
                tmp.transform.rotation = Quaternion.Euler(Vector3.up * (-90f + deg * i));

                characterList.Add(tmp);

                tmp.transform.SetParent(CharacterSpawnPosition.transform);
            }

            if (Camera.main != null)
            {
                Camera.main.transform.position = centerpos + Vector3.forward * 7f + Vector3.up;
            }
            else
            {
                Debug.LogWarning("Character preview camera was not found (tag MainCamera).");
            }
        }

        /// <summary>
        /// 전면의 gameObject return
        /// </summary>
        /// <returns></returns>
        private GameObject getFrontObject()
        {
            RaycastHit hit;
            GameObject target = null;

            //화면 가운데 좌표에 레이캐스트
            Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(Vector3.zero));

            //게임 오브젝트가 존재한다면 return
            if (true == (Physics.Raycast(ray.origin, ray.direction, out hit)))
                target = hit.collider.gameObject;//있다면 오브젝트 저장

            return target;
        }

        /// <summary>
        /// 선택된 캐릭터 이름 저장
        /// </summary>
        private void setSelectedCharacter()
        {
            GameObject selectedCharacter = getFrontObject();

            // Keep a safe default if the camera ray does not hit a character preview.
            if (selectedCharacter == null)
            {
                selectedCharacterName = "Character_Random";
                return;
            }

            string[] splitedName = selectedCharacter.name.Split('_');
            if (splitedName.Length < 2)
            {
                selectedCharacterName = "Character_Random";
                Debug.LogWarning("Selected preview object does not follow the Character_Name naming convention.");
                return;
            }

            selectedCharacterName = splitedName[0] + "_" + splitedName[1];
        }

        /// <summary>
        /// 내가 선택한 캐릭터 스폰
        /// </summary>
        private void spawnMyCharacter(Vector3 spawnPos)
        {
            if (PlayerCharacter == null)
            {
                Debug.LogError("PlayerCharacter prefab is not assigned on CharacterSelecter.");
                return;
            }

            GameObject playerCharacter = PhotonNetwork.Instantiate(PlayerCharacter.name, spawnPos, Quaternion.identity, 0);
            if (playerCharacter == null)
            {
                Debug.LogError("Photon could not instantiate the player character. Check the room connection and prefab registration.");
                return;
            }

            List<Transform> characterModels = new List<Transform>();
            foreach (Transform child in playerCharacter.transform)
            {
                if (child.name.StartsWith("Character"))
                {
                    child.gameObject.SetActive(false);
                    characterModels.Add(child);
                }
            }

            if (characterModels.Count == 0)
            {
                Debug.LogError("PlayerCharacter prefab has no child model whose name starts with 'Character'.");
                PhotonNetwork.Destroy(playerCharacter);
                return;
            }

            Transform selectedModel = null;
            if (!string.IsNullOrEmpty(selectedCharacterName) && selectedCharacterName != "Character_Random")
            {
                selectedModel = playerCharacter.transform.Find(selectedCharacterName);
            }

            if (selectedModel == null)
            {
                selectedModel = characterModels[Random.Range(0, characterModels.Count)];
            }

            selectedModel.gameObject.SetActive(true);
            selectedCharacterName = selectedModel.name;

            // Notify the other players about the selected character model.
            PhotonView playerPhotonView = playerCharacter.GetComponent<PhotonView>();
            if (playerPhotonView != null && PhotonNetwork.player != null)
            {
                playerPhotonView.RPC("WhatModelChoose", PhotonTargets.Others, selectedCharacterName, PhotonNetwork.player.NickName);
            }
            else
            {
                Debug.LogWarning("Player character PhotonView or local Photon player is missing; model selection was not synchronized.");
            }

            Destroy(gameObject);
        }
    }
}