using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//we need the UI for the button
using UnityEngine.UI;
//we need the ML model
using Unity.Barracuda;


public class PoseEstimation : HandToSave
{
    [SerializeField]
    protected TextMeshProUGUI m_Text; //UI text to display the prediction
    [SerializeField]
    protected Canvas m_Canvas; //UI canvas to display the prediction
    [SerializeField]
    protected NNModel m_NNModel; //the model we use to make predictions that we load from the assets folder
    [SerializeField]
    protected Model m_Model;  //the model we use to make predictions
    [SerializeField]
    protected IWorker m_Worker; //the worker we use to make predictions

    //tuple to store the prediction and the coordinates of the hand
    [SerializeField]
    protected List<(float[], float[])> predictions = new List<(float[], float[])>();

    [SerializeField]
    protected string[] gestures = new string[]{"Fist", "Five", "OK", "One", "Thumbs Down", "Thumbs Up", "Two"};
    
    [SerializeField]
    protected string last_gesture = "Fist";

    protected bool DisplayPrediction = true;

    


    /*
        initializes a scriprable object to get jont position data
        initializes the model
        initializes the worker
    */
    void Start()
    {  
        Initialize(true);
        
    }

    /*

        initializes the hand, model and worker
    */
    public void Initialize(bool displayPrediction ){

        //initialize the model
        m_Model = ModelLoader.Load(m_NNModel);

        Debug.Log("_______________________MODEL LOADED_______________________" + m_Model.ToString());

        //initialize the worker
        m_Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, m_Model);

        Debug.Log("_______________________WORKER LOADED_______________________" + m_Worker.ToString());

