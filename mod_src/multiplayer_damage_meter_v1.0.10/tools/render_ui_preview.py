from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageFont


ROOT = Path(r"C:\算法\小应用\Slay the Spire 2\mod_src\multiplayer_damage_meter_v1.0.10")
OUT_DIR = ROOT / "preview"
OUT_PATH = OUT_DIR / "ui_preview_mock.png"
POPUP_PATH = OUT_DIR / "combat_end_popup_preview.png"


BG_TOP = (26, 39, 62)
BG_BOTTOM = (8, 15, 28)
TEXT = (244, 249, 255)
TEXT_SOFT = (204, 218, 232)
TEXT_DIM = (166, 182, 198)
GOLD = (244, 212, 144)
SILVER = (219, 229, 243)
BRONZE = (230, 186, 140)
WHITE = (255, 255, 255)
BLACK = (0, 0, 0)


def load_font(size: int, bold: bool = False):
    candidates = []
    if bold:
        candidates.extend(
            [
                r"C:\Windows\Fonts\msyhbd.ttc",
                r"C:\Windows\Fonts\simhei.ttf",
            ]
        )
    candidates.extend(
        [
            r"C:\Windows\Fonts\msyh.ttc",
            r"C:\Windows\Fonts\simhei.ttf",
            r"C:\Windows\Fonts\arial.ttf",
        ]
    )
    for path in candidates:
        try:
            return ImageFont.truetype(path, size=size)
        except OSError:
            continue
    return ImageFont.load_default()


FONT_12 = load_font(12)
FONT_14 = load_font(14)
FONT_16 = load_font(16)
FONT_18 = load_font(18)
FONT_20 = load_font(20, bold=True)
FONT_22 = load_font(22, bold=True)
FONT_26 = load_font(26, bold=True)
FONT_30 = load_font(30, bold=True)
FONT_36 = load_font(36, bold=True)


def draw_text(draw, xy, text, font, fill, anchor=None, shadow_alpha=210):
    x, y = xy
    draw.text((x + 1, y + 1), text, font=font, fill=(0, 0, 0, shadow_alpha), anchor=anchor)
    draw.text((x, y), text, font=font, fill=fill, anchor=anchor)


def rounded_mask(size, radius):
    mask = Image.new("L", size, 0)
    draw = ImageDraw.Draw(mask)
    draw.rounded_rectangle((0, 0, size[0] - 1, size[1] - 1), radius=radius, fill=255)
    return mask


def add_shadow(canvas, box, radius, offset=(0, 10), blur=18, alpha=118):
    x0, y0, x1, y1 = box
    shadow_mask = Image.new("L", canvas.size, 0)
    draw = ImageDraw.Draw(shadow_mask)
    dx, dy = offset
    draw.rounded_rectangle((x0 + dx, y0 + dy, x1 + dx, y1 + dy), radius=radius, fill=alpha)
    shadow_mask = shadow_mask.filter(ImageFilter.GaussianBlur(blur))
    shadow = Image.new("RGBA", canvas.size, (8, 18, 32, 0))
    shadow.putalpha(shadow_mask)
    canvas.alpha_composite(shadow)


