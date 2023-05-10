Capstone Project, sponsored by EmCraft.

Required Hardwear
1. Meta Quest 2
2. NXP NAVQ+
3. IRobot create3

Required Software:
1. Unity 2021.2+
2. <a href='https://docs.unity3d.com/Packages/com.unity.xr.hands@1.1/manual/index.html'>HandXR</a>
3. <a href='https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-7.0'>Net Sockets</a>
4. <a href='https://numerics.mathdotnet.com/'>Math.NET</a>
5. <a href='https://github.com/Unity-Technologies/barracuda-release'>Unity Barracuda</a>
6. <a href='https://www.tensorflow.org/api_docs/python/tf/keras'>Tensorflow</a>
7. <a href='https://numpy.org/'>Numpy</a>
8. <a href='https://github.com/onnx/tensorflow-onnx'>tf2onnx</a>
9. some other configuration might be needed both in Unity and Python

## 1. Introduction
This project is a capstone project sponsored by EmCraft. The goal of this project is to create a VR application that can control a robot a create3 robot in real-time. 

## 2. Unity Configuration
The code is meant to run the standard HandXR HandVisualizer scene with a few modifications: A canvas and TextMeshPro to display the current current predictions

The scripts can be added to the hand Visualizer component, which is responsible for rendering the hand model. 

## 3. Data Collection
Data collection is done by the Create_training_data_for_python.cs, which uses the HandToSave.cs script to get data from the predicted joints the Create_training_data_for_python script displays a list of gestures to be performed, a user pinches to start reording and a timer is started. The user should use the timer to get ready by holding up the gesture. A TCP client will then communicate to the python server, it will send a given number of gestures as JSON files, the cycle continues until all the gestures are recorded.

simpleServer.py is the server that receives the data and stores it in pre-labelled folders.

## 4. Training
In order to train the model, the data needs to be converted to a format that can be used by the model. This is done by the create_training_data.py script. It marshalls the JSON data into Numpy arrays based on their pre-labelled folders. In order to achieve satisfactory results, the data is normalized using Principal Component Analysis (PCA). each gesture will have its own .npy file.

The model loads all training data, concatenates them, shuffles them and splits it into a 80-20 training-validation split. The model is trained using the Adam optimizer and the categorical cross-entropy loss function. The model is trained for 20 epochs or until the validation loss increases. The model is then saved in the saved_models format.

using the tf2onnx package, the model is converted to an ONNX format, which then be loaded into Unity using the Barracuda package.

## 5. Unity Prediction

The Unity prediction is done by the PoseEstimation.cs which extends from the HandToSave.cs script. The script is responsible for getting the hand joints from the HandXR package, normalizes them and uses the Barracuda package to make a prediction. The script uses NNModel to load the model and the IWorker to make the prediction. The prediction is then displayed on the canvas.

## 6. Parsing the prediction

To parse predictions the PoseParser.cs script is used. It extends the PoseEstimation class, and uses the fist gesture as a 'NULL' key in order to avoid false positives. then the PoseSender.cs script is used to send the prediction to the robot. The script uses the Socket class to send the data to the robot. 

## 7. Robot Control

To communicate with the robot, the Navq+ must first be configured to connect to the same network as the headset running the Unity application. The board must also be connected to the robot via the USB port. Using the PoseSender.cs script, the robot can be controlled by sending the prediction to the robot using a TCP connection. Using the ros2_teleop_as_server.py script, a modified <a herf='https://github.com/ros2/teleop_twist_keyboard'>teleop_twist_keyboard</a> script, a modified ROS2 package can be created. The script maps the prediction to a given key, which is then sent to the robot. The robot will then move according to the key pressed.
