import pygame
import client
from util import *

WIDTH, HEIGHT = 640, 480

def encodePlayer(player, vel):
    info = [player.X(), player.Y(), vel[0], vel[1]]
    info = [str(v) for v in info]
    msg = ",".join(info) + '\r\n'
    return msg

class Person(Movable):
    def __init__(self, name, x=0, y=0):
        self.name = name
        Movable.__init__(self, name, x, y)

    def makeImage(self):
        self.image = pygame.Surface((64, 64))
        self.image.fill((0, 100, 0))

class Game():
    def run(self):
        pygame.init()
        self.screen = pygame.display.set_mode((WIDTH, HEIGHT))
        self.restart = True
        self.client = client.Client()
        name = self.client.connect()
        name = name.rstrip()
        print "logging in as player " + name

        self.persons = pygame.sprite.Group()
        self.player = Person(name, 100, 100)
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
            self.controls.update()
            self.updatePlayer()

            self.draw()
            pygame.display.flip()

    def networkUpdate(self, elapsed):
        moves = self.client.read()
        for m in moves:
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
                self.client.write((encodePlayer(self.player, self.player.velocity)))

        for p in self.persons:
            p.updatePosition(elapsed)

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
            self.client.write((encodePlayer(self.player, newVel)))

    def draw(self):
        self.screen.fill((0,0,0))
        self.persons.draw(self.screen)

g = Game()
g.run()
