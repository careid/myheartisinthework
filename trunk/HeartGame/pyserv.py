from socket import *
import thread
 
BUFF = 1024
HOST = '127.0.0.1'
PORT = 1007
 
def handler(clientsock,addr):
    while True:
        data = clientsock.recv(BUFF)
        print 'data:' + repr(data)
        if not data: break
        clientsock.send("5\r\n")
    clientsock.close()
 
if __name__=='__main__':
    ADDR = (HOST, PORT)
    serversock = socket(AF_INET, SOCK_STREAM)
    serversock.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
    serversock.bind(ADDR)
    serversock.listen(5)
    while True:
        print 'waiting for connection...'
        clientsock, addr = serversock.accept()
        print '...connected from:', addr
        thread.start_new_thread(handler, (clientsock, addr))
    serversock.close()
