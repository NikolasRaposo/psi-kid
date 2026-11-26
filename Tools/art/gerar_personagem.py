"""Protagonista do PSI-KID, desenhada pixel a pixel (mapa de caracteres), com as
técnicas de pixel art profissionais (Slynyrd, Derek Yu, saint11):

- silhueta primeiro; criança com ~2,3 cabeças, cabeça 10 px, olhos 1x2 baixos na cabeça
- rampas com hue shift da paleta Endesga 32 (pele, camisola branca fria)
- contorno escuro colorido por fora (#262b44) e selout por dentro (sem preto interno)
- clusters grandes, curvas sem jaggies (degraus 1-2-3), brilho da careca em arco,
  sombra fria do cabelo raspado e orelha para a cabeça não virar "bola"
- caminhada com contato/passagem + bob de 1 px, pulo com pernas recolhidas,
  poder com antecipação, braço estendido, olhos ciano e um fio de sangue no nariz

Uso: python Tools/art/gerar_personagem.py [--preview saida.png [fundo.png]]
Escreve as folhas em Assets/Sprites/Characters mantendo tamanho e fatias originais.
"""
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

CHAR = Path(__file__).resolve().parents[2] / "Assets" / "Sprites" / "Characters"

PAL = {
    ".": None,
    "o": "#262b44",  # contorno colorido
    "O": "#181425",  # olhos
    "h": "#c9b2ac",  # sombra fria do cabelo raspado
    "s": "#fff1dc",  # pele luz (brilho da careca, puxado para o amarelo)
    "S": "#f2cfa8",  # pele base
    "k": "#d99a76",  # pele sombra (hue shift para o vermelho)
    "K": "#a86158",  # pele sombra profunda
    "w": "#ffffff",  # camisola luz
    "W": "#c0cbdc",  # camisola base (branco frio)
    "g": "#8b9bb4",  # camisola sombra
    "G": "#5a6988",  # camisola dobra
    "r": "#f6757a",  # bochecha
    "b": "#e43b44",  # sangue no nariz
    "c": "#2ce8f5",  # olhos com poder
    "p": "#b55088",  # brilho psíquico
}


def rgba(hexs):
    hexs = hexs.lstrip("#")
    return (int(hexs[0:2], 16), int(hexs[2:4], 16), int(hexs[4:6], 16), 255)


# ---------------------------------------------------------------- partes (14 colunas, colocadas em x=9)
HEAD = [  # y 7..17
    "....oooooo....",
    "...ohhhsshho..",
    "..ohhSSssSSho.",
    "..oSSSSSSSSSSo",
    ".okSSSSSSSSSSo",
    "okkkSSSSSSSSSo",
    "okkkSSOSSSOSSo",
    ".okkSSOSSSOSSo",
    ".okkkSSSSSrkSo",
    "..okkkSSSSSSo.",
    "...ookkkkkoo..",
]
BODY = [  # y 18..26  mangas curtas, decote em V, laço lateral, sombra nas costas
    "....oGWwWGo...",
    "..ooGWwgwWWoo.",
    ".okoGWwwwWWoko",
    ".oSogWWwwWWoSo",
    "..oggWWWwWWgo.",
    "..oGgWWWWWWgo.",
    ".ogGgWWWWWWWgo",
    ".oGGggWWWWWggo",
    ".ooGGGgggggGoo",
]
LEGS = {  # y 27..31
    "stand": [
        "...okSo.okSo..",
        "...okSo.okSo..",
        "...oWWo.oWWo..",
        "..oWWWo.oWWWo.",
        "..oooo..oooo..",
    ],
    "contact": [
        "..okSo...okSo.",
        ".okSo.....okSo",
        ".oWWo.....oWWo",
        "oWWWo....oWWWo",
        "oooo.....oooo.",
    ],
    "pass": [
        "....okSokSo...",
        "....okSokSo...",
        "....oWWoWWo...",
        "...oWWWWWWWo..",
        "...ooooooooo..",
    ],
    "air": [
        "...okSo.okSo..",
        "..oWWo..oWWWo.",
        "..oooo..oooo..",
        "..............",
        "..............",
    ],
}


