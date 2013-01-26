import pygame

class Controls():
    def __init__(self):
        self.DOWN = 1
        self.UP = 0
        keys = ["quit", "escape", "w", "a", "s", "d"]
        self.keys = {}
        for k in keys:
            self.keys[k] = self.UP

    def update(self):
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                self.keys["quit"] = self.DOWN
            elif event.type == pygame.K_ESCAPE:
                if event.type == pygame.KEYDOWN:
                    self.keys["escape"] = self.DOWN
                else:
                    self.keys["escape"] = self.UP
            elif event.type == pygame.K_w:
                if event.type == pygame.KEYDOWN:
                    self.keys["w"] = self.DOWN
                else:
                    self.keys["w"] = self.UP
            elif event.type == pygame.K_s:
                if event.type == pygame.KEYDOWN:
                    self.keys["s"] = self.DOWN
                else:
                    self.keys["s"] = self.UP

    def down(self, key):
        if self.keys[key] == self.DOWN:
            return True
        return False

class GameObject(pygame.sprite.Sprite):
    def __init__(self, image, x = 0, y = 0):
        pygame.sprite.Sprite.__init__(self)
        if not self.loadImage(image):
            self.makeImage()
        self.rect = self.image.get_rect()
        self.rect.topleft = x, y

    def loadImage(self, imageName):
        try:
            self.image = pygame.image.load('images/' + imageName + '.png')
            return True
        except:
            return False

    def draw(self, screen):
        screen.blit(self.image, self.rect.topleft)

    def X(self):
        return self.rect.topleft[0]

    def Y(self):
        return self.rect.topleft[1]

class Movable(GameObject):
    def __init__(self, image, x=0, y=0):
        GameObject.__init__(self, image, x, y)
        self.position = [float(x), float(y)]
        self.velocity = [0.0, 0.0]
        self.acceleration = [0.0, 0.0]
        self.maxVelocity = 100.0

    def updatePosition(self, elapsed, offset = (0, 0)):
        self.position[0] += elapsed * self.velocity[0]
        self.position[1] += elapsed * self.velocity[1]
        self.velocity[0] += elapsed * self.acceleration[0]
        self.velocity[1] += elapsed * self.acceleration[1]
        if self.velocity[0] > self.maxVelocity:
            self.velocity[0] = self.maxVelocity
        elif self.velocity[0] < -self.maxVelocity:
            self.velocity[0] = -self.maxVelocity
        if self.velocity[1] > self.maxVelocity:
            self.velocity[1] = self.maxVelocity
        elif self.velocity[1] < -self.maxVelocity:
            self.velocity[1] = -self.maxVelocity
        self.rect.topleft = self.position[0] - offset[0], self.position[1] - offset[1]
