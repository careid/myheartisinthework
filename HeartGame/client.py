from socket import *
import threading
import thread
import sys

HOST = '172.24.8.157'
PORT = 3000
BUFF = 1024

class Client:
    def connect(self):
        self.sock = socket(AF_INET, SOCK_STREAM)
        self.sock.connect((HOST, PORT))
        self.lock = threading.Lock()
        self.buf = []
        data = self.sock.recv(BUFF)
        thread.start_new_thread(self.listen, ())
        return data

    def listen(self):
        while True:
            data = self.sock.recv(BUFF)
            self.lock.acquire()
            self.buf.append(data)
            self.lock.release()

    def read(self):
        self.lock.acquire()
        bufCopy = self.buf
        self.buf = []
        self.lock.release()
        return bufCopy

    def write(self, msg):
        return self.sock.send(msg)

    def close(self):
        self.sock.close()
