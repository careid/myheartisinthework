from socket import *
import thread
import threading
 
BUFF = 1024
HOST = '127.0.0.1'
PORT = 1020

class ClientConn:
    def __init__(self, clientsock):
        self.sock = clientsock
        self.lock = threading.Lock()

    def write(self, msg):
        self.lock.acquire()
        self.sock.send(msg)
        self.lock.release()

    def read(self):
        return self.sock.recv(BUFF)

    def close(self):
        self.sock.close()

def clientHandler(cc, comm):
    try:
        while True:
            data = cc.read()
            print 'data:' + repr(data)
            if not data: break
            comm.broadcast("okay\r\n")
        cc.close()
    except Exception as e:
        print e

class Communicator:
    def __init__(self):
        self.lock = threading.Lock()
        self.clients = []

    def addClient(self, client):
        self.lock.acquire()
        self.clients.append(client)
        self.lock.release()
        thread.start_new_thread(clientHandler, (cc, self))

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
        print 'waiting for connection...'
        clientsock, addr = serversock.accept()
        print '...connected from:', addr
        cc = ClientConn(clientsock)
        comm.addClient(cc)
    serversock.close()