def glass_panel(canvas, box, radius=24, tint=(198, 220, 244, 74), border=(255, 255, 255, 118), inner=(255, 250, 239, 34), shadow_alpha=112):
    add_shadow(canvas, box, radius, alpha=shadow_alpha)
    x0, y0, x1, y1 = box
    crop = canvas.crop(box).filter(ImageFilter.GaussianBlur(14)).convert("RGBA")
    tint_layer = Image.new("RGBA", crop.size, tint)
    crop.alpha_composite(tint_layer)
    crop.alpha_composite(Image.new("RGBA", crop.size, (255, 255, 255, 12)))
    crop.putalpha(rounded_mask(crop.size, radius).point(lambda p: int(p * 0.9)))
    canvas.alpha_composite(crop, (x0, y0))

    deco = Image.new("RGBA", crop.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(deco)
    draw.rounded_rectangle((0, 0, crop.size[0] - 1, crop.size[1] - 1), radius=radius, outline=border, width=1)
    draw.rounded_rectangle((1, 1, crop.size[0] - 2, crop.size[1] - 2), radius=max(radius - 1, 1), outline=inner, width=1)
    draw.rounded_rectangle((18, 10, crop.size[0] - 18, int(crop.size[1] * 0.34)), radius=max(radius - 8, 8), fill=(255, 255, 255, 18))
    draw.line((24, 12, crop.size[0] - 24, 12), fill=(255, 255, 255, 38), width=1)
    deco.putalpha(rounded_mask(crop.size, radius))
    canvas.alpha_composite(deco, (x0, y0))


def build_background(size):
    canvas = Image.new("RGBA", size, BG_BOTTOM)
    draw = ImageDraw.Draw(canvas)
    width, height = size
    for i in range(height):
        t = i / max(height - 1, 1)
        color = (
            int(BG_TOP[0] * (1 - t) + BG_BOTTOM[0] * t),
            int(BG_TOP[1] * (1 - t) + BG_BOTTOM[1] * t),
            int(BG_TOP[2] * (1 - t) + BG_BOTTOM[2] * t),
            255,
        )
        draw.line((0, i, width, i), fill=color)

    blob_layer = Image.new("RGBA", size, (0, 0, 0, 0))
    blob_draw = ImageDraw.Draw(blob_layer)
    blob_draw.ellipse((-160, height - 420, 520, height + 240), fill=(78, 118, 192, 126))
    blob_draw.ellipse((width - 540, -160, width + 180, 520), fill=(96, 134, 208, 108))
    blob_draw.ellipse((width // 2 - 220, 30, width // 2 + 260, 420), fill=(214, 162, 98, 52))
    blob_layer = blob_layer.filter(ImageFilter.GaussianBlur(52))
    canvas.alpha_composite(blob_layer)

    haze = Image.new("RGBA", size, (0, 0, 0, 0))
    haze_draw = ImageDraw.Draw(haze)
    haze_draw.rectangle((0, 0, width, height), fill=(255, 255, 255, 14))
    haze = haze.filter(ImageFilter.GaussianBlur(22))
    canvas.alpha_composite(haze)
    return canvas


def draw_hud(canvas):
    box = (84, 190, 448, 618)
    glass_panel(canvas, box, radius=28, tint=(206, 224, 246, 76), border=(255, 255, 255, 122), inner=(255, 245, 222, 30), shadow_alpha=126)
    draw = ImageDraw.Draw(canvas)

    title_box = (98, 204, 434, 252)
    glass_panel(canvas, title_box, radius=18, tint=(224, 204, 158, 42), border=(255, 241, 214, 92), inner=(255, 255, 255, 18), shadow_alpha=80)
    draw_text(draw, (120, 216), "伤害统计", FONT_22, TEXT)

    cards = [
        ("阿璃", 126, 841),
        ("DekuLongNamePlayer", 98, 622),
        ("猫头鹰", 51, 413),
    ]

    card_y = 266
    for name, combat, total in cards:
        row_box = (98, card_y, 434, card_y + 102)
        glass_panel(canvas, row_box, radius=20, tint=(204, 222, 244, 60), border=(255, 255, 255, 94), inner=(255, 255, 255, 14), shadow_alpha=72)
        draw_text(draw, (118, card_y + 18), name, FONT_18, TEXT)
        draw_text(draw, (118, card_y + 48), f"本场战斗 {combat}", FONT_20, GOLD)
        draw_text(draw, (118, card_y + 76), f"总伤害 {total}", FONT_14, TEXT_SOFT)
        card_y += 112


def draw_popup(canvas):
    box = (548, 120, 1460, 792)
    glass_panel(canvas, box, radius=32, tint=(204, 220, 244, 78), border=(255, 255, 255, 126), inner=(255, 247, 229, 34), shadow_alpha=138)
    draw = ImageDraw.Draw(canvas)
    center_x = (box[0] + box[2]) // 2

    draw_text(draw, (center_x, box[1] + 42), "本场伤害摘要", FONT_30, TEXT, anchor="ma")
    draw_text(draw, (center_x, box[1] + 82), "已计入格挡伤害，不计超额击杀伤害", FONT_16, TEXT_SOFT, anchor="ma")

    total_box = (box[0] + 50, box[1] + 126, box[2] - 50, box[1] + 178)
    glass_panel(canvas, total_box, radius=18, tint=(210, 224, 244, 60), border=(255, 238, 208, 76), inner=(255, 255, 255, 14), shadow_alpha=72)
    draw_text(draw, (total_box[0] + 18, total_box[1] + 14), "总伤害", FONT_16, TEXT_SOFT)
    draw_text(draw, (total_box[2] - 20, total_box[1] + 8), "421", FONT_26, GOLD, anchor="ra")

    rows = [
        (1, "阿璃", 126, 841, GOLD),
        (2, "DekuLongNamePlayer", 98, 622, SILVER),
        (3, "猫头鹰", 51, 413, BRONZE),
        (4, "路人甲", 0, 159, TEXT_DIM),
    ]
    max_damage = max(row[2] for row in rows)
    row_y = box[1] + 196
    row_h = 72

    for rank, name, combat, total, accent in rows:
        row_box = (box[0] + 50, row_y, box[2] - 50, row_y + row_h)
        glass_panel(canvas, row_box, radius=16, tint=(208, 224, 244, 48), border=(255, 255, 255, 76), inner=(255, 255, 255, 10), shadow_alpha=60)
        draw.rounded_rectangle((row_box[0] + 12, row_box[1] + 14, row_box[0] + 17, row_box[3] - 14), radius=3, fill=accent + (180,))
        draw_text(draw, (row_box[0] + 46, row_box[1] + 18), f"#{rank}", FONT_18, accent)
        draw_text(draw, (row_box[0] + 102, row_box[1] + 18), name, FONT_18, TEXT)

        bar_box = (row_box[0] + 306, row_box[1] + 31, row_box[0] + 548, row_box[1] + 41)
        draw.rounded_rectangle(bar_box, radius=5, fill=(255, 255, 255, 24), outline=(255, 255, 255, 28), width=1)
        if combat > 0 and max_damage > 0:
            fill_w = int((bar_box[2] - bar_box[0]) * combat / max_damage)
            draw.rounded_rectangle((bar_box[0], bar_box[1], bar_box[0] + max(fill_w, 10), bar_box[3]), radius=5, fill=accent + (224,))

        draw_text(draw, (row_box[2] - 26, row_box[1] + 14), str(combat), FONT_22, accent, anchor="ra")
        draw_text(draw, (row_box[2] - 26, row_box[1] + 44), f"累计 {total}", FONT_12, TEXT_SOFT, anchor="ra")
        row_y += row_h + 10

    button_box = (center_x - 80, box[3] - 88, center_x + 80, box[3] - 36)
    glass_panel(canvas, button_box, radius=18, tint=(226, 205, 162, 54), border=(255, 242, 216, 112), inner=(255, 255, 255, 18), shadow_alpha=68)
    draw_text(draw, (center_x, button_box[1] + 14), "关闭", FONT_18, TEXT, anchor="ma")


def draw_preview():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    canvas = build_background((1600, 900))
    draw = ImageDraw.Draw(canvas)

    draw_text(draw, (78, 58), "UI Mock Preview", FONT_16, TEXT_SOFT)
    draw_text(draw, (78, 86), "液态玻璃 HUD + 战斗结束摘要弹窗", FONT_36, TEXT)
    draw_text(draw, (78, 132), "平衡款方案：高透明度、轻模糊、柔和高光，保留战斗内可读性。", FONT_16, TEXT_SOFT)

    draw_hud(canvas)
    draw_popup(canvas)
    canvas.save(OUT_PATH)


def draw_popup_preview():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    canvas = build_background((1120, 780))
    popup = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    popup.alpha_composite(canvas)
    draw_popup(popup)
    popup.save(POPUP_PATH)


if __name__ == "__main__":
    draw_preview()
    draw_popup_preview()
    print(OUT_PATH)
    print(POPUP_PATH)
