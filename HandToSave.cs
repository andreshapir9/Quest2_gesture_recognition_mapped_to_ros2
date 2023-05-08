using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;
// to save the hand permenantly
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//we need to use Namespace UnityEngine.XR.Hands
using UnityEngine.XR.Hands;
// tracking so we can see if a user is pinching or not
using UnityEngine.XR.Hands.OpenXR;
//for converting the hand to json
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using System;
using System.Linq;

// Your code he

// [CreateAssetMenu(fileName = "HandToSave", menuName = "HandToSave", order = 1)]
public class HandToSave : MonoBehaviour
{
    [SerializeField]
    public bool m_HandIsLeft;
    [SerializeField]
    public GameObject m_HandRoot;
    [SerializeField]
    public Transform[] m_JointXforms;
    [SerializeField]
    public string[] m_JointNames;
    [SerializeField]
    public Transform m_WristRootXform;
    [SerializeField]
    public MetaAimHand m_MetaAimHand;
    [SerializeField]
    public XRHandSubsystem handSubsystem;
    [SerializeField]
    public XRHand m_HandUpdateCallback;
    public float m_Threshold = 0.1f;
    public bool m_IsHandInitialized = false;


    void OnEnable()
    {
        Debug.Log("_______________________STARTING THE SCRIPTABLE OBJECT_______________________");
        m_HandIsLeft = false;
        m_HandRoot = null;

       //lets assign the wrist joint transform
        m_JointNames = new string[XRHandJointID.EndMarker.ToIndex()];
        for (int jointIndex = XRHandJointID.BeginMarker.ToIndex(); jointIndex < XRHandJointID.EndMarker.ToIndex(); ++jointIndex)
            m_JointNames[jointIndex] = XRHandJointIDUtility.FromIndex(jointIndex).ToString();
        
        Debug.Log("_______________________WE ASSIGNED THE JOINT NAMES_______________________" + m_JointNames[0]);

        //declare and assign the joint transforms
        m_JointXforms = new Transform[XRHandJointID.EndMarker.ToIndex()];    


        m_MetaAimHand =  (!m_HandIsLeft ? MetaAimHand.right : MetaAimHand.left);

        m_WristRootXform = null;

        //lets get the hand subsystem
        handSubsystem = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRHandSubsystem>();

        //lets get the hand update callback
        handSubsystem.trackingLost += OnTrackingLost;

        handSubsystem.trackingAcquired += OnTrackingFound;
    }


    /*
        updatre callback for when the hand is lost
    */
    void OnTrackingLost(XRHand hand)
    {
        if(!m_HandIsLeft)
        {
            if(hand.handedness == Handedness.Right)
            {
                m_HandUpdateCallback = hand;
            }
        }else
        {
            if(hand.handedness == Handedness.Left)
            {
                m_HandUpdateCallback = hand;
            }
        }
    }
    /*
       update callback for when the hand is found
    */
    void OnTrackingFound(XRHand hand)
    {
        if(!m_HandIsLeft)
        {
            if(hand.handedness == Handedness.Right)
            {
                m_HandUpdateCallback = hand;
            }
        }else
        {
            if(hand.handedness == Handedness.Left)
            {
                m_HandUpdateCallback = hand;
            }
        }
    }
    /*
        method to return whether the hand is currently being tracked
    */
    public bool IsHandTracked()
    {
        return m_HandUpdateCallback.isTracked;
    }


    public void construct()
    {
        m_HandRoot = FindHandRoot(m_HandIsLeft);

        if (m_HandRoot != null)
        {
            Debug.Log("_______________________WE FOUND THE HAND ROOT_______________________");

            m_IsHandInitialized = true;


            //lets assign the wrist joint transform
            m_WristRootXform = FindWrist(m_HandRoot, XRHandJointID.Wrist, m_JointNames);   

            Debug.Log("_______________________WE FOUND THE WRIST ROOT_______________________");
            AssignJoints(m_WristRootXform, m_JointXforms, m_JointNames);
        }

    }

    //using the hand and joint Id and joint names, find the appropriate joint transform
    private void AssignJoint(GameObject handRoot, XRHandJointID jointId, string[] jointNames){
        int jointIndex = jointId.ToIndex();
        m_JointXforms[jointIndex] = handRoot.transform;
        Debug.Log("_______________________WE ASSIGNED THE JOINT TRANSFORMS_______________________" + m_JointXforms[jointIndex].position);
    }

