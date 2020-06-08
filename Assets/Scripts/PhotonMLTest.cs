using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;

#if UNITY_EDITOR || PLATFORM_LUMIN

using UnityEngine.Experimental.XR;
using UnityEngine.XR.MagicLeap;

#endif

public class PhotonMLTest : MonoBehaviour
{
    [SerializeField]
    GameObject tParent;

    GameObject SenderObj;

    MLSpatialMapper tMapper = null;

    float time = -20.0f;

    int iStep = 0;

    List<GameObject> lstCopyObject = new List<GameObject>();
    List<Mesh> lstCopyMesh = new List<Mesh>();

    List<Dictionary<short, byte[]>> meshDic = new List<Dictionary<short, byte[]>>();

    // Start is called before the first frame update
    void Start()
    {

    }

    void CheckMesh()
    {
        if (tMapper == null && GameObject.FindObjectOfType<MLSpatialMapper>() != null)
        {
            tMapper = GameObject.FindObjectOfType<MLSpatialMapper>();
            Debug.LogError("Init"+ tMapper.levelOfDetail);
        }
        else
        {
            CopyMesh();
        }
    }

    //Vector3 CompareVectorPos(Vector3 a, Vector3 b, bool bMin)
    //{
    //    if (bMin)
    //    {
    //        if (a.x > b.x)
    //        {
    //            a.x = b.x;
    //        }
    //        if (a.y > b.y)
    //        {
    //            a.y = b.y;
    //        }
    //        if (a.z > b.z)
    //        {
    //            a.z = b.z;
    //        }
    //    }
    //    else
    //    {
    //        if (a.x < b.x)
    //        {
    //            a.x = b.x;
    //        }
    //        if (a.y < b.y)
    //        {
    //            a.y = b.y;
    //        }
    //        if (a.z < b.z)
    //        {
    //            a.z = b.z;
    //        }
    //    }

    //    return a;
    //}

    void CopyMesh()
    {
        if (tMapper != null)
        {
            int lstIndex = 0;
            short index = 0;

            Dictionary<short, byte[]> tempDic = new Dictionary<short, byte[]>();

            //GameObject cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //Mesh cubeMesh = cubeObj.GetComponent<MeshFilter>().sharedMesh;
            //Destroy(cubeObj);

            foreach (GameObject obj in tMapper.meshIdToGameObjectMap.Values)
            {
                Mesh tMesh = obj.GetComponent<MeshFilter>().sharedMesh;
                Destroy(obj);

                if (tMesh.vertices.Length == 0)
                    continue;

                //Vector3 min = new Vector3(999f, 999f, 999f);
                //Vector3 max = new Vector3(-999f, -999f, -999f);

                //for (int i = 0; i < tMesh.vertices.Length; i++)
                //{
                //    min = CompareVectorPos(min, tMesh.vertices[i], true);
                //    max = CompareVectorPos(max, tMesh.vertices[i], false);
                //}

                //Debug.Log(index+". Lenth " +tMesh.vertices.Length);

                //Vector3 center = tMesh.bounds.center;
                //Vector3 size = tMesh.bounds.size;
                //Vector3 min = tMesh.bounds.min;
                //Vector3 max = tMesh.bounds.max;
                //Vector3 edge1 = new Vector3(min.x,max.y,min.z);
                //Vector3 edge2 = new Vector3(max.x, min.y, max.z);

                ////obj.transform.position = center;
                ////obj.transform.localScale = size;

                ////tMesh = cubeMesh;

                //tMesh.Clear();
                //tMesh.SetVertices(new Vector3[] {min,edge1,edge2,max});
                //tMesh.SetUVs(0, new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero });
                //tMesh.SetTriangles(new int[] { 0, 1, 2, 3, 1, 2}, 0);
                //tMesh.RecalculateNormals();

                ////tMesh.Simplify(); //MeshSimplification.Simplify(tMesh);

                //Debug.LogError(index + ". Lenth " + tMesh.vertices.Length);

                if (tMesh.triangles.Length/3 > 250)
                {
                    Debug.LogError(index + ". Tris " + tMesh.triangles.Length / 3);
                }

                tempDic.Add(index, MeshSerializer.WriteMesh(tMesh, false));
                

                index++;
                if (index > 100)
                {
                    meshDic.Add(tempDic);

                    tempDic = new Dictionary<short, byte[]>();
                    lstIndex++;
                    index = 0;
                }
            }

            meshDic.Add(tempDic);

            Debug.LogError("Create");
            iStep = 1;
            tMapper.transform.parent.gameObject.SetActive(false);

            if (SenderObj == null)
            {
                SenderObj = PhotonNetwork.Instantiate(this.tParent.name, Vector3.zero, Quaternion.identity, 0);
            }
        }
    }

    void SendDictionary()
    {
        Debug.LogError("Send");
        SenderObj.SendMessage("SetDictionary", meshDic);
        iStep++;
    }

    // Update is called once per frame
    void Update()
    {
        if (iStep < 2)
        {
            if (time > 5.0f)
            {
                if (iStep == 0)
                {
                    CheckMesh();
                    time = 0f;
                }
                else if (iStep == 1)
                {
                    SendDictionary();
                }
            }

            time += Time.deltaTime;
        }
    }
}

