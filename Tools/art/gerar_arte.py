"""Gera a arte nova do PSI-KID (pixel art 16 px por unidade), de forma reproduzível.

Saída: Assets/Resources/Art/*.png (carregadas em runtime por nome)
Uso:   python Tools/art/gerar_arte.py
Paleta: laboratório escuro (fundo), aço frio (objetos), roxo/ciano para o poder psíquico.
"""
import math
from pathlib import Path

from PIL import Image, ImageDraw

OUT = Path(__file__).resolve().parents[2] / "Assets" / "Resources" / "Art"
OUT.mkdir(parents=True, exist_ok=True)

# paleta
OUTLINE = (13, 11, 22, 255)
WALL_0 = (22, 18, 38, 255)
WALL_1 = (30, 24, 50, 255)
WALL_HI = (44, 36, 70, 255)
WALL_SH = (15, 12, 27, 255)
STEEL_0 = (64, 70, 96, 255)
STEEL_1 = (98, 106, 138, 255)
STEEL_2 = (150, 158, 190, 255)
STEEL_3 = (205, 212, 232, 255)
PSI = (192, 132, 252, 255)      # roxo psíquico
PSI_HI = (233, 213, 255, 255)
CYAN = (94, 234, 255, 255)
CYAN_DIM = (40, 110, 130, 255)
RED = (239, 68, 68, 255)
RED_DIM = (110, 30, 38, 255)
GREEN = (74, 222, 128, 255)
HAZARD = (250, 204, 21, 255)


def save(img, name):
    img.save(OUT / name)
    print("ok", name, img.size)


def px(d, pts, c):
    for p in pts:
        d.point(p, fill=c)


# ---------------------------------------------------------------- parede do fundo (tile 32x32, sem emenda)
def back_wall():
    img = Image.new("RGBA", (32, 32), WALL_0)
    d = ImageDraw.Draw(img)
    for ox, oy in [(0, 0), (16, 0), (0, 16), (16, 16)]:
        d.rectangle((ox, oy, ox + 15, oy + 15), fill=WALL_1 if (ox + oy) % 32 == 0 else WALL_0)
        d.line((ox, oy, ox + 15, oy), fill=WALL_HI)
        d.line((ox, oy, ox, oy + 15), fill=WALL_HI)
        d.line((ox, oy + 15, ox + 15, oy + 15), fill=WALL_SH)
        d.line((ox + 15, oy, ox + 15, oy + 15), fill=WALL_SH)
        px(d, [(ox + 2, oy + 2), (ox + 13, oy + 2), (ox + 2, oy + 13), (ox + 13, oy + 13)], WALL_SH)
    # uma grade de ventilação num dos painéis
    for y in range(20, 28, 2):
        d.line((4, y, 11, y), fill=WALL_SH)
    save(img, "back_wall.png")


# ---------------------------------------------------------------- caixa (16x16) normal e segurada
def box(held):
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, 15, 15), fill=OUTLINE)
    d.rectangle((1, 1, 14, 14), fill=STEEL_1)
    d.rectangle((3, 3, 12, 12), fill=STEEL_0)
    # moldura com brilho em cima/esquerda
    d.line((1, 1, 14, 1), fill=STEEL_3)
    d.line((1, 1, 1, 14), fill=STEEL_2)
    d.line((1, 14, 14, 14), fill=(40, 44, 62, 255))
    d.line((14, 1, 14, 14), fill=(52, 57, 80, 255))
    # cantoneiras de perigo
    for (x, y) in [(2, 2), (12, 2), (2, 12), (12, 12)]:
        d.rectangle((x, y, x + 1, y + 1), fill=HAZARD)
    # núcleo psi-reativo no centro
    core = PSI_HI if held else CYAN_DIM
    ring = PSI if held else (52, 60, 84, 255)
    d.rectangle((6, 6, 9, 9), fill=ring)
    d.rectangle((7, 7, 8, 8), fill=core)
    if held:
        d.rectangle((0, 0, 15, 15), outline=PSI)  # contorno roxo quando segurada
    save(img, "box_held.png" if held else "box.png")


# ---------------------------------------------------------------- placa de pressão (16x16) solta e pressionada
def plate(pressed):
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    base_y = 12
    d.rectangle((0, base_y, 15, 15), fill=OUTLINE)
    d.rectangle((1, base_y + 1, 14, 15), fill=STEEL_0)
    top = base_y - (1 if pressed else 3)
    d.rectangle((2, top, 13, base_y), fill=OUTLINE)
    d.rectangle((3, top + 1, 12, base_y - 1 if base_y - 1 >= top + 1 else top + 1), fill=STEEL_2)
    d.line((3, top + 1, 12, top + 1), fill=STEEL_3)
    led = GREEN if pressed else RED
    d.rectangle((7, base_y + 2, 8, base_y + 2), fill=led)
    save(img, "plate_on.png" if pressed else "plate_off.png")


# ---------------------------------------------------------------- porta do laboratório (16x32, duas metades 16x16)
def door():
    img = Image.new("RGBA", (16, 32), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle((1, 0, 14, 31), fill=OUTLINE)
    d.rectangle((2, 0, 13, 31), fill=STEEL_1)
    d.line((2, 0, 2, 31), fill=STEEL_2)
    d.line((13, 0, 13, 31), fill=STEEL_0)
    # listras de perigo no topo e na base
    for i in range(0, 12, 4):
        d.polygon([(2 + i, 2), (4 + i, 2), (2 + i + 2, 4), (2 + i, 4)], fill=HAZARD)
        d.polygon([(2 + i, 27), (4 + i, 27), (2 + i + 2, 29), (2 + i, 29)], fill=HAZARD)
    # faixa de energia no centro
    d.rectangle((6, 6, 9, 25), fill=OUTLINE)
    d.rectangle((7, 7, 8, 24), fill=CYAN)
    px(d, [(7, 7), (8, 24)], (210, 250, 255, 255))
    img.crop((0, 0, 16, 16)).save(OUT / "door_top.png")
    img.crop((0, 16, 16, 32)).save(OUT / "door_bottom.png")
    save(img, "door.png")


# ---------------------------------------------------------------- brilho radial suave (aditivo) e partícula
def glow(size=64):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    c = (size - 1) / 2
    for y in range(size):
        for x in range(size):
            r = math.hypot(x - c, y - c) / c
            a = max(0.0, 1 - r) ** 2.2
            img.putpixel((x, y), (255, 255, 255, int(255 * a)))
    save(img, "glow.png")


def particle():
    img = Image.new("RGBA", (4, 4), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle((1, 0, 2, 3), fill=(255, 255, 255, 255))
    d.rectangle((0, 1, 3, 2), fill=(255, 255, 255, 255))
    save(img, "psi_particle.png")


# ---------------------------------------------------------------- cursor psíquico (16x16)
def cursor():
    img = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.ellipse((1, 1, 14, 14), outline=OUTLINE, width=3)
    d.ellipse((2, 2, 13, 13), outline=PSI, width=1)
    d.rectangle((7, 7, 8, 8), fill=PSI_HI)
    for (x0, y0, x1, y1) in [(7, 0, 8, 2), (7, 13, 8, 15), (0, 7, 2, 8), (13, 7, 15, 8)]:
        d.rectangle((x0, y0, x1, y1), fill=PSI_HI)
    save(img, "psi_cursor.png")


# ---------------------------------------------------------------- vinheta (escurece as bordas da tela)
def vignette(w=320, h=180):
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    for y in range(h):
        for x in range(w):
            dx, dy = (x - w / 2) / (w / 2), (y - h / 2) / (h / 2)
            r = math.hypot(dx * 0.9, dy)
            a = max(0.0, min(1.0, (r - 0.55) / 0.6)) ** 1.6
            img.putpixel((x, y), (8, 6, 16, int(200 * a)))
    save(img, "vignette.png")


def door_mask():
    save(Image.new("RGBA", (16, 32), (255, 255, 255, 255)), "door_mask.png")


if __name__ == "__main__":
    door_mask()
    back_wall()
    box(False)
    box(True)
    plate(False)
    plate(True)
    door()
    glow()
    particle()
    cursor()
    vignette()


# ---------------------------------------------------------------- paredes: repintura do Tileset_10 (TotusLotus)
# Mantém o desenho e as fatias do tileset original e troca a paleta lilás/magenta
# por aço azulado com detalhe verde-azulado, que contrasta com a parede do fundo.
WALL_REMAP = {
    (104, 111, 153): (84, 96, 126),    # corpo
    (223, 224, 232): (178, 194, 216),  # brilho
    (105, 36, 100): (40, 46, 68),      # sombra
    (156, 42, 112): (46, 146, 156),    # borda (era magenta)
    (0, 0, 0): (12, 10, 20),           # contorno
}


def recolor_walls():
    path = OUT.parents[1] / "Sprites" / "Environment" / "Tiles_pack" / "Tileset_10.png"
    src = path.with_name("Tileset_10.original.png")
    if not src.exists():
        src = path  # primeira vez: usa o arquivo atual como fonte
        Image.open(path).save(path.parents[3].parent / "Tools" / "art" / "Tileset_10.original.png")
    img = Image.open(path.parents[3].parent / "Tools" / "art" / "Tileset_10.original.png").convert("RGBA")
    px = img.load()
    for y in range(img.height):
        for x in range(img.width):
            r, g, b, a = px[x, y]
            if a and (r, g, b) in WALL_REMAP:
                px[x, y] = WALL_REMAP[(r, g, b)] + (a,)
    img.save(path)
    print("ok Tileset_10 repintado")


if __name__ == "__main__" and "--paredes" in __import__("sys").argv:
    recolor_walls()


# ---------------------------------------------------------------- ícone do tutorial (32x32): caixa segurada + cursor psíquico
def tutorial_icon():
    path = OUT.parents[1] / "Sprites" / "UI" / "tutorial_01.png"
    img = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    box = Image.open(OUT / "box_held.png")
    cur = Image.open(OUT / "psi_cursor.png")
    img.paste(box, (4, 4), box)
    img.paste(cur, (14, 14), cur)
    img.save(path)
    print("ok tutorial_01")


if __name__ == "__main__" and "--tutorial" in __import__("sys").argv:
    tutorial_icon()


# ---------------------------------------------------------------- interface (9-slice; bordas definidas no PixelArtImporter)
UI_FILL = (36, 30, 60, 255)
UI_FILL_HOVER = (86, 52, 138, 255)
UI_FILL_PRESS = (62, 38, 104, 255)


def ui_button(state):
    """Botão 24x16: contorno escuro, face com brilho no topo e 2 px de sombra embaixo (pressionado afunda)."""
    w, h = 24, 16
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    fill, line, hl = {
        "normal": (UI_FILL, CYAN_DIM, STEEL_0),
        "hover": (UI_FILL_HOVER, PSI, PSI_HI),
        "pressed": (UI_FILL_PRESS, PSI, PSI),
    }[state]
    top = 1 if state == "pressed" else 0
    bottom = h - 1
    # sombra (profundidade)
    if state != "pressed":
        d.rectangle([1, h - 3, w - 2, h - 1], fill=WALL_SH)
        bottom = h - 3
    d.rectangle([0, top, w - 1, bottom], fill=OUTLINE)
    d.rectangle([1, top + 1, w - 2, bottom - 1], fill=fill)
    d.rectangle([1, top + 1, w - 2, bottom - 1], outline=line)
    d.line([2, top + 2, w - 3, top + 2], fill=hl)
    # cantos arredondados de 1 px
    for x, y in [(0, top), (w - 1, top), (0, bottom), (w - 1, bottom)]:
        img.putpixel((x, y), (0, 0, 0, 0))
    if state != "pressed":
        img.putpixel((0, h - 1), (0, 0, 0, 0))
        img.putpixel((w - 1, h - 1), (0, 0, 0, 0))
    return img


def ui_panel():
    """Painel 32x32 translúcido com cantoneiras ciano e parafusos."""
    s = 32
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, s - 1, s - 1], fill=OUTLINE)
    d.rectangle([1, 1, s - 2, s - 2], fill=(20, 16, 36, 242))
    d.rectangle([2, 2, s - 3, s - 3], outline=WALL_HI)
    d.line([3, 3, s - 4, 3], fill=(52, 44, 82, 255))
    for cx, cy, dx, dy in [(2, 2, 1, 1), (s - 3, 2, -1, 1), (2, s - 3, 1, -1), (s - 3, s - 3, -1, -1)]:
        for i in range(5):
            img.putpixel((cx + dx * i, cy), CYAN)
            img.putpixel((cx, cy + dy * i), CYAN)
        img.putpixel((cx + dx * 3, cy + dy * 3), STEEL_2)
    for x, y in [(0, 0), (s - 1, 0), (0, s - 1), (s - 1, s - 1)]:
        img.putpixel((x, y), (0, 0, 0, 0))
    return img


