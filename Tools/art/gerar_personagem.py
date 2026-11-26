"""Desenha a protagonista do PSI-KID (menina careca de camisola de laboratório), quadro a quadro, nas mesmas folhas e fatias
das animações originais (células de 32x32; o corpo ocupa x 6..25, pés na linha 31).

Uso: python Tools/art/gerar_personagem.py [--preview saida.png]
Sobrescreve Assets/Sprites/Characters/{idle,walking,jumping,power,power_walking,player}.png
mantendo o tamanho de cada folha, então os .meta (fatias) e as animações continuam válidos.
"""
import math
import sys
from pathlib import Path

from PIL import Image

CHAR = Path(__file__).resolve().parents[2] / "Assets" / "Sprites" / "Characters"

T = (0, 0, 0, 0)
OUT = (20, 14, 30, 255)
SKIN = (246, 204, 166, 255)
SKIN_SH = (216, 164, 130, 255)
HAIR = (62, 44, 98, 255)
HAIR_HI = (104, 78, 150, 255)
HOOD = (226, 224, 236, 255)      # camisola de hospital
HOOD_SH = (176, 172, 198, 255)
HOOD_HI = (250, 250, 255, 255)
BADGE = (94, 200, 214, 255)
BLOOD = (196, 40, 52, 255)
LASH = (24, 18, 36, 255)
PSI = (192, 132, 252, 255)
PSI_HI = (240, 222, 255, 255)
PANTS = (44, 46, 74, 255)
PANTS_SH = (32, 32, 54, 255)
SHOE = (232, 232, 242, 255)
SHOE_SH = (150, 150, 176, 255)
EYE = (24, 18, 36, 255)
CHEEK = (236, 150, 150, 255)


