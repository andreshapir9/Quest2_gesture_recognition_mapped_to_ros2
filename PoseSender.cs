using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseSender : PoseParser
{
    [SerializeField]
    protected ConnectToPython m_ConnectToPython;
    [SerializeField]
    protected string m_Gesture_to_send = "";
    [SerializeField]
    protected bool m_Should_we_send_data = false;

    // Start is called before the first frame update
    void Start()
    {
        InitializeSender(true);
    }

    public void InitializeSender(bool DisplayPrediction){
        Debug.Log("_______________________ VIRTUAL INITIALIZE_______________________");
        InitializeParser(false); //currently we don't display the prediction(DEBUG)
        m_ConnectToPython = new ConnectToPython("192.168.0.26");
        // m_ConnectToPython = new ConnectToPython("172.20.10.8");
        m_ConnectToPython.ConnectToServer();
    }


    // Update is called once per frame
    void Update(){
        if(!m_IsHandInitialized){
            construct();
        }else{
            if(IsHandTracked()){
                m_Should_we_send_data = true;
                string temp_gesture = parseGesture();
                if(temp_gesture == "Fist"){
                    m_Gesture_to_send = "NULL";
                }else if(temp_gesture != null){
                    m_Gesture_to_send = temp_gesture;
                }
                m_Text.text = m_Gesture_to_send;
            }else{
                m_Should_we_send_data = false;
                m_Text.text = "Hand not tracked";
            }

        }

        if(m_Should_we_send_data){
            m_ConnectToPython.SendData(m_Gesture_to_send);
        }


        //the information canvas is always in front of the user        
        m_Canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 200;
        m_Canvas.transform.rotation = Camera.main.transform.rotation;
    }
}
