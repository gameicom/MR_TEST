using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649
    public class MLPhotonSyncManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Public Fields

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [SerializeField]
        GameObject originMesh;

        public List<Dictionary<short, byte[]>> mapDic = new List<Dictionary<short, byte[]>>();

        public Dictionary<short, byte[]> meshDic = new Dictionary<short, byte[]>();

        public bool bMulti = false;
        int iStep = 0;
        float time = 0f;

        #endregion

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
#if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

        void SetDictionary(List<Dictionary<short, byte[]>> dic)
        {
            //int size = 5;
            //int index = 0;

            //int maxSize = 0;
            //int maxIndex = 0;

            //List<short> removeList = new List<short>();
            
            //foreach(KeyValuePair<short, byte[]> kv in dic)
            //{
            //    size += 3+5+ kv.Value.Length;

            //    if(kv.Value.Length > 32758)
            //    {
            //        removeList.Add(kv.Key);
            //    }

            //    if (maxSize < kv.Value.Length)
            //    {
            //        maxSize = kv.Value.Length;
            //        maxIndex = index;
            //    }
            //    index++;
            //}

            //size -= dic.Count * 2;

            //Debug.LogError("SetDictionary!! Size = "+ size + " > " + "32767\nmaxIndex = " + maxIndex+" maxSize = "+ maxSize);

            //for(int i=0; i<removeList.Count; i++)
            //{
            //    dic.Remove(removeList[i]);
            //}

            //maxSize = 0;

            //foreach (KeyValuePair<short, byte[]> kv in dic)
            //{
            //    if (maxSize < kv.Value.Length)
            //    {
            //        maxSize = kv.Value.Length;
            //    }
            //}

            //Debug.LogError("maxSize = " + maxSize);

            mapDic = dic;
        }

        void SendDictionary(Dictionary<short, byte[]> dic)
        {
            meshDic = dic;
            CreateMesh();
        }

        IEnumerator DeleteData()
        {
            string appPath = Application.persistentDataPath;

            int count = 0;

            while (File.Exists(appPath + "/testData" + count + ".data"))
            {
                File.Delete(appPath + "/testData" + count + ".data");
                count++;
            }

            count = 0;
            while (File.Exists(appPath + "/testData" + count + ".data"))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        public void SaveMesh()
        {
            if (mapDic.Count < 1)
            {
                Debug.LogError("SaveDataNull");
                return;
            }

            StartCoroutine(FileSaveStart());
        }

        IEnumerator FileSaveStart()
        {

            string appPath = Application.persistentDataPath;

            yield return DeleteData();

            int count = 0;

            for (count = 0; count < mapDic.Count; count++)
            {
                byte[][] block = new byte[mapDic[count].Count][];

                for (short i = 0; i < mapDic[count].Count; i++)
                {
                    block[i] = new byte[mapDic[count][i].Length];
                    block[i] = mapDic[count][i];
                }

                BinaryFormatter bf = new BinaryFormatter();

                FileStream fs = new FileStream(appPath + "/testData" + count + ".data", FileMode.Create, FileAccess.Write);
                bf.Serialize(fs, block);
            }

            
            while (!File.Exists(appPath + "/testData" + (count-1) + ".data"))
            {
                Debug.LogError("SaveError");
                yield return new WaitForEndOfFrame();
            }

            Debug.LogError("Save");

            yield return new WaitForEndOfFrame();
        }

        public bool LoadMesh()
        {
            int count = 0;

            string appPath = Application.persistentDataPath;

            if (!File.Exists(appPath+"/testData" + count + ".data"))
            {
                Debug.LogError("LoadFileNull");
                return false;
            } 

            for(int i=0; i<transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            mapDic = new List<Dictionary<short, byte[]>>();

            while (File.Exists(appPath + "/testData" + count + ".data"))
            {
                FileStream fs = new FileStream(appPath + "/testData" + count + ".data", FileMode.Open, FileAccess.Read);

                BinaryFormatter bf = new BinaryFormatter();
                byte[][] block = (byte[][])bf.Deserialize(fs);

                Dictionary<short, byte[]> dic = new Dictionary<short, byte[]>();

                for (short i = 0; i < block.Length; i++)
                {
                    dic.Add(i, block[i]);
                }

                mapDic.Add(dic);
                count++;
            }

            time = 0f;
            iStep = 0;

            Debug.LogError("Load");
            bMulti = true;

            return true;
        }

        void Update()
        {
            if (bMulti)
            {
                if (time > 2.0f)
                {
                    if (iStep < mapDic.Count)
                    {
                        SendDictionary(mapDic[iStep++]);
                    }
                    time = 0f;
                }

                time += Time.deltaTime;
            }
            else
            {
                if(PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    bMulti = true;
                }
            }
        }

        void CreateMesh()
        {
            Debug.Log("CreateMesh");
            int index = 0;
            foreach (byte[] mesh in meshDic.Values)
            {
                GameObject tempObj = Instantiate(this.originMesh, transform);
                tempObj.GetComponent<MeshFilter>().sharedMesh = MeshSerializer.ReadMesh(mesh);
                tempObj.GetComponent<MeshCollider>().sharedMesh = MeshSerializer.ReadMesh(mesh);
                tempObj.name = string.Format("Mesh {0}", index++);
            }
        }

        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }


#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif

        void CalledOnLevelWasLoaded(int level)
        {
            
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(this.meshDic);
            }
            else
            {
                // Network player, receive data
                this.meshDic = (Dictionary<short, byte[]>)stream.ReceiveNext();
                CreateMesh();
            }
        }
    }
}