from socket import *
import _thread as thread
import threading

BUFF = 1024
HOST = '172.24.8.157'
PORT = 3000

class ClientConn:
    def __init__(self, name, clientsock):
        self.name = name
        self.sock = clientsock
        self.lock = threading.Lock()

    def write(self, msg):
        self.lock.acquire()
        self.sock.send(bytes(msg, 'UTF8'))
        self.lock.release()

    def read(self):
        return self.sock.recv(BUFF).decode('UTF8')

    def close(self):
        self.sock.close()

def clientHandler(cc, comm):
    try:
        while True:
            data = cc.read()
            #print('data:' + repr(data))
            if not data: break
            comm.broadcast(data)
        cc.close()
    except Exception as e:
        print(e)

class Communicator:
    def __init__(self):
        self.lock = threading.Lock()
        self.clients = []

    def addClient(self, clientsock):
        self.broadcast("sendpos\r\n")
        self.lock.acquire()
        client = ClientConn(len(self.clients), clientsock)
        client.write(str(client.name) + "\r\n")
        self.clients.append(client)
        self.lock.release()
        thread.start_new_thread(clientHandler, (client, self))

    def broadcast(self, msg):
        self.lock.acquire()
        for c in self.clients:
            c.write(msg)
        self.lock.release()

if __name__=='__main__':
    ADDR = (HOST, PORT)
    serversock = socket(AF_INET, SOCK_STREAM)
    serversock.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
    serversock.bind(ADDR)
    serversock.listen(5)
    comm = Communicator()
    while True:
        print('waiting for connection...')
        clientsock, addr = serversock.accept()
        print('...connected from:', addr)
        comm.addClient(clientsock)
    serversock.close()
