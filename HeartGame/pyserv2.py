import socket
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
HOST = '169.254.67.42'
PORT = 3000

class ClientConn(threading.Thread):
    def __init__(self, name, clientsock):
        super(type(self), self).__init__(name="Client-{}".format(name))
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
        except socket.error as e:
            sockets.remove(self.sock)
            for sock in sockets:
                sock.send(b"exit\r\n")
        except Exception as e:
            print(e)
        finally:
            self.sock.close()


if __name__=='__main__':
    ADDR = (HOST, PORT)
    serversock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    serversock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    serversock.bind(ADDR)
    serversock.listen(5)
    
    global sockets
    sockets = []
    
    for i in itertools.count():
        print('waiting for connection...')
        clientsock, addr = serversock.accept()
        print('...connected from:', addr)
        client = ClientConn(i, clientsock)
        clientsock.send(bytes(str(client.name) + "\r\n", 'utf8'))
        sockets.append(clientsock)
        for sock in sockets:
            sock.send(b"sendpos\r\n")
            
        client.start()

    serversock.close()
