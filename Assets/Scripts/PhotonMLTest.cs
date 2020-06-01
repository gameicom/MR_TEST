using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR || PLATFORM_LUMIN

using UnityEngine.Experimental.XR;
using UnityEngine.XR.MagicLeap;

#endif

public class PhotonMLTest : MonoBehaviour
{
    [SerializeField]
    GameObject tOriginalMesh;

    [SerializeField]
    GameObject tParent;

    GameObject SenderObj;

    MLSpatialMapper tMapper = null;

    float time = 0.0f;

    int iStep = 0;

    List<GameObject> lstCopyObject = new List<GameObject>();
    List<Mesh> lstCopyMesh = new List<Mesh>();

    Dictionary<string, byte[]> meshDic = new Dictionary<string, byte[]>();

    // Start is called before the first frame update
    void Start()
    {

    }

    void CheckMesh()
    {
        if (tMapper == null && GameObject.FindObjectOfType<MLSpatialMapper>() != null)
        {
            tMapper = GameObject.FindObjectOfType<MLSpatialMapper>();
            Debug.LogError("init");
        }
        else
        {
            CopyMesh();
        }
    }

    void CopyMesh()
    {
        if (tMapper != null)
        {
            int index = 0;
            foreach (GameObject obj in tMapper.meshIdToGameObjectMap.Values)
            {
                Mesh tMesh = obj.GetComponent<MeshFilter>().sharedMesh;

                //GameObject tempObj = PhotonNetwork.Instantiate(this.tOriginalMesh.name, Vector3.zero, Quaternion.identity, 0);
                //lstCopyObject.Add(tempObj);
                //lstCopyMesh.Add(tMesh);

                meshDic.Add(tMesh.name, MeshSerializer.WriteMesh(tMesh, false));
                Destroy(obj);

                index++;
                if (index > 250) break;
            }

            Debug.LogError("create");
            iStep = 1;
            tMapper.transform.parent.gameObject.SetActive(false);

            if (SenderObj == null)
            {
                SenderObj = PhotonNetwork.Instantiate(this.tParent.name, Vector3.zero, Quaternion.identity, 0);
            }
        }
    }

    void SendMesh()
    {
        for(int i=0; i<lstCopyObject.Count; i++)
        {
            lstCopyObject[i].SendMessage("SetMesh", lstCopyMesh[i]);
            lstCopyObject[i].SendMessage("SetStep", iStep);
        }
        Debug.LogError("send");
        iStep++;
    }

    void SendDictionary()
    {
        SenderObj.SendMessage("SetDictionary", meshDic);
        iStep++;
    }

    // Update is called once per frame
    void Update()
    {
        if (time > 10.0f)
        {
            switch (iStep)
            {
                case 0:
                    CheckMesh();
                    break;
                case 1: case 2: case 3:
                    SendDictionary();
                    //SendMesh();
                    break;
                default:
                    break;
            }
            time = 0f;
        }

        time += Time.deltaTime;
    }
}

