using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


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

        public Dictionary<string, byte[]> meshDic = new Dictionary<string, byte[]>();

        bool bCreate = false;

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

        void SetDictionary(Dictionary<string, byte[]> dic)
        {
            Debug.LogError("SetDictionary");
            meshDic = dic;
            CreateMesh();
        }

        void CreateMesh()
        {
            if (bCreate)
                return;

            Debug.LogError("CreateMesh");
            foreach (byte[] mesh in meshDic.Values)
            {
                GameObject tempObj = Instantiate(this.originMesh, transform);
                tempObj.GetComponent<MeshFilter>().sharedMesh = MeshSerializer.ReadMesh(mesh);
            }

            bCreate = true;
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
                this.meshDic = (Dictionary<string, byte[]>)stream.ReceiveNext();
                CreateMesh();
            }
        }
    }
}