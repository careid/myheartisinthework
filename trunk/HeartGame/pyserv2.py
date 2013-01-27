from socket import *
import threading
try:
    # Python 3.x
    import queue
    decode = lambda s: s.decode('utf8')
except ImportError:
    # Python 2.x
    import Queue as queue
    bytes = lambda x,*args: x
    decode = lambda s: s
import itertools

BUFF = 1024
HOST = '172.24.8.157'
PORT = 3000

def encodePlayer(player, vel):
    return "position, {}, {}, {}, {}, {}, {}\r\n".format(player.name, player.X(), player.Y(), vel[0], vel[1])

class ClientConn(threading.Thread):
    def __init__(self, name, clientsock):
        super().__init__(name="Client-{}".format(name))
        self.name = name
        self.sock = clientsock

    def run(self):
        global sockets
        try:
            while True:
                data = decode(self.sock.recv(BUFF))
                if data:
                    for sock in sockets:
                        sock.send(bytes(data, 'utf8'))
        except Exception as e:
            print(e)

    def join(self):
        self.sock.close()

if __name__=='__main__':
    ADDR = (HOST, PORT)
    serversock = socket(AF_INET, SOCK_STREAM)
    serversock.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
    serversock.bind(ADDR)
    serversock.listen(5)
    
    global socketss
    sockets = []
    
    for i in itertools.count():
        print('waiting for connection...')
        clientsock, addr = serversock.accept()
        print('...connected from:', addr)
        client = ClientConn(i, clientsock)
        clientsock.send(bytes("{}\r\n".format(client.name), 'utf8'))
        sockets.append(clientsock)
        for sock in sockets:
            sock.send(b"sendpos\r\n")
            
        client.run()

    serversock.close()