    //finds the wrist joint transform
    private Transform FindWrist(GameObject handRoot, XRHandJointID jointId, string[] jointNames){
        Transform wristRootXform = null;
        Debug.Log("_______________________WE ARE LOOKING FOR THE WRIST ROOT_______________________");
        //traverse all the children of the hand root
        for (int childIndex = 0; childIndex < m_HandRoot.transform.childCount; ++childIndex)
        {
            //if the child name ends with the wrist joint name, assign it to the wristRootXform
            var child = m_HandRoot.transform.GetChild(childIndex);
            Debug.Log("_______________________CHILD NAME_______________________" + child.gameObject.name);
            if (child.gameObject.name.EndsWith(jointNames[XRHandJointID.Wrist.ToIndex()]))
            {
                wristRootXform = child;
                Debug.Log("_______________________WE FOUND THE WRIST ROOT As a child_______________________");
                break;
            }
            //otherwise, traverse the grandchildren of the hand root at the current child index
            for (int grandchildIndex = 0; grandchildIndex < child.childCount; ++grandchildIndex)
            {
                var grandchild = child.GetChild(grandchildIndex);
                if (grandchild.gameObject.name.EndsWith(jointNames[XRHandJointID.Wrist.ToIndex()]))
                {
                    wristRootXform = grandchild;
                    Debug.Log("_______________________WE FOUND THE WRIST ROOT As a grandchild_______________________");
                    break;
                }
            }
            if (wristRootXform != null)
                break;
        }
        return wristRootXform;
    }
    // finds the hand root object
    private GameObject FindHandRoot( bool isLeft){

        //traverse the children of the xrOrigin
        GameObject handRoot = null;
        
        //while the handroot is null we loop forever
        while (handRoot == null)
        {
             //lets print all of the gameobjects in the scene and their parents
            foreach (GameObject go in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                //Debug.Log("________________HERE________ GameObject: " + go.name + " Parent: " + go.transform.parent);

                //if we find the right hand or left hand, assign it to the handRoot they will be called RightHand(Clone) or LeftHand(Clone)
                if (go.name == (isLeft ? "LeftHand(Clone)" : "RightHand(Clone)"))
                {
                    handRoot = go;
                    Debug.Log("________________FOUND HAND ROOT________ GameObject: " + go.name + " Parent: " + go.transform.parent);
                    return handRoot;
                }
            }
        }

        //before we return, lets assign add an XR D
        return handRoot;
    }

    //using the wrist joint transform, assign the rest of the joints using the AssignJoint function
    private void AssignJoints(Transform wristRootXform, Transform[] jointXforms, string[] jointNames){
       
        //traverse the children of the wrist root
        for (int childIndex = 0; childIndex < wristRootXform.childCount; ++childIndex)
        {
            var child = wristRootXform.GetChild(childIndex);
            //if the child is a palm joint, assign it to the palm joint and continue
            if (child.name.EndsWith(jointNames[XRHandJointID.Palm.ToIndex()]))
            {
                AssignJoint(child.gameObject, XRHandJointID.Palm, jointNames);
                Debug.Log("________________ ASSIGNING PALM______________: " + child.name);
                continue;
            }

            //otherwise we traverse across the fingerindex joints
            for (int fingerindex =(int)XRHandFingerID.Thumb; fingerindex <= (int)XRHandFingerID.Little; ++fingerindex)
            {
                var fingerId = (XRHandFingerID)fingerindex;

                var jointIdFront = fingerId.GetFrontJointID();
                //as long as the child name does not end with the front joint name, continue
                if (!child.name.EndsWith(jointNames[jointIdFront.ToIndex()]))
                    continue;

                AssignJoint(child.gameObject, jointIdFront, jointNames);
                Debug.Log("________________ ASSIGNING FRONT______________: " + child.name);

                var lastChild = child;
                int jointIndexBack = fingerId.GetBackJointID().ToIndex();

                //traverse the children of the fingerindex joints
                for( int joointIndex = jointIdFront.ToIndex() + 1; joointIndex <= jointIndexBack; ++joointIndex)
                {
                    Transform nextChild = null;
                    
                    //traverse the children of the last child
                    for (int nextChildIndex = 0; nextChildIndex < lastChild.childCount; ++nextChildIndex)
                    {
                        nextChild = lastChild.GetChild(nextChildIndex);
                        
                        //if the child name ends with the joint name, lastChild is assigned to the nextChild and break
                        if (nextChild.name.EndsWith(jointNames[joointIndex]))
                        {
                            lastChild = nextChild;
                            break;
                        }
                    }

                    //if the last child does not end with the joint name, we have a problem
                    if (!lastChild.name.EndsWith(jointNames[joointIndex]))
                    {
                        Debug.LogError("________________ERROR________ Could not find joint " + jointNames[joointIndex]);
                        continue;
                    }

                    //assign the last child to the jointXforms
                    var jointId = XRHandJointIDUtility.FromIndex(joointIndex);
                    AssignJoint(lastChild.gameObject, jointId, jointNames);
                    Debug.Log("_______________ASSIGNING JOINT_____________: " + lastChild.name);
                }

            }
        }
    }