def ui_slider(kind):
    """Trilho (escuro) e preenchimento (roxo) 12x8 do slider."""
    w, h = 12, 8
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, w - 1, h - 1], fill=OUTLINE)
    if kind == "track":
        d.rectangle([1, 1, w - 2, h - 2], fill=WALL_SH)
        d.line([1, h - 2, w - 2, h - 2], fill=WALL_HI)
    else:
        d.rectangle([1, 1, w - 2, h - 2], fill=PSI)
        d.line([1, 1, w - 2, 1], fill=PSI_HI)
        d.line([1, h - 2, w - 2, h - 2], fill=(130, 80, 200, 255))
    for x, y in [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]:
        img.putpixel((x, y), (0, 0, 0, 0))
    return img


def ui_knob():
    """Cursor do slider 10x14: peça de aço com friso ciano."""
    w, h = 10, 14
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, w - 1, h - 1], fill=OUTLINE)
    d.rectangle([1, 1, w - 2, h - 2], fill=STEEL_1)
    d.line([1, 1, w - 2, 1], fill=STEEL_3)
    d.line([1, h - 2, w - 2, h - 2], fill=STEEL_0)
    d.line([w // 2 - 1, 4, w // 2 - 1, h - 5], fill=CYAN)
    d.line([w // 2, 4, w // 2, h - 5], fill=CYAN_DIM)
    for x, y in [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]:
        img.putpixel((x, y), (0, 0, 0, 0))
    return img


def ui_toggle(on):
    """Caixa de seleção 12x12; ligada mostra o quadrado ciano aceso."""
    s = 12
    img = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 0, s - 1, s - 1], fill=OUTLINE)
    d.rectangle([1, 1, s - 2, s - 2], fill=WALL_SH)
    d.rectangle([1, 1, s - 2, s - 2], outline=CYAN_DIM if not on else CYAN)
    if on:
        d.rectangle([3, 3, s - 4, s - 4], fill=CYAN)
        d.line([3, 3, s - 4, 3], fill=(200, 250, 255, 255))
    for x, y in [(0, 0), (s - 1, 0), (0, s - 1), (s - 1, s - 1)]:
        img.putpixel((x, y), (0, 0, 0, 0))
    return img


def interface():
    for st in ("normal", "hover", "pressed"):
        save(ui_button(st), f"ui_button_{st}.png")
    save(ui_panel(), "ui_panel.png")
    save(ui_slider("track"), "ui_slider_track.png")
    save(ui_slider("fill"), "ui_slider_fill.png")
    save(ui_knob(), "ui_knob.png")
    save(ui_toggle(False), "ui_toggle_off.png")
    save(ui_toggle(True), "ui_toggle_on.png")


if __name__ == "__main__" and "--ui" in __import__("sys").argv:
    interface()
