import numpy as np
import os
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
# import tf2onnx #https://github.com/onnx/tensorflow-onnx
# import onnx
from sklearn.utils import shuffle
from sklearn.metrics import confusion_matrix
# import onnxmltools
# from onnx import numpy_helper
# import tempfile






print("____________________LOADING DATA____________________")

data_dir = "new_data/Training_data"

files = os.listdir(data_dir)
#lets remove everything that is not a numpy file
files = [file for file in files if file.endswith(".npy")]
#lets sort the files so that they are in the same order every time
files.sort()

data = []
#lets load all the data
for file in files:
    data.append(np.load(os.path.join(data_dir, file)))
    data[-1] = data[-1].astype(np.float32)
    print("Loaded: ", file)
    print("Shape: ", data[-1].shape)
#we should now have a list of numpy arrays
#lets concatenate them into one big array
x_complete_data = np.concatenate(data)
y_complete_data = np.concatenate([np.full(len(data[i]), i) for i in range(len(data))])
y_complete_data = keras.utils.to_categorical(y_complete_data, num_classes=len(data))


#shuffle the data so that the model doesn't learn the order lets set a seed
x_complete_data, y_complete_data = shuffle(x_complete_data, y_complete_data, random_state=9)

#lets split the data into training and validation data
split_index = int(len(x_complete_data) * 0.8)
train_x = x_complete_data[:split_index]
test_x = x_complete_data[split_index:]

train_y = y_complete_data[:split_index]
test_y = y_complete_data[split_index:]

print("train_x ", train_x)
print("train_x shape: ", train_x.shape)
print("train_y ", train_y)
print("train_y shape: ", train_y.shape)
print("test_x shape: ", test_x.shape)
print("test_y shape: ", test_y.shape)


#TDOO: crossvalidation
print("____________________TRAINING____________________")

def create_model():
    # Create the model
    model = keras.Sequential(name="RockPaperScissors")
    model.add(layers.Dense(128, activation='relu', input_shape=(78,)))
    model.add(layers.Dense(512, activation='tanh'))
    model.add(layers.Dense(512, activation='relu'))
    model.add(layers.Dense(256, activation='tanh'))
    model.add(layers.Dense(len(data), activation='softmax'))


    model.compile(optimizer='adam',loss='categorical_crossentropy',metrics=['accuracy'])

    return model


model = create_model()

#lets define a callback to stop training when we start overfitting
early_stop = keras.callbacks.EarlyStopping(
        monitor='loss', mode='min', verbose=1, patience=2)
    
    
# model.fit(train_x, train_y, epochs=10, validation_split=0.2)
model.fit(train_x, train_y, epochs=20, callbacks=[early_stop])


loss, accuracy = model.evaluate(test_x, test_y)
print("Accuracy: ", accuracy)
print("Loss: ", loss)


y_pred = model.predict(test_x)

y_pred = np.argmax(y_pred, axis=1)

y_true = np.argmax(test_y, axis=1)

cm = confusion_matrix(y_true, y_pred)
print("Confusion Matrix:")
print(cm)





print("____________________TestSet____________________")

# #lets check against another dataset
data_dir = "new_data/Validation_data"

files = os.listdir(data_dir)
#lets remove everything that is not a numpy file
files = [file for file in files if file.endswith(".npy")]
#lets sort the files so that they are in the same order every time
files.sort()
data = []
#lets load all the data
for file in files:
    data.append(np.load(os.path.join(data_dir, file)))
    data[-1] = data[-1].astype(np.float32)
    print("Loaded: ", file)
    print("Shape: ", data[-1].shape)
#we should now have a list of numpy arrays
#lets concatenate them into one big array
x_validation = np.concatenate(data)
y_validation = np.concatenate([np.full(len(data[i]), i) for i in range(len(data))])
y_validation = keras.utils.to_categorical(y_validation, num_classes=len(data))

print("x_validation shape: ", x_validation.shape)
print("y_validation shape: ", y_validation.shape)
print("x_validation ", x_validation)
print("y_validation ", y_validation)



#lets see how many we got right
print("____________________RESULTS____________________")

#testing on the original model
print("Testing on the original model")
# model = keras.models.load_model("models/rock_paper_scissors.h5")
loss, accuracy = model.evaluate(x_validation, y_validation)

print("Accuracy: ", accuracy)
print("Loss: ", loss)

#lets print out a confusion matrix

y_pred = model.predict(x_validation)

y_pred = np.argmax(y_pred, axis=1)

y_true = np.argmax(y_validation, axis=1)

cm = confusion_matrix(y_true, y_pred)
print("files: ", files)
print("Confusion Matrix:")
print(cm)


print("____________________EXPORTING____________________")

# # Save the entire model as a SavedModel.
# model.save('models/rock_paper_scissors')

#save the model as a saved model format
model.save('models/rock_paper_scissors')

#to convert the model to onnx
# go to /test_tf2onnx/tensorflow-onnx
#python3.10 -m tf2onnx.convert --saved-model ../../models/rock_paper_scissors --output model.onnx