    //returns true if we are pressing index and thumb
    public bool IsIndexThumbPressed(){ 
        return m_MetaAimHand.indexPressed.isPressed;
    }


    /*
        takes in vector3 of joints and saves them to a json file 
        joint_name : x, y, z
    */
    public string SaveGestureToJson(){

        Debug.Log("________________SAVING GESTURE_____________: ");
        //lets record the gesture
        Vector3[] tempJoints = new Vector3[m_JointXforms.Length];
        for (int i = 0; i < m_JointXforms.Length; i++)
        {   if(m_JointXforms[i] == null){
                Debug.Log("________________SAVING GESTURE_____________: " + m_JointNames[i] + " is null");
                tempJoints[i] = new Vector3(0,0,0);
                continue;
            }
            tempJoints[i] = m_JointXforms[i].position;
            Debug.Log("________________SAVING GESTURE_____________: " + tempJoints[i]);
        }

        //lets create a dictionary of the joints and their positions
        trainingData jointDict = new trainingData(m_JointNames, tempJoints);

        //lets convert the dictionary to json
        string json = jointDict.ToJson();

        Debug.Log("________________SAVING GESTURE_____________: " + json);

        //lets convert the json to bytes
       return json;
    }


    //prints out the joints
    public void dump(){

        if(m_IsHandInitialized){
            Debug.Log("________________________________START OF DUMP________________________________");
            Debug.Log("______HAND ROOT______: " +  m_HandRoot.transform.position);
            Debug.Log("__________________________JOINT NAMES AND TRANSFORMS__________________________");
            for(int i = 0; i < m_JointNames.Length; i++){
                Debug.Log("______JOINT " + m_JointNames[i] + "______: " + m_JointXforms[i]);
            }
            Debug.Log("________________________________END OF DUMP________________________________");
        }
    }

    /*
        returns the joints as a list of doubles
    */
    public float[] GetJointsAsList(){
        Vector3[] tempJoints = new Vector3[m_JointXforms.Length];
        float sum = 0;
        for (int i = 0; i < m_JointXforms.Length; i++)
        {   if(m_JointXforms[i] == null){
                tempJoints[i] = new Vector3(0,0,0);
                continue;
            }
            tempJoints[i] = m_JointXforms[i].position;
            sum += tempJoints[i].x + tempJoints[i].y + tempJoints[i].z;
        }
        Debug.Log("________________SUM_____________: " + sum);
        float[] joints = new float[tempJoints.Length * 3];
        for(int i = 0; i < tempJoints.Length; i++){
            joints[i*3] = tempJoints[i].x;
            joints[i*3 + 1] = tempJoints[i].y;
            joints[i*3 + 2] = tempJoints[i].z;
        }
        return joints;
    }

    /*
        THIS FUNCTION WAS WRITTEN BY CHATGPT(except for the for actually making it work, and changing like 80% of the code)
        function generates a covariance matrix from the given joint positions by calculating eigenvalues and eigenvectors
        normalizes the joint positions by subtracting the centroid of the hand
        then uses the covariance matrix to find the principal components of the hand
        uses the principal components to normalize the hand into something that can compared in XYZ space against other hands
        returns a list of floats that represent the normalized hand
        USED FOR GESTURE RECOGNITION
    */