class Canvas:
    def __init__(self):
        self.img = Image.new("RGBA", (32, 32), T)
        self.px = self.img.load()

    def p(self, x, y, c):
        if 0 <= x < 32 and 0 <= y < 32:
            self.px[x, y] = c

    def rect(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.p(x, y, c)

    def outline(self):
        src = self.img.copy().load()
        for y in range(32):
            for x in range(32):
                if src[x, y][3] == 0:
                    for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                        nx, ny = x + dx, y + dy
                        if 0 <= nx < 32 and 0 <= ny < 32 and src[nx, ny][3] > 0 and src[nx, ny] != OUT:
                            self.px[x, y] = OUT
                            break


def kid(bob=0, step=0.0, arms="down", eyes="open", glow=0.0, airborne=False, hair_float=0):  # hair_float mantido por compatibilidade
    """bob: deslocamento vertical da parte de cima (0/1). step: fase da passada (-1..1).
    arms: down | swing | temple | up. eyes: open | blink | glow. glow: brilho da gema 0..1."""
    c = Canvas()
    top = 3 + bob  # topo da cabeça

    # ---- pernas e tênis (não sobem com o bob)
    def leg(x, lift, fwd):
        y_foot = 30 - lift
        c.rect(x, 25, x + 2, y_foot - 1, SKIN)   # pernas à mostra (camisola)
        c.p(x, 25, SKIN_SH)
        c.rect(x + fwd, y_foot, x + 3 + fwd, y_foot + 1, SHOE)
        c.p(x + fwd, y_foot + 1, SHOE_SH)
        c.p(x + 3 + fwd, y_foot + 1, SHOE_SH)

    if airborne:
        leg(11, 3, -1)
        leg(16, 2, 1)
    else:
        a = int(round(step * 2))
        leg(11, max(0, a), -min(0, a) * 0 + (1 if a < 0 else 0))
        leg(16, max(0, -a), (1 if a > 0 else 0))

    # ---- corpo: camisola de hospital (vestido até o joelho)
    by = top + 13
    c.rect(10, by, 21, 25, HOOD)
    c.rect(9, 23, 22, 25, HOOD)          # barra mais larga
    c.rect(10, by, 11, 25, HOOD_SH)
    c.rect(9, 24, 10, 25, HOOD_SH)
    c.rect(20, by + 1, 21, 25, HOOD_SH)
    c.rect(13, by, 18, by, HOOD_SH)      # gola
    c.p(15, by + 3, HOOD_HI)
    c.p(16, by + 6, HOOD_HI)
    c.rect(17, by + 3, 19, by + 4, BADGE)  # crachá de paciente
    c.p(17, by + 3, HOOD_HI)

    # ---- braços
    def arm(side, pose):
        x = 8 if side < 0 else 22
        if pose == "down":
            c.rect(x, by + 1, x + 1, by + 7, HOOD)
            c.rect(x, by + 8, x + 1, by + 9, SKIN)
        elif pose == "fwd":
            c.rect(x, by + 1, x + 1, by + 6, HOOD)
            c.rect(x + (1 if side > 0 else -1), by + 6, x + (2 if side > 0 else 0), by + 8, SKIN)
        elif pose == "back":
            c.rect(x, by + 1, x + 1, by + 6, HOOD_SH)
            c.rect(x - (1 if side > 0 else -1), by + 7, x - (0 if side > 0 else -1), by + 8, SKIN_SH)
        elif pose == "temple":  # mão na têmpora, concentrando
            c.rect(x, by - 2, x + 1, by + 3, HOOD)
            c.rect(x, by - 4, x + 1, by - 3, SKIN)
        elif pose == "up":
            c.rect(x, by - 5, x + 1, by + 2, HOOD)
            c.rect(x, by - 7, x + 1, by - 6, SKIN)

    if arms == "down":
        arm(-1, "down"); arm(1, "down")
    elif arms == "swing":
        if step >= 0:
            arm(-1, "back"); arm(1, "fwd")
        else:
            arm(-1, "fwd"); arm(1, "back")
    elif arms == "temple":
        arm(-1, "down"); arm(1, "temple")
    elif arms == "up":
        arm(-1, "up"); arm(1, "up")

    # ---- cabeça raspada, redonda (14 x 13), olhando para a direita
    hx0, hx1 = 9, 22
    hy0, hy1 = top, top + 12
    rows = [(4, 4), (2, 2), (1, 1), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (1, 1), (2, 1), (3, 3)]
    for i, (l, r) in enumerate(rows):
        c.rect(hx0 + l, hy0 + i, hx1 - r, hy0 + i, SKIN)
    for i in range(5, 11):
        c.p(hx0, hy0 + i, SKIN_SH)                    # sombra da nuca
    c.rect(hx0 + 1, hy0 + 9, hx0 + 2, hy0 + 11, SKIN_SH)
    c.rect(hx0 + 5, hy0 + 1, hx0 + 8, hy0 + 1, (255, 232, 206, 255))  # brilho da careca
    c.rect(hx0 + 4, hy0 + 2, hx0 + 5, hy0 + 2, (255, 232, 206, 255))
    c.p(11, hy0 + 7, SKIN_SH)  # orelha
    c.p(11, hy0 + 8, SKIN_SH)
    # sobrancelhas
    ey = hy0 + 6
    c.rect(15, ey - 2, 16, ey - 2, SKIN_SH)
    c.rect(19, ey - 2, 21, ey - 2, SKIN_SH)
    # olhos grandes com cílios
    if eyes == "blink":
        c.rect(15, ey + 1, 16, ey + 1, LASH)
        c.rect(19, ey + 1, 20, ey + 1, LASH)
    else:
        col = PSI_HI if eyes == "glow" else EYE
        c.rect(15, ey, 16, ey + 2, col)
        c.rect(19, ey, 20, ey + 2, col)
        c.rect(15, ey - 1, 16, ey - 1, LASH)   # cílios
        c.rect(19, ey - 1, 21, ey - 1, LASH)
        c.p(14, ey - 1, LASH)
        if eyes != "glow":
            c.p(16, ey, SHOE)
            c.p(20, ey, SHOE)
    c.p(14, ey + 3, CHEEK)
    c.p(21, ey + 3, CHEEK)
    c.p(18, ey + 4, (214, 120, 128, 255))  # boca
    if eyes == "glow" and glow > 0.4:
        c.p(18, ey + 3, BLOOD)  # esforço psíquico: um fio de sangue no nariz
    c.outline()
    return c.img


def sheet(name, cols, rows, frames):
    img = Image.new("RGBA", (cols * 32, rows * 32), T)
    for i, f in enumerate(frames):
        img.paste(f, ((i % cols) * 32, (i // cols) * 32))
    img.save(CHAR / name)
    return img


def build():
    out = {}
    idle = []
    for i in range(8):
        bob = 1 if i in (2, 3, 4) else 0
        idle.append(kid(bob=bob, eyes="blink" if i == 6 else "open"))
    out["idle.png"] = sheet("idle.png", 3, 3, idle)

    walk = []
    for i in range(14):
        ph = math.sin(i / 14 * 2 * math.pi * 2)  # duas passadas por ciclo
        walk.append(kid(bob=1 if abs(ph) < 0.4 else 0, step=ph, arms="swing"))
    out["walking.png"] = sheet("walking.png", 4, 4, walk)

    out["jumping.png"] = sheet("jumping.png", 1, 1, [kid(arms="up", airborne=True, hair_float=1)])

    power = []
    for i in range(10):
        pulse = 0.5 + 0.5 * math.sin(i / 10 * 2 * math.pi)
        power.append(kid(bob=1 if i % 4 in (1, 2) else 0, arms="temple", eyes="glow", glow=pulse, hair_float=1 if pulse > 0.5 else 0))
    out["power.png"] = sheet("power.png", 3, 4, power)

    pw = [kid(step=0.8, arms="temple", eyes="glow", glow=1, hair_float=1),
          kid(step=-0.8, bob=1, arms="temple", eyes="glow", glow=0.6)]
    out["power_walking.png"] = sheet("power_walking.png", 1, 2, pw)

    out["player.png"] = sheet("player.png", 1, 1, [kid()])
    return out


if __name__ == "__main__":
    sheets = build()
    if "--preview" in sys.argv:
        dest = sys.argv[sys.argv.index("--preview") + 1]
        frames = []
        for n in ["idle.png", "walking.png", "jumping.png", "power.png"]:
            im = sheets[n]
            for r in range(im.height // 32):
                for c2 in range(im.width // 32):
                    cell = im.crop((c2 * 32, r * 32, c2 * 32 + 32, r * 32 + 32))
                    if cell.getbbox():
                        frames.append(cell)
        prev = Image.new("RGBA", (len(frames) * 34, 34), (40, 34, 64, 255))
        for i, f in enumerate(frames):
            prev.paste(f, (i * 34 + 1, 1), f)
        prev = prev.resize((prev.width * 4, prev.height * 4), Image.NEAREST)
        prev.save(dest)
    print("ok")
