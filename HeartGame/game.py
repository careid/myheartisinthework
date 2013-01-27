import pygame
import client
import random
from util import *

WIDTH, HEIGHT = 640, 480
ONLINE = True

def encodePlayer(player, vel):
    info = ["position", player.name, player.X(), player.Y(), vel[0], vel[1]]
    info = [str(v) for v in info]
    msg = ",".join(info) + '\r\n'
    return msg

class Person(Movable):
    def __init__(self, name, x=0, y=0):
        self.name = name
        self.positionHistory = []
        Movable.__init__(self, name, x, y)

    def makeImage(self):
        self.image = pygame.Surface((64, 64))
        self.image.fill((0, 100, 0))

    def updatePosition(self, elapsed):
        Movable.updatePosition(self, elapsed)
        if len(self.positionHistory) > 3:
            self.positionHistory = self.positionHistory[1:]
        self.positionHistory.append(self.position)
        self.rect.topleft = 0, 0
        for idx, val in enumerate([0.1, 0.2, 0.3, 0.4]):
            if idx == len(self.positionHistory):
                break
            self.rect.topleft = (self.rect.topleft[0] + self.positionHistory[idx][0] * val,
                                 self.rect.topleft[1] + self.positionHistory[idx][1] * val)

class Game():
    def run(self):
        pygame.init()
        self.screen = pygame.display.set_mode((WIDTH, HEIGHT))
        self.restart = True
        if ONLINE:
            self.client = client.Client()
            name = self.client.connect()
            name = name.rstrip()
        else:
            self.msgQueue = []
            name = "0"
        print "logging in as player " + name

        self.persons = pygame.sprite.Group()
        self.player = Person(name, 100, 100)
        #if name == "0":
        #    for i in range(1000,1010):
        #        self.persons.add(Person(str(i), random.randint(0, WIDTH), random.randint(0, HEIGHT)))
        self.persons.add(self.player)
        while self.restart:
            self.restart = False
            self.runGame()

    def runGame(self):
        clock = pygame.time.Clock()
        self.controls = Controls()

        self.running = True

        while self.running:
            clock.tick(60)
            elapsed = clock.get_time() / 1000.0

            self.networkUpdate(elapsed)
            for p in self.persons:
                p.updatePosition(elapsed)
            self.updateAI()

            self.controls.update()
            self.updatePlayer()

            self.draw()
            pygame.display.flip()

    def updateAI(self):
        if self.player.name != "0":
            return
        for p in self.persons:
            if int(p.name) < 1000:
                continue
            if random.randint(0, 100) == 0:
                self.write((encodePlayer(p, (random.randint(-100,100), random.randint(-100,100)))))

    def networkUpdate(self):
        entries = self.read()
        for e in entries:
            lines = e.split('\n')
            for m in lines:
                m = m.rstrip()
                toks = m.split(',')
                command = toks[0]
                if command == "position":
                    found = False
                    for p in self.persons:
                        if str(p.name) == str(toks[1]):
                            p.position = [float(toks[2]), float(toks[3])]
                            p.velocity = [float(toks[4]), float(toks[5])]
                            found = True
                    if not found:
                        self.persons.add(Person(toks[1], float(toks[2]), float(toks[3])))
                        self.persons.velocity = [float(toks[4]), float(toks[5])]
                elif command == "sendpos":
                    self.write((encodePlayer(self.player, self.player.velocity)))

    def updatePlayer(self):
        oldVel = (self.player.velocity[0], self.player.velocity[1])
        newVel = [0,0]
        if pygame.key.get_pressed()[pygame.K_d]:
            newVel[0] = 100
        elif pygame.key.get_pressed()[pygame.K_a]:
            newVel[0] = -100
        if pygame.key.get_pressed()[pygame.K_w]:
            newVel[1] = -100
        elif pygame.key.get_pressed()[pygame.K_s]:
            newVel[1] = 100

        if newVel[0] != oldVel[0] or newVel[1] != oldVel[1]:
            self.write(encodePlayer(self.player, newVel))

    def write(self, msg):
        if ONLINE:
            self.client.write(msg)
        else:
            self.msgQueue.append(msg)

    def read(self):
        if ONLINE:
            entries = self.client.read()
        else:
            entries = self.msgQueue
            self.msgQueue = []
        return entries

    def draw(self):
        self.screen.fill((0,0,0))
        self.persons.draw(self.screen)

g = Game()
g.run()
