"""Quatro estudos de estilo para a protagonista do PSI-KID (menina careca com poderes,
inspirada na Eleven), no mesmo espaço do sprite do jogo (32x32, corpo em x 6..25).

Uso: python Tools/art/variantes_personagem.py <saida.png> [fundo.png]
Gera uma prancha comparativa: cada versão parada, andando e usando o poder,
ampliada e em escala real sobre um recorte do cenário.

A  Chibi com contorno colorido  - 2 cabeças, vestido rosa + jaqueta azul, contorno em tons (sem preto)
B  Proporção de 3 cabeças        - mais alta e ágil, camisola de hospital, contorno preto, pés descalços
C  Minimalista de alto contraste - silhueta clara, poucos tons, só o roxo psíquico como cor (estilo 1-bit + acento)
D  Sombria sem contorno          - cores locais escuras, casaco grande por cima da camisola, luz nos olhos
"""
import sys

from PIL import Image, ImageDraw, ImageFont

T = (0, 0, 0, 0)


class C:
    def __init__(self):
        self.img = Image.new("RGBA", (32, 32), T)
        self.px = self.img.load()

    def p(self, x, y, c):
        if 0 <= x < 32 and 0 <= y < 32 and c is not None:
            self.px[x, y] = c

    def r(self, x0, y0, x1, y1, c):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.p(x, y, c)

    def rows(self, x0, y0, spans, c):
        """spans: lista de (recuo_esq, recuo_dir, largura_total) por linha a partir de y0."""
        for i, (l, rr, w) in enumerate(spans):
            self.r(x0 + l, y0 + i, x0 + w - 1 - rr, y0 + i, c)

    def outline(self, color_fn):
        """Contorno externo; color_fn(cor_vizinha) devolve a cor da borda (contorno colorido)."""
        src = self.img.copy().load()
        for y in range(32):
            for x in range(32):
                if src[x, y][3]:
                    continue
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < 32 and 0 <= ny < 32 and src[nx, ny][3]:
                        self.px[x, y] = color_fn(src[nx, ny])
                        break


def darker(c, k=0.45):
    return (int(c[0] * k), int(c[1] * k), int(c[2] * k), 255)


# ------------------------------------------------------------------ A: chibi, contorno colorido
def var_a(pose):
    c = C()
    SK, SKS, SKH = (250, 210, 178, 255), (222, 168, 140, 255), (255, 236, 214, 255)
    DR, DRS = (240, 140, 170, 255), (204, 100, 136, 255)     # vestido rosa
    JK, JKS = (70, 96, 168, 255), (48, 66, 124, 255)         # jaqueta azul
    SO, SH = (246, 246, 250, 255), (90, 70, 80, 255)          # meia branca, sapato
    EY, PSI = (40, 26, 40, 255), (200, 150, 255, 255)
    bob = 1 if pose == "walk" else 0
    top = 4 + bob
    # pernas
    lx = [12, 17] if pose != "walk" else [11, 18]
    for i, x in enumerate(lx):
        c.r(x, 25, x + 2, 28, SO)
        c.r(x - (1 if i == 0 else 0), 29, x + 2 + (1 if i == 1 else 0), 30, SH)
    # vestido (sino) + jaqueta aberta
    c.rows(9, top + 13, [(3, 3, 14), (2, 2, 14), (2, 2, 14), (1, 1, 14), (1, 1, 14), (1, 1, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14)], DR)
    c.r(9, top + 20, 22, top + 21, DRS)
    c.r(10, top + 13, 12, top + 19, JK)
    c.r(19, top + 13, 21, top + 19, JK)
    c.r(10, top + 13, 10, top + 19, JKS)
    c.r(13, top + 13, 18, top + 13, (250, 250, 252, 255))  # gola branca
    # braços
    if pose == "power":
        c.r(22, top + 9, 23, top + 14, JK)
        c.r(22, top + 7, 23, top + 8, SK)
        c.r(8, top + 14, 9, top + 18, JK)
        c.r(8, top + 19, 9, top + 19, SK)
    else:
        sw = 1 if pose == "walk" else 0
        c.r(8, top + 14 - sw, 9, top + 18 - sw, JK)
        c.r(8, top + 19 - sw, 9, top + 19 - sw, SK)
        c.r(22, top + 14 + sw, 23, top + 18, JK)
        c.r(22, top + 19, 23, top + 19, SK)
    # cabeça grande e redonda (2 cabeças de altura)
    c.rows(8, top, [(5, 5, 16), (3, 3, 16), (2, 2, 16), (1, 1, 16), (1, 1, 16), (0, 0, 16), (0, 0, 16), (0, 0, 16), (0, 0, 16), (0, 0, 16), (1, 1, 16), (1, 1, 16), (3, 2, 16)], SK)
    c.r(9, top + 6, 9, top + 10, SKS)
    c.r(13, top + 1, 17, top + 2, SKH)
    for x in range(11, 22, 2):  # cabelo raspado: pontinhos
        c.p(x, top + 2, SKS)
    eye = PSI if pose == "power" else EY
    c.r(15, top + 6, 16, top + 8, eye)
    c.r(20, top + 6, 21, top + 8, eye)
    c.p(16, top + 6, None if pose == "power" else SO)
    c.p(21, top + 6, None if pose == "power" else SO)
    c.p(14, top + 9, (240, 160, 170, 255))
    c.p(19, top + 10, (200, 110, 120, 255))
    if pose == "power":
        c.p(19, top + 9, (200, 40, 50, 255))
    c.outline(lambda n: darker(n, 0.42))
    return c.img