   public float[] Normalize(){
        //find the centroid of the hand
        Vector3[] tempJoints = new Vector3[m_JointXforms.Length];
        for (int i = 0; i < m_JointXforms.Length; i++)
        {   if(m_JointXforms[i] == null){
                // Debug.Log("________________SAVING GESTURE_____________: " + m_JointNames[i] + " is null");
                tempJoints[i] = new Vector3(0,0,0);
                continue;
            }
            tempJoints[i] = m_JointXforms[i].position;
            // Debug.Log("________________SAVING GESTURE_____________: " + tempJoints[i]);
        }

        Vector3 centroid = new Vector3(0,0,0);
        for(int i = 0; i < tempJoints.Length; i++){
            centroid += tempJoints[i];
        }

        centroid /= tempJoints.Length;
        Debug.Log("________________CENTROID_____________: " + centroid);

        List<Vector3> normalized_positions = new List<Vector3>();
        for(int i = 0; i < tempJoints.Length; i++){
            normalized_positions.Add(tempJoints[i] - centroid);
        }

        // Convert List<Vector3> to DenseMatrix
        var positions = DenseMatrix.OfRowVectors(normalized_positions.Select(j => Vector<double>.Build.DenseOfArray(new double[] { j.x, j.y, j.z })).ToArray());

        // calculate the covariance matrix
        var covariance = GetCovarianceMatrix(positions);

        Debug.Log("________________COV_____________: " + covariance.ToString());

        // Calculate the rotation matrix to align the hand in a standardized orientation
        var eigen = covariance.Evd();

        Debug.Log("________________EIG_____________: " + eigen.ToString());

        // get the eigenvectors as a sequence of vectors
        var eigenvectors = Enumerable.Range(0, eigen.EigenVectors.ColumnCount)
            .Select(i => Vector<double>.Build.DenseOfArray(new[] { eigen.EigenVectors[0, i], eigen.EigenVectors[1, i], eigen.EigenVectors[2, i] }));

        // sort the eigenvectors based on their corresponding eigenvalues and select the top 3
        var sortedEigenVecs = eigenvectors
            .Select((v, i) => new KeyValuePair<double, Vector<double>>(eigen.EigenValues[i].Magnitude, v))
            .OrderByDescending(kv => kv.Key)
            .Select(kv => kv.Value)
            .Take(3)
            .ToArray();
        Debug.Log("________________SORTED EIGENVECS_____________: " + sortedEigenVecs.ToString());

        //constructing the rotation matrix
        var rotation_matrix = Matrix<double>.Build.DenseOfRowVectors(sortedEigenVecs[0], sortedEigenVecs[1], sortedEigenVecs[2]);

        // Apply the rotation matrix to the hand
        for (int i = 0; i < normalized_positions.Count; i++)
        {
            var joint_coords = Vector<double>.Build.DenseOfArray(new double[] { normalized_positions[i].x, normalized_positions[i].y, normalized_positions[i].z });
            joint_coords = rotation_matrix * joint_coords;
            normalized_positions[i] = new Vector3((float)joint_coords[0], (float)joint_coords[1], (float)joint_coords[2]);
        }

        // Get the positions of the normalized hand
        var normalized_positions_float = new float[normalized_positions.Count * 3];
        for(int i = 0; i < normalized_positions.Count; i++){
            normalized_positions_float[i * 3] = normalized_positions[i].x;
            normalized_positions_float[i * 3 + 1] = normalized_positions[i].y;
            normalized_positions_float[i * 3 + 2] = normalized_positions[i].z;
        }
        return normalized_positions_float;
   }

    /*
        * This function is used to calculate the covariance matrix of a matrix
        * https://stackoverflow.com/questions/32256998/find-covariance-of-math-net-matrix
    */
    public static Matrix<double> GetCovarianceMatrix(Matrix<double> matrix)
    {
        //calculate the mean of each column
        var columnAverages = matrix.ColumnSums() / matrix.RowCount;
        
        //center the columns by subtracting the column means
        var centeredColumns = matrix.EnumerateColumns().Zip(columnAverages, (col, avg) => col - avg);

        //create a new matrix from the centered columns
        var centered = DenseMatrix.OfColumnVectors(centeredColumns);

        //calculate the covariance matrix by multiplying the transposed centered matrix by the centered matrix
        var normalizationFactor = matrix.RowCount == 1 ? 1 : matrix.RowCount - 1;
        return centered.TransposeThisAndMultiply(centered) / normalizationFactor;
    }

    
}

