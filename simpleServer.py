import socket
import os


HOST = '0.0.0.0' # Replace with the IP address of your server if it's remote
PORT = 1234

# Create a socket object
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to a specific host and port
s.bind((HOST, PORT))

# Listen for incoming connections
s.listen(1)

print('Waiting for a connection...')

# Accept a connection
conn, addr = s.accept()

print(f'Connected by {addr}')

# Receive data from the client
while True:
    data = conn.recv(1000000)
    if not data:
        break
    # Decode the received data from bytes to string and print it
    print(data.decode())
    message = data.decode()
    path, content = message.split(',', 1)
    directory, file_name = path.split('/', 1)
    #write the content to the file, create the file if it doesn't exist
    if not os.path.exists(directory):
        os.makedirs(directory)
    with open(path, 'w') as f:
        f.write(content)
        f.close()
    print(f"File {file_name} created in directory {directory}")
    #send the file byte count back to the client
    # Send a response to the with the quantity of bytes received
    conn.sendall(str(len(data)).encode())

# Close the connection
conn.close()