# ------------------------------------------------------------------ B: 3 cabeças, contorno preto
def var_b(pose):
    c = C()
    OUT = (14, 10, 20, 255)
    SK, SKS = (244, 200, 168, 255), (206, 152, 124, 255)
    GW, GWS, GWH = (196, 220, 226, 255), (138, 168, 180, 255), (236, 246, 248, 255)  # camisola verde-água
    EY, PSI, BL = (20, 14, 26, 255), (220, 190, 255, 255), (200, 36, 48, 255)
    top = 5 + (1 if pose == "walk" else 0)
    # pernas finas e pés descalços
    if pose == "walk":
        c.r(12, 23, 13, 29, SK); c.r(11, 30, 13, 30, SKS)
        c.r(18, 23, 19, 28, SK); c.r(18, 29, 20, 29, SKS)
    else:
        c.r(13, 23, 14, 30, SK); c.r(17, 23, 18, 30, SK)
        c.r(12, 31, 14, 31, SKS); c.r(17, 31, 19, 31, SKS)
    # camisola longa
    c.r(12, top + 9, 19, 23, GW)
    c.r(11, 20, 20, 24, GW)
    c.r(12, top + 9, 12, 23, GWS)
    c.r(11, 23, 20, 24, GWS)
    c.p(17, top + 12, GWH); c.p(16, top + 15, GWH)
    c.r(15, top + 9, 16, top + 9, GWS)
    # braços
    if pose == "power":
        c.r(20, top + 4, 21, top + 10, SK)   # braço estendido para cima/frente
        c.r(22, top + 3, 23, top + 4, SK)
    else:
        c.r(10, top + 10, 11, top + 16, SK)
        c.r(20, top + 10, 21, top + 16, SK)
    # cabeça menor (10x10)
    c.rows(11, top - 3, [(3, 3, 10), (1, 1, 10), (0, 0, 10), (0, 0, 10), (0, 0, 10), (0, 0, 10), (0, 0, 10), (0, 0, 10), (1, 1, 10), (2, 2, 10)], SK)
    c.r(11, top + 1, 11, top + 4, SKS)
    c.r(14, top - 2, 16, top - 2, (255, 230, 206, 255))
    eye = PSI if pose == "power" else EY
    c.r(16, top + 1, 16, top + 2, eye)
    c.r(19, top + 1, 19, top + 2, eye)
    c.p(18, top + 4, SKS)
    if pose == "power":
        c.p(18, top + 3, BL)
    c.outline(lambda n: OUT)
    return c.img


