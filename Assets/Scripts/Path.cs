using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Path : MonoBehaviour
{
    public List<Multiway> path;

    /// <summary>
    /// Get next point for movement from path
    /// </summary>
    /// <param name="pathID"></param>
    /// <param name="wayID"></param>
    /// <param name="pointID"></param>
    /// <returns>next point, path id, way id, point id, was path ended</returns>
    public (Vector3, int, int, int, bool) GetNextPoint(int pathID, int wayID, int pointID)
    {
        int lastPathID = path.Count - 1;
        int lastWayID = path[pathID].ways.Count - 1;
        int lastPointID = path[pathID].ways[wayID].points.Count - 1;
        bool needNewPath = false;

        Vector3 result = path[lastPathID].ways[lastWayID].points[lastPointID].position;

        pointID += 1;
        if(pointID < path[pathID].ways[wayID].points.Count)
        {
            result = path[pathID].ways[wayID].points[pointID].position;
        } else
        {
            pathID += 1;
            if (pathID < path.Count)
            {
                wayID = Random.Range(0, path[pathID].ways.Count);
                pointID = 0;
                result = path[pathID].ways[wayID].points[pointID].position;
            } else
            {
                pathID = lastPathID;
                wayID = lastWayID;
                pointID = lastPointID;
                needNewPath = true;
            }
        }

        return (result, pathID, wayID, pointID,needNewPath);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Draw();
    }

    private void Draw()
    {


        for (int i = 0; i < path.Count; i++)
        {
            for (int j = 0; j < path[i].ways.Count; j++)
            {
                for (int k = 0; k < path[i].ways[j].points.Count - 1; k++)
                {
                    Color segmentCol = Color.green;

                    Handles.color = Color.green;

                    Handles.DrawLine(path[i].ways[j].points[k].position, path[i].ways[j].points[k + 1].position);

                    //Vector3 c1 = Vector3.Lerp(path[i].ways[j].points[k].position, path[i].ways[j].points[k + 1].position, 0.01f);
                    //Vector3 c2 = Vector3.Lerp(path[i].ways[j].points[k].position, path[i].ways[j].points[k + 1].position, 0.99f);
                    //Handles.DrawBezier(path[i].ways[j].points[k].position, path[i].ways[j].points[k + 1].position, c1, c2, segmentCol, null, 4);

                    if (i > 0 && k == 0)
                    {
                        for (int m = 0; m < path[i - 1].ways.Count; m++)
                        {
                            Handles.DrawLine(path[i - 1].ways[m].points[path[i - 1].ways[m].points.Count - 1].position, path[i].ways[j].points[k].position);

                            //c1 = Vector3.Lerp(path[i - 1].ways[m].points[path[i - 1].ways[m].points.Count - 1].position, path[i].ways[j].points[k].position, 0.01f);
                            //c2 = Vector3.Lerp(path[i - 1].ways[m].points[path[i - 1].ways[m].points.Count - 1].position, path[i].ways[j].points[k].position, 0.99f);
                            //Handles.DrawBezier(path[i - 1].ways[m].points[path[i - 1].ways[m].points.Count - 1].position, path[i].ways[j].points[k].position, c1, c2, segmentCol, null, 4);
                        }
                    }

                }
            }
        }
    }
#endif
}

[System.Serializable]
public class Way
{
    public List<Transform> points;
}

[System.Serializable]
public class Multiway
{
    public List<Way> ways;
}