def frame(legs="stand", bob=0, eyes="open", arm=None, blood=False, dx=0, flare=False, aura=False):
    grid = [["."] * 32 for _ in range(32)]

    def put(rows, y0, x0=9):
        for j, row in enumerate(rows):
            for i, ch in enumerate(row):
                if ch != "." and 0 <= y0 + j < 32 and 0 <= x0 + i < 32:
                    grid[y0 + j][x0 + i] = ch

    put(LEGS[legs], 27, 9)
    body = [list(r) for r in BODY]
    head = [list(r) for r in HEAD]
    if eyes == "blink":
        for r in (6, 7):
            head[r][6] = "S"
            head[r][10] = "S"
        head[7][6] = "k"
        head[7][10] = "k"
    elif eyes == "glow":
        for r in (6, 7):
            head[r][6] = "c"
            head[r][10] = "c"
    if blood:
        head[8][11] = "b"
        head[9][11] = "b"
    if flare:  # camisola esvoaçando para trás
        body[6] = list("ogGgWWWWWWWgo.")
        body[7] = list("oGGggWWWWWggo.")
        body[8] = list("ooGGGgggggGoo.")
    put(["".join(r) for r in head], 7 + bob, 9 + dx)
    put(["".join(r) for r in body], 18 + bob, 9 + dx)

    if arm == "forward":  # braço estendido para a frente (manga + antebraço de 2 px + mão aberta)
        put(["ooooo..", "WWkSSoo", "gWkSSSo", "ooooooo"], 19 + bob, 21 + dx)
    elif arm == "up":  # pulo: braços para cima
        put([".oSo", ".okSo", "ogWo", "ogWo"], 15 + bob, 20 + dx)
        put(["oSo.", "oSko", "oWgo"], 15 + bob, 7 + dx)
    elif arm == "swing_f":  # mão da frente balançando, colada ao corpo
        put(["oSo", "oo."], 22 + bob, 22 + dx)
    if aura:
        for (x, y) in ((27, 17), (29, 19), (28, 22), (30, 21), (26, 15)):
            if 0 <= x + dx < 32:
                grid[y + bob][x + dx] = "p" if (x + y) % 2 else "c"

    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    px = img.load()
    for y in range(32):
        for x in range(32):
            col = PAL[grid[y][x]]
            if col:
                px[x, y] = rgba(col)
    return img


def sheet(name, cols, rows, frames):
    img = Image.new("RGBA", (cols * 32, rows * 32), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        img.paste(f, ((i % cols) * 32, (i // cols) * 32))
    img.save(CHAR / name)
    return img


def build():
    idle = [frame(bob=1 if i in (2, 3, 4) else 0, eyes="blink" if i == 6 else "open") for i in range(8)]
    cycle = [("contact", 0), ("contact", 1), ("pass", 0), ("contact", 0), ("contact", 1), ("pass", 0)]
    walk = []
    for i in range(14):
        legs, bob = cycle[i % 6]
        walk.append(frame(legs=legs, bob=bob, arm="swing_f" if i % 6 < 3 else None))
    jump = [frame(legs="air", arm="up")]
    power = []
    for i in range(10):
        if i < 2:
            power.append(frame(bob=1, dx=-1))
        else:
            power.append(frame(arm="forward", eyes="glow", blood=i >= 4, dx=(i % 2), flare=True, aura=True))
    pw = [frame(legs="contact", arm="forward", eyes="glow", flare=True, aura=True),
          frame(legs="pass", bob=1, arm="forward", eyes="glow", flare=True, aura=True)]
    sheet("idle.png", 3, 3, idle)
    sheet("walking.png", 4, 4, walk)
    sheet("jumping.png", 1, 1, jump)
    sheet("power.png", 3, 4, power)
    sheet("power_walking.png", 1, 2, pw)
    sheet("player.png", 1, 1, [idle[0]])
    return idle, walk, jump, power


def preview(dest, idle, walk, jump, power, bg_path=None):
    Z = 8
    picks = [("parada", idle[0]), ("piscando", idle[6]), ("andando: contato", walk[0]), ("andando: passagem", walk[2]),
             ("pulo", jump[0]), ("poder: antecipação", power[0]), ("poder", power[5])]
    W = 40 + len(picks) * (32 * Z + 16)
    H = 32 * Z + 280
    img = Image.new("RGBA", (W, H), (24, 20, 36, 255))
    d = ImageDraw.Draw(img)
    fb = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 30)
    fs = ImageFont.truetype("C:/Windows/Fonts/seguisb.ttf", 20)
    d.text((40, 20), "PSI-KID: protagonista v2 (pixel a pixel · paleta Endesga 32 · contorno colorido + selout)", font=fb, fill=(240, 240, 250))
    x = 40
    for label, f in picks:
        d.rectangle((x, 70, x + 32 * Z, 70 + 32 * Z), fill=(38, 32, 56))
        big = f.resize((32 * Z, 32 * Z), Image.NEAREST)
        img.paste(big, (x, 70), big)
        d.text((x, 76 + 32 * Z), label, font=fs, fill=(200, 200, 220))
        x += 32 * Z + 16
    y = 70 + 32 * Z + 50
    if bg_path:
        bg = Image.open(bg_path).convert("RGBA").resize((W - 80, 110))
        img.paste(bg, (40, y))
    x = 80
    for _, f in picks:
        s = f.resize((96, 96), Image.NEAREST)
        img.paste(s, (x, y + 8), s)
        x += 150
    img.save(dest)


if __name__ == "__main__":
    idle, walk, jump, power = build()
    if "--preview" in sys.argv:
        i = sys.argv.index("--preview")
        bg = sys.argv[i + 2] if len(sys.argv) > i + 2 else None
        preview(sys.argv[i + 1], idle, walk, jump, power, bg)
    print("ok")
