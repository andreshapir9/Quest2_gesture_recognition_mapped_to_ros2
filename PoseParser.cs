using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseParser : PoseEstimation
{
    [SerializeField]
    protected bool m_Has_gesture_been_reset = false;

    [SerializeField]
    protected string m_Recognized_gestures = "";
    // [SerializeField]
    // protected bool m_should_we_start_parsing = false;
    // Start is called before the first frame update
    void Start()
    {
        InitializeParser(true);
    }

    public void InitializeParser(bool DisplayPrediction){
        Debug.Log("_______________________ VIRTUAL INITIALIZE_______________________");
       Initialize(DisplayPrediction);
    }

    // Update is called once per frame
    //TODO: try using fixed update?
    void Update()
    {
        if(!m_IsHandInitialized){
            construct();
       }else{

            if(IsHandTracked()){
                string temp_gesture = parseGesture();
                string text = m_Text.text;
                if(temp_gesture != null && temp_gesture != "Fist"){
                    m_Recognized_gestures += temp_gesture + " ";
                    Debug.Log("_______________________Recognized gesurres _______________________" + m_Recognized_gestures);
                }

                if(DisplayPrediction)
                    m_Text.text = m_Recognized_gestures + "\n" + text;
                // if(IsIndexThumbPressed()){
                //    m_should_we_start_parsing = true;
                // }
            }else{
                // m_should_we_start_parsing = false;
            }
       }

       //the information canvas is always in front of the user        
        m_Canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 200;
        m_Canvas.transform.rotation = Camera.main.transform.rotation;
    }


    /*
    ///////////////////////////////////////////////
    // Fist acts a NULL character                //
    // the only way to change your character is  //
    // to return to the NULL character           //
    ///////////////////////////////////////////////
    */
    public string parseGesture(){

        //if its not null we have can update the last gesture
        string gesture = getGesture();
        if(gesture != null){
            last_gesture = gesture;
            Debug.Log("_______________________LAST GESTURE_______________________"+last_gesture);
            //if we get thumbs up or down we flip the gesture
        }
        string text = m_Text.text;
        string temp_gesture = gesture;

        if(gesture == "Fist"){
            Debug.Log("_______________________FIST_______________________");
            m_Has_gesture_been_reset = true;
            //if we have a fist we reset the gesture
            return gesture;
        }
        else if(gesture != null && m_Has_gesture_been_reset){
            m_Has_gesture_been_reset = false;
            //if we dont have a fist we return the last gesture
            Debug.Log("_______________________HERE WE CAN MAKE AN API CALL_______________________" + last_gesture);
            // string new_text = "Parsed : " + last_gesture + "\n" + text;
            // m_Text.text = new_text;
            return last_gesture;
        }

        // string new_text2 = "Parsed : NULL\n" + text;
        // m_Text.text = new_text2;

        return null;
    }

}