        DisplayPrediction = displayPrediction;
    }


     /*
       waits for the hand to be initialized
        if the hand is initialized, it outputs the data to the tensor
            the output is a prediction of the gesture
        makes sure the information canvas is always in front of the user
     */
    void Update()
    {
       if(!m_IsHandInitialized){
            construct();
       }else{
            
            //if its not null we have can update the last gesture
            string gesture = getGesture();
            if(gesture != null){
                last_gesture = gesture;
                //if we get thumbs up or down we flip the gesture
            }
       }

       //the information canvas is always in front of the user        
        m_Canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 200;
        m_Canvas.transform.rotation = Camera.main.transform.rotation;
    }


    /* 
        //////////////////////////////////////////////////////////////////////////
        //                                                                      //    
        // This function takes joint posion data, runs it through a pre-trained //
        // model, and outputs a prediction of what gesture it is                //
        ///////////////////////////////////////////////////////////////////////////

        gets hand coordinates
        creates a tensor with the coordinates
        performs inference on the tensor
        gets the output tensor
        converts the output tensor to a float array
        makes a prediction based on the output tensor values
        updates the UI text with the prediction and output tensor values
        disposes the tensors
    */

    string outputToTensor(){
        // Get the joints as a 1D array
        float[] temp_joints = GetJointsAsList();

        //cast it to a double array
        double[] joints_double = new double[temp_joints.Length];
        for(int i = 0; i < temp_joints.Length; i++){
            joints_double[i] = (double)temp_joints[i];
        }
        float[] joints = Normalize();

        // Create a tensor with the joints
        var shape = new TensorShape(1, joints.Length);
        Tensor inputTensor = new Tensor(shape, joints);

        Debug.Log("_____________INPUT TENSOR_______________" + inputTensor.ToString());
    

        // Perform inference on the input tensor
        m_Worker.Execute(inputTensor);

        // Get the output tensor from the worker
        Tensor outputTensor = m_Worker.PeekOutput();

        Debug.Log("_____________OUTPUT TENSOR_______________________" + outputTensor.ToString());

        // Convert the output tensor to a float array
        float[] outputData = outputTensor.ToReadOnlyArray();

        string output = "";
        for(int i = 0; i < outputData.Length; i++){
            output += outputData[i] + " ";
        }
        Debug.Log("________________OUTPUT DATA_____________" + output);

        // Make a prediction based on the output tensor values
        double max = -1;
        int prediction = -1;
        for (int i = 0; i < outputData.Length; i++)
        {
            if (outputData[i] > max)
            {
                max = outputData[i];
                prediction = i;
            }
        }


        
        //store the prediction and the coordinates of the hand
        predictions.Add((outputData, joints));

        if(DisplayPrediction){
            // Update the UI text with the prediction and output tensor values
            m_Text.text = "Recognized : " + last_gesture + "\n";
            m_Text.text += "prediction: " + gestures[prediction] + "\n";
            m_Text.text += output;
        }
        Debug.Log("____________FRAME_PREDICTION___________" + gestures[prediction]);
        inputTensor.Dispose();
        outputTensor.Dispose();

        return gestures[prediction];
        

        
    }
    

    /*
    //////////////////////////////////////////////////////////////////
    //                                                              //
    // this function compares the last prediction to the previous   //
    // one and checks if the joints have changed more than 10%      //
    // if they have, it returns true                                //
    //////////////////////////////////////////////////////////////////

        checks if the gesture has changed
        if the last prediction is equal to the previous one, return false
        otherwise lets see how much the joints have changed
        if the joints have changed more than 10%, return true
    */
    protected enum changed{
        sme_as_last,
        changed,
        throw_away
    }

    protected changed has_gesture_changed(){
        //if the last prediction is equal to the previous one, return false
        if(predictions.Count > 1){
            if(predictions[predictions.Count - 1].Item1 == predictions[predictions.Count - 2].Item1){
                return changed.sme_as_last;

            }
            //otherwise lets see how much the joints have changed
            else{
                float[] last_joints = predictions[predictions.Count - 1].Item2;
                float[] previous_joints = predictions[predictions.Count - 2].Item2;

                //if the joints have changed more than 10%, return true
                for(int i = 0; i < last_joints.Length; i++){
                    if(Mathf.Abs(last_joints[i] - previous_joints[i]) > 0.05f){
                        return changed.changed;
                    }
                }
            }
        }
        return changed.throw_away;
    }


    /*
    //////////////////////////////////////////////////////////////////
    //                                                              //
    // This function calls the outputToTensor function to generate  //
    // a prediction. Using the current predictions and the previous //
    // ones parses the gsutures and detrmines if a conclussion has  //
    // been reached. If so, it returns the gesture.                 //
    // this function is called every frame but mostly returns null  //
    // should be used to call the future API so we diont have a call//
    // every frame                                                  //
    //////////////////////////////////////////////////////////////////
    */
    protected string getGesture(){

        //if the hand is not being tracked, return null
        if(IsHandTracked()){

            //lets make a prediction on the current hand
            string gesture = outputToTensor();

            //we want at least 10 predictions
            if(predictions.Count > 10){
                Debug.Log("_______________________TRYING TO RECOGNIZE GESTURE_______________________" + gesture);
                //well we can assume the user might be trying to make a gesture
                //lets calculate how many times the gesture has been made
                float[] gesture_count = new float[gestures.Length];
                for(int i = 0; i < predictions.Count; i++){
                    for(int j = 0; j < predictions[i].Item1.Length; j++){
                    gesture_count[j] += predictions[i].Item1[j];
                    }
                }

                //lets find the gesture with the most predictions
                float max = -1;
                int max_index = -1;
                for(int i = 0; i < gesture_count.Length; i++){
                    if(gesture_count[i] > max){
                        max = gesture_count[i];
                        max_index = i;
                    }
                }
                Debug.Log("_______________________TIMES RCOGNIZED_______________________" + max);

                //lets check if the gesture has been made more than 50% of the time
                if(max > predictions.Count * 0.8f){
                    //lets empty the predictions
                    predictions.Clear();
                    Debug.Log("_____________________________________________FINAL_PREDICTION_______________________________________" + gestures[max_index]);
                    //lets return the gesture
                    return gestures[max_index];
                }else{
                    //lets empty the predictions
                    predictions.Clear();
                    Debug.Log("_______________________COULD NOT MAKE A FINAL PREDICTION_______________________");
                }
            }
        }
        return null;
    }

    
    
}