# ------------------------------------------------------------------ C: minimalista alto contraste
def var_c(pose):
    c = C()
    W, G, K, P = (240, 240, 246, 255), (160, 160, 178, 255), (16, 14, 24, 255), (190, 120, 255, 255)
    top = 5 + (1 if pose == "walk" else 0)
    # corpo como bloco (camisola) + cabeça redonda; olhos como fendas
    if pose == "walk":
        c.r(12, 26, 13, 30, W); c.r(18, 25, 19, 29, W)
    else:
        c.r(13, 26, 14, 31, W); c.r(17, 26, 18, 31, W)
    c.rows(10, top + 12, [(2, 2, 12)] + [(1, 1, 12)] * 4 + [(0, 0, 12)] * 5, W)
    c.r(10, top + 19, 21, top + 21, G)
    c.rows(9, top, [(4, 4, 14), (2, 2, 14), (1, 1, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (1, 1, 14), (2, 2, 14), (4, 4, 14)], W)
    c.r(9, top + 4, 10, top + 8, G)
    eye = P if pose == "power" else K
    c.r(15, top + 5, 16, top + 6, eye)
    c.r(19, top + 5, 20, top + 6, eye)
    if pose == "power":
        c.r(22, top + 10, 23, top + 14, W)
        for (x, y) in ((25, top + 8), (27, top + 6), (24, top + 4)):
            c.p(x, y, P)
    c.outline(lambda n: K if n[:3] != P[:3] else P)
    return c.img


# ------------------------------------------------------------------ D: sombria, sem contorno
def var_d(pose):
    c = C()
    SK, SKS, SKD = (214, 170, 146, 255), (168, 122, 104, 255), (120, 84, 76, 255)
    CO, COS, COD = (56, 82, 86, 255), (40, 60, 64, 255), (28, 40, 44, 255)     # casaco grande verde-petróleo
    GW, GWS = (182, 182, 172, 255), (130, 130, 124, 255)                       # camisola por baixo
    SH = (36, 30, 34, 255)
    EY, GL = (30, 22, 26, 255), (176, 230, 255, 255)
    top = 5 + (1 if pose == "walk" else 0)
    if pose == "walk":
        c.r(12, 25, 13, 29, SKS); c.r(11, 30, 13, 30, SH)
        c.r(18, 24, 19, 28, SKS); c.r(18, 29, 20, 29, SH)
    else:
        c.r(13, 25, 14, 30, SKS); c.r(17, 25, 18, 30, SKS)
        c.r(12, 31, 14, 31, SH); c.r(17, 31, 19, 31, SH)
    # casaco largo até o joelho, mangas compridas
    c.r(9, top + 10, 22, 25, CO)
    c.r(9, top + 10, 10, 25, COD)
    c.r(21, top + 11, 22, 25, COS)
    c.r(14, top + 10, 17, 25, GW)       # camisola aparecendo na frente
    c.r(14, top + 10, 14, 25, GWS)
    c.r(9, 25, 22, 25, COD)
    if pose == "power":
        c.r(22, top + 6, 24, top + 11, CO)
        c.r(24, top + 4, 25, top + 5, SK)
    else:
        c.r(8, top + 11, 9, top + 19, COS)
        c.r(22, top + 11, 23, top + 19, COS)
        c.r(8, top + 20, 9, top + 20, SKS)
        c.r(22, top + 20, 23, top + 20, SKS)
    # cabeça redonda de criança, luz de cima e sombra suave embaixo
    c.rows(9, top - 2, [(4, 4, 14), (2, 2, 14), (1, 1, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (0, 0, 14), (1, 1, 14), (2, 2, 14), (4, 4, 14)], SK)
    c.r(9, top + 3, 10, top + 7, SKS)
    c.r(12, top + 8, 20, top + 9, SKS)
    c.r(13, top - 2, 18, top - 1, (236, 204, 180, 255))
    eye = GL if pose == "power" else EY
    c.r(15, top + 3, 16, top + 5, eye)
    c.r(19, top + 3, 20, top + 5, eye)
    if pose != "power":
        c.p(16, top + 3, (236, 230, 220, 255))
        c.p(20, top + 3, (236, 230, 220, 255))
    else:
        c.p(14, top + 4, (110, 190, 230, 255))
        c.p(21, top + 4, (110, 190, 230, 255))
    c.p(18, top + 7, SKD)
    return c.img


VARIANTS = [
    ("A", "Chibi, contorno colorido", "vestido rosa + jaqueta azul", var_a),
    ("B", "3 cabeças, contorno preto", "camisola de hospital, descalça", var_b),
    ("C", "Minimalista alto contraste", "silhueta clara + roxo psíquico", var_c),
    ("D", "Sombria, sem contorno", "casaco grande sobre a camisola", var_d),
]


def main():
    out = sys.argv[1]
    bg = Image.open(sys.argv[2]).convert("RGBA") if len(sys.argv) > 2 else None
    Z = 7
    cell = 32 * Z
    pad = 24
    W = max(pad + 3 * (cell + 12) + 260 + pad, 1500)
    H = 110 + len(VARIANTS) * (cell + 60)
    sheet = Image.new("RGBA", (W, H), (24, 20, 36, 255))
    d = ImageDraw.Draw(sheet)
    fb = ImageFont.truetype("C:/Windows/Fonts/segoeuib.ttf", 34)
    fs = ImageFont.truetype("C:/Windows/Fonts/seguisb.ttf", 22)
    d.text((pad, 26), "PSI-KID: 4 estudos da protagonista  (parada · andando · poder · tamanho real no jogo)", font=fb, fill=(240, 240, 250))
    y = 100
    for key, name, desc, fn in VARIANTS:
        d.text((pad, y), f"{key}  {name}", font=fb, fill=(200, 160, 255))
        d.text((pad + 520, y + 8), desc, font=fs, fill=(190, 192, 210))
        y += 48
        x = pad
        for pose in ("idle", "walk", "power"):
            img = fn(pose).resize((cell, cell), Image.NEAREST)
            d.rectangle((x - 2, y - 2, x + cell + 1, y + cell + 1), fill=(36, 30, 54))
            sheet.paste(img, (x, y), img)
            x += cell + 12
        # escala real (3x, como aparece no jogo em 1080p) sobre o cenário
        box = (x, y, x + 260, y + cell)
        if bg is not None:
            crop = bg.crop((760, 640, 760 + 260, 640 + cell)) if bg.width > 1100 else bg.resize((260, cell))
            sheet.paste(crop, (x, y))
        else:
            d.rectangle(box, fill=(22, 18, 38))
        small = fn("idle").resize((96, 96), Image.NEAREST)
        sheet.paste(small, (x + 30, y + cell - 96 - 10), small)
        small = fn("power").resize((96, 96), Image.NEAREST)
        sheet.paste(small, (x + 140, y + cell - 96 - 10), small)
        y += cell + 12
    sheet.save(out)
    print("ok", out)


if __name__ == "__main__":
    main()
