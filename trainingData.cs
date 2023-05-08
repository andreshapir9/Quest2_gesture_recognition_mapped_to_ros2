using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class trainingData : MonoBehaviour
{
    [SerializeField]
    public List<trainingDataJoint> jointList = new List<trainingDataJoint>();

    public trainingData(string[] jointNames, Vector3[] jointPositions){
        for(int i = 0; i < jointNames.Length; i++){
            jointList.Add(new trainingDataJoint(jointNames[i], jointPositions[i]));
        }
    }

    public string ToJson(){
        string json = "{\n";
        for(int i = 0; i < jointList.Count; i++){
            json += "\t\"" + jointList[i].jointName + "\": {\n";
            json += "\t\t\"x\": " + jointList[i].x + ",\n";
            json += "\t\t\"y\": " + jointList[i].y + ",\n";
            json += "\t\t\"z\": " + jointList[i].z + "\n";
            json += "\t}";
            if(i < jointList.Count - 1){
                json += ",\n";
            }else{
                json += "\n";
            }
        }
        json += "}";
        return json;
    }
}

[System.Serializable]
public class trainingDataJoint : MonoBehaviour
{
    [SerializeField]
    public string jointName;
    [SerializeField]
    public float x, y, z;

    public trainingDataJoint(string jointName, Vector3 jointPosition){
        this.jointName = jointName;
        this.x = jointPosition.x;
        this.y = jointPosition.y;
        this.z = jointPosition.z;
    }
}