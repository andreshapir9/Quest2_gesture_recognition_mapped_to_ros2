using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//we need the UI for the button
using UnityEngine.UI;

public class Create_training_data_for_python : ConnectToPython
{
    [SerializeField]
    private HandToSave handToSave;
    [SerializeField]
    private List<string> handToSaveList;
    [SerializeField]
    public TextMeshProUGUI m_Text;
    [SerializeField]
    public Canvas m_Canvas;


     private bool m_IsGestureBeingRecorded = false;


    // Start is called before the first frame update
    void Start()
    {   
        Debug.Log("_______________________LETS TEST THE SCRIPTABLE OBJECT_______________________");

        //initialize the hand we use to record the gestures
        handToSave = new HandToSave();
       //initialize the list of gestures to be recorded
        handToSaveList = new List<string>();
        handToSaveList.Add("Thumbs_up");
        handToSaveList.Add("Thumbs_down");
        // handToSaveList.Add("Thumbs_side");
        handToSaveList.Add("One");
        handToSaveList.Add("Two");
        // handToSaveList.Add("Three");
        // handToSaveList.Add("Four");
        handToSaveList.Add("Five");
        handToSaveList.Add("OK");
        // handToSaveList.Add("Rock_n_roll");
        handToSaveList.Add("Fist");
        // handToSaveList.Add("Zero");
        // handToSaveList.Add("Finger_gun");
        // handToSaveList.Add("Pinkie");
        // handToSaveList.Add("Phone");
        // handToSaveList.Add("Italian");
        // handToSaveList.Add("Middle_finger");
        // handToSaveList.Add("Crossed_fingers");
        // handToSaveList.Add("Finger_Gun_include_middle_finger");
        // handToSaveList.Add("7_in_sign_language");
        // handToSaveList.Add("8_in_sign_language");


        Debug.Log("_______________________GESTURE LIST LOADED_______________________" + handToSaveList.ToString());
        DisplayRemainingGestures();

        ConnectToServer();
    }


     // Update is called once per frame
    void Update()
    {
       if(!handToSave.m_IsHandInitialized){
            handToSave.construct();
       }else{
            // // handToSave.dump();
            if(handToSaveList.Count > 0 && !m_IsGestureBeingRecorded){
                
                    //if we are pinching the index and thumb, start recording the gesture
                    if(handToSave.IsIndexThumbPressed()){
                        m_IsGestureBeingRecorded = true;
                        StartCoroutine(GestureRecognizer());
                    }
            }
       }

       //the information canvas is always in front of the user        
        m_Canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 200;
        m_Canvas.transform.rotation = Camera.main.transform.rotation;
    }

     //displays the remaining gestures to be recorded
    public void DisplayRemainingGestures(){
        Debug.Log("_______________________DISPLAYING REMAINING GESTURES_______________________");
       string remainingGestures = "press index and thumb to start recording : \n";
       int count = 0;
         foreach(string gesture in handToSaveList){
            count++;
            remainingGestures += count + ". " + gesture + "\n";
         }
        m_Text.text = remainingGestures;
    }


     /*
    * displays a counter to let the user know when the gesture will be recognized
    */
    private IEnumerator GestureRecognizer(){

        //the user gets a 5 second timer before the gesture is recorded
        string text = m_Text.text + "\n Plase hold texture up, Starting in: X";
        for (int i = 3; i >= 0; i--)
        {
            //replace the last character of the string with the current i
            text = text.Remove(text.Length - 1) + i;
            yield return new WaitForSecondsRealtime(1);
            m_Text.text = text;

        }

        
        
        //upload the gesture to firebase
        StartCoroutine(SaveGestures());


       
    }

    /*
        saves a given amount of gestures to a given folder in firebase storage
    */
    private IEnumerator SaveGestures(){

        DisplayRemainingGestures();

        m_Text.text = m_Text.text + "\n Recording gesture... x";


        for(int i = 0; i < 1000; i++){

            string data = handToSave.SaveGestureToJson();
            
            //upload the gesture to firebase
            string utcTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-fff");
            string data_to_send = handToSaveList[0] + "/" + utcTime + ".json"+ "," + data;
            if(SendAndReceiveData(data_to_send)){
                Debug.Log("_______________________GESTURE SAVED_______________________");
            }else{
                Debug.Log("_______________________GESTURE NOT SAVED_______________________");
            }
            m_Text.text = m_Text.text.Remove(m_Text.text.Length - (i.ToString().Length )) + i;
            yield return new WaitForSeconds(0.001f);
        }

        //remoove the recrded gesture from the list
        handToSaveList.RemoveAt(0);

        DisplayRemainingGestures();
        m_IsGestureBeingRecorded = false;
    }
}
