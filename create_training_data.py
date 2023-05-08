import numpy as np
import os

# our data looks like this: 
# {
    # "name": {
    # 		"x": 0,
    # 		"y": 0,
    # 		"z": 0
    # 	},
    # "name": {
    # 		"x": 0,
    # 		"y": 0,
    # 		"z": 0
    # 	},
    # ...
# }
# we want to convert it to this:
# [x1,y1,z1,x2,y2,z2...xn,yn,zn]

class Joint:
    def __init__(self, name, x, y, z):
        self.name = name
        self.x = float(x)
        self.y = float(y)
        self.z = float(z)
    def get_position(self):
        #retun as double
        return [float(self.x), float(self.y), float(self.z)]
    def __str__(self):
        return self.name + " [ " + self.x + ", " + self.y + ", " + self.z + " ]"

class Hand:
    def __init__(self, joints):
        self.joints = joints
    def get_positions(self):
        positions = []
        for joint in self.joints:
            positions.extend(joint.get_position())
        return positions
    def __str__(self):
        joint_string = "[ "
        for joint in self.joints:
            joint_string += str(joint) + ", "
        joint_string += " ]"
        return joint_string
    def __repr__(self):
        return self.get_positions()
    ####################################################################################
    # THIS FUNCTION NORMALIZES THE DATA                                               #
    # SHOULD CREATE A ROTATION MATRIX TO ALIGN THE HAND IN A STANDARDIZED ORIENTATION #
    # SHOULD TRANSLATE THE HAND TO CENTER IT AT THE ORIGIN                            #
    # RAISED zvalidation accuracy from 0.73 -> 0.91                                   #
    # THIS WAS WRITTEN BY CHATGPT                                                     #
    ####################################################################################     
     # https://www.researchgate.net/post/Is-there-any-pose-normalization-method-for-human-3D-joint-positions                           
    def normalize(self):
        # Find the centroid of the hand
        centroid = np.mean([joint.get_position() for joint in self.joints], axis=0)
        
        # Translate the hand to center it at the origin
        for joint in self.joints:
            joint.x -= centroid[0]
            joint.y -= centroid[1]
            joint.z -= centroid[2]
        
        # Calculate the rotation matrix to align the hand in a standardized orientation
        cov = np.cov([joint.get_position() for joint in self.joints], rowvar=False)
        eigenvals, eigenvecs = np.linalg.eigh(cov)
        sort_indices = np.argsort(eigenvals)[::-1]
        eigenvecs = eigenvecs[:, sort_indices]
        x_axis = eigenvecs[:, 0]
        y_axis = eigenvecs[:, 1]
        z_axis = eigenvecs[:, 2]
        rotation_matrix = np.vstack((x_axis, y_axis, z_axis)).T
        
        # Apply the rotation matrix to the hand
        for joint in self.joints:
            joint_coords = np.array([joint.x, joint.y, joint.z])
            joint_coords = np.dot(rotation_matrix, joint_coords)
            joint.x = joint_coords[0]
            joint.y = joint_coords[1]
            joint.z = joint_coords[2]
        
        # Get the positions of the normalized hand
        positions = self.get_positions()
        
        return positions
    

#takes in string data and returns a list of joints
def serialize_data(data):
    #lets remove all whitespace
    hand = []
    countLines = 0
    countValues = 0
    joints = data.split("},")
    for i in range(len(joints)):
        try:
            name = joints[i].split(":")[0].replace("{", "").replace('"', "").replace("\n", "").replace("\t", "")
            x = joints[i].split(",")[0].split(":")[2].replace("\n", "").replace("\t", "").replace("   ", "")
            y = joints[i].split(",")[1].split(":")[1].replace("\n", "").replace("\t", "")
            z = joints[i].split(",")[2].split(":")[1].replace("}", "").replace("\n", "").replace("\t", "")
            hand.append(Joint(name, x, y, z))
            countLines += 1
            countValues += 3
        except:
            pass

    # print("countLines: ", countLines)
    # print("countValues: ", countValues)
    hand = Hand(hand)
    hand.normalize()
    return hand
        
import os
import numpy as np

def read_data(directory_name):
    #opens a directory and gets a list of all the files in it
    #make sure it is a directory and not a file
    if not os.path.isdir(directory_name):
        print("Error: ", directory_name, " is not a directory")
        return []
    files = os.listdir(directory_name)
    gestures = []
    #opens a file and gets a string of all the lines in it
    for file in files:
        #if the file is not a json file, we skip it
        if file[-5:] != ".json":
            continue
        #we open each file in the directory
        f = open(os.path.join(directory_name, file), "r")
        file_data = f.readlines()
        file_as_string = ""
        line_count = 0
        for line in file_data:
            file_as_string += line
            line_count += 1
        f.close()
        #if the file does not start and end with a curly brace, we know it is not a valid json file
        if file_as_string[0] == "{" and file_as_string[-1] == "}":
            #there should be 132 lines in the file
            if line_count == 132:
                 gestures.append(serialize_data(file_as_string))

       
    return gestures
        
#save data as a numpy array
def save_data(data, file_name):
    data_as_double = []
    for gesture in data:
        gesture_as_double = []
        for joint in gesture.joints:
            gesture_as_double.append(float(joint.x))
            gesture_as_double.append(float(joint.y))
            gesture_as_double.append(float(joint.z))
        data_as_double.append(gesture_as_double)
    np.save(file_name, data_as_double)

#what directory are we in
print("Current directory: ", os.getcwd())
main_dir = input("Enter the directory name: ")
#lets open the directory and get a list of all directories in it
directories = os.listdir(main_dir)
for directory in directories:
    save_dir = os.path.join(main_dir, directory + ".npy")
    print("save_dir: ", save_dir)
    data = read_data(main_dir + "/" + directory)
    if len(data) > 0:
        save_data(data, save_dir)
        
        data = np.load(save_dir)
        data = data.astype(np.float32)
        print("directory: ", directory)
        print("data: ", data)
        print("data shape: ", data.shape)

   
# def read_data():
#     #opens a directory and gets a list of all the files in it
#     files = os.listdir("data/Validation_data/ok")
#     gestures = []
#     #opens a file and gets a string of all the lines in it
#     for file in files:
        
#         #we open each file in the directory
#         f = open("data/Validation_data/ok/" + file, "r")
#         file_data = f.readlines()
#         file_as_string = ""
#         line_count = 0
#         for line in file_data:
#             file_as_string += line
#             line_count += 1
#         f.close()
#         #if the file does not start and end with a curly brace, we know it is not a valid json file
#         if file_as_string[0] == "{" and file_as_string[-1] == "}":
#             #there should be 132 lines in the file
#             if line_count == 132:
#                  gestures.append(serialize_data(file_as_string))

       
#     return gestures
        
# #save data as a numpy array
# def save_data(data):
#     data_as_double = []
#     for gesture in data:
#         gesture_as_double = []
#         for joint in gesture.joints:
#             gesture_as_double.append(float(joint.x))
#             gesture_as_double.append(float(joint.y))
#             gesture_as_double.append(float(joint.z))
#         data_as_double.append(gesture_as_double)
#     np.save("data/Validation_data/ok.npy", data_as_double)

# # print(read_data())
# save_data(read_data())

# #lets load the data and see what it looks like
# data = np.load("data/Validation_data/ok.npy")
# data = data.astype(np.float32)
# np.save("data/Validation_data/ok.npy", data)
# print("data: ", data)
# print("data shape: ", data.shape)