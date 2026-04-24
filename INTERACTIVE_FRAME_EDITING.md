# 🖱️ Interactive Frame Editing

## Редактирование кадров мышью прямо на canvas!

---

## ✨ Новые возможности

### 1. 🎯 **Click to Select** - Выбор кадра
### 2. ✋ **Drag to Move** - Перетаскивание
### 3. 📏 **Resize Handles** - Изменение размера
### 4. 🎨 **Transparent Preview** - Шахматный фон

---

## 🎯 Click to Select

### Как работает:

```
1. Click на любом кадре в группе
2. ✅ Кадр выделяется (magenta border)
3. ✅ Появляются resize handles (8 белых квадратиков)
4. ✅ SelectedFrame обновляется в UI
5. ✅ Preview показывает этот кадр
```

### Визуальная обратная связь:

```
Normal frame:
┌─────────┐
│  sprite │  ← Обычная рамка
└─────────┘

Selected frame:
┌═════════┐
║ ■ ■ ■ ║  ← Толстая magenta рамка
║ ■ ■ ■ ║
║ ■ ■ ║     ← 8 resize handles
└═════════┘
 ■     ■
```

### Приоритет выбора:

```
Priority 1: Selected frame handles (если уже выбран)
Priority 2: Any frame in selected group
Priority 3: Grid group (select group)
```

---

## ✋ Drag to Move

### Как перетащить кадр:

```
1. Select frame (click на нём)
2. Click and hold внутри кадра
3. ✋ Cursor → Hand
4. Drag to new position
5. Release mouse
6. ✅ Frame.X и Frame.Y обновлены!
```

### Пример:

```
Before drag:
Frame at (100, 50)
┌─────────┐
│ sprite  │ (100, 50)
└─────────┘

After drag (moved +50, +30):
              ┌─────────┐
              │ sprite  │ (150, 80)
              └─────────┘

New position: X=150, Y=80
```

### Cursor feedback:

```
Hover over frame → ✋ Hand cursor
Dragging → ✋ Hand cursor
Released → → Arrow cursor
```

---

## 📏 Resize Handles

### 8 handles вокруг кадра:

```
■───────■───────■
│               │
■       🎨      ■
│               │
■───────■───────■

Handles:
■ = resize handle (8×8 px белый квадрат)
```

### Handle positions:

```
TopLeft     Top      TopRight
   ■         ■          ■
   
Left                   Right
   ■        [  ]        ■

BottomLeft  Bottom   BottomRight
   ■         ■          ■
```

### Resize modes:

| Handle | Cursor | Action |
|--------|--------|--------|
| **Top-Left** | ↖ | Move top-left corner |
| **Top** | ↕ | Move top edge |
| **Top-Right** | ↗ | Move top-right corner |
| **Left** | ↔ | Move left edge |
| **Right** | ↔ | Move right edge |
| **Bottom-Left** | ↙ | Move bottom-left corner |
| **Bottom** | ↕ | Move bottom edge |
| **Bottom-Right** | ↘ | Move bottom-right corner |

### Как изменить размер:

```
1. Select frame
2. Hover over handle
3. ↗ Cursor changes (↖↗↙↘↔↕)
4. Click and drag handle
5. 📏 Frame resizes in real-time
6. Release mouse
7. ✅ Frame.Width и Frame.Height обновлены!
```

### Constraints:

```
Minimum size: 4×4 pixels
No maximum size
Real-time visual feedback
Snaps to pixel grid
```

---

## 🎨 Transparent Preview

### Шахматный паттерн (checkerboard)

```
Before (old):
┌─────────────┐
│             │  ← Solid dark gray
│   sprite    │
│             │
└─────────────┘

After (new):
□■□■□■□■□■□■□
■□■□■□■□■□■□■
□■  sprite  ■  ← Checkerboard pattern
■□■□■□■□■□■□■
□■□■□■□■□■□■□

□ = Light gray (#FFFFFF)
■ = Dark gray (#CCCCCC)
```

### Pattern specs:

```
Checker size: 10×10 pixels
Light color: #FFFFFF (white)
Dark color: #CCCCCC (light gray)
Pattern: Alternating squares
```

### Why checkerboard?

```
✅ Industry standard (Photoshop, GIMP, etc.)
✅ Clearly shows transparency
✅ Easy to see sprite edges
✅ Not distracting
✅ Works with any sprite colors
```

### Preview features:

```
✅ Checkerboard background
✅ Shows trimmed frame (GetTrimmedBounds)
✅ Green border = trimmed
✅ Cyan border = original
✅ Info text with size
```

---

## 🎯 Complete Workflow

### Scenario 1: Adjust single frame position

```
1. Open sprite sheet
2. Auto Detect
3. Select group
4. Click on frame in canvas
   → Frame selected (magenta border + handles)
5. Drag frame to new position
   → X: 100 → 120
   → Y: 50 → 60
6. ✅ Position updated!
7. Check in Frame Editor (right panel)
   → X: 120, Y: 60
```

### Scenario 2: Resize one frame

```
1. Select frame (click)
2. Hover over bottom-right handle
   → Cursor: ↘
3. Drag handle +20 pixels right and down
   → Width: 64 → 84
   → Height: 64 → 84
4. ✅ Size updated!
5. Preview shows new size
6. Frame Editor shows: Width: 84, Height: 84
```

### Scenario 3: Fine-tune overlapping frames

```
Problem:
Frame 1 and Frame 2 overlap

Solution:
1. Select Frame 1 (click)
2. Drag to move away (-10px)
3. Resize if needed (drag handle)
4. Select Frame 2 (click on it)
5. Adjust position
6. ✅ No more overlap!
```

### Scenario 4: Check transparency in preview

```
1. Apply Auto-Trim (with background removal)
2. Select frame in list
3. Preview shows:
   ✅ Checkerboard background
   ✅ Transparent areas visible
   ✅ Clean sprite edges
4. Verify background removal worked!
```

---

## ⌨️ Keyboard & Mouse Controls

### Mouse controls:

```
Left Click:
  On frame → Select frame
  On handle → Start resize
  Inside selected frame → Start drag
  On empty space → Select group

Left Click + Drag:
  Frame → Move frame
  Handle → Resize frame

Cursor feedback:
  Over frame → ✋ Hand
  Over handle → ↖↗↙↘↔↕ (directional)
  Default → → Arrow
```

### Modifier keys:

```
Ctrl + Click on group → Drag entire grid
Shift + Drag → Pan canvas (same as before)
```

---

## 🎨 Visual Indicators

### Frame states:

| State | Border | Handles | Cursor |
|-------|--------|---------|--------|
| **Normal** | Thin cyan | None | → Arrow |
| **Hovered** | Thin cyan | None | ✋ Hand |
| **Selected** | Thick magenta | 8 white squares | ✋ Hand |
| **Dragging** | Thick magenta | 8 white squares | ✋ Hand |
| **Resizing** | Thick magenta | 8 white squares | ↖↗↙↘↔↕ |

### Resize handles:

```
Size: 8×8 screen pixels
Fill: White (#FFFFFF)
Border: Black 1px
Position: Centered on corners/edges
```

### Preview indicators:

```
Background: Checkerboard (10px)
Border: Green (trimmed) or Cyan (original)
Info: Frame number + name + size
```

---

## 🔧 Technical Implementation

### Frame selection priority:

```csharp
OnPointerPressed:
1. Check resize handle (if frame selected)
   → Start resize mode
2. Check inside selected frame
   → Start drag mode
3. Check any frame in selected group
   → Select that frame
4. Check grid group
   → Select group
```

### Resize logic:

```csharp
switch (handle) {
    case TopLeft:
        frame.X += deltaX
        frame.Y += deltaY
        frame.Width -= deltaX
        frame.Height -= deltaY
        break
    case BottomRight:
        frame.Width += deltaX
        frame.Height += deltaY
        break
    // ... other handles
}

// Constrain
frame.Width = max(4, frame.Width)
frame.Height = max(4, frame.Height)
```

### Drag logic:

```csharp
OnPointerMoved (dragging):
    imagePos = ScreenToImageCoordinates(mousePos)
    frame.X = imagePos.X - dragOffset.X
    frame.Y = imagePos.Y - dragOffset.Y
    InvalidateVisual()
```

### Checkerboard pattern:

```csharp
DrawCheckerboardBackground(context, bounds):
    checkerSize = 10
    for y in 0..height step 10:
        for x in 0..width step 10:
            isLight = ((x/10) + (y/10)) % 2 == 0
            brush = isLight ? white : lightGray
            context.FillRectangle(brush, rect)
```

---

## 💡 Tips & Best Practices

### Editing tips:

```
✅ Select frame first (click)
✅ Use handles for precision resize
✅ Drag by center for position
✅ Check preview after changes
✅ Use Frame Editor for exact values
```

### When to use canvas editing:

```
✅ Visual adjustment
✅ Quick position tweaks
✅ Overlapping frame fixes
✅ Size approximation

❌ Precise pixel values → Use Frame Editor
❌ Batch operations → Use buttons
```

### Workflow integration:

```
Canvas editing ←→ Frame Editor
  (visual)         (precise)

Best workflow:
1. Auto Detect
2. Canvas: rough adjustment
3. Editor: fine-tune
4. Preview: verify
5. Export!
```

---

## 🎉 Summary

**Interactive Frame Editing provides:**

- 🎯 **Click to select** - instant frame selection
- ✋ **Drag to move** - intuitive positioning
- 📏 **8 resize handles** - precise size control
- 👀 **Visual feedback** - cursors, borders, handles
- 🎨 **Transparent preview** - checkerboard pattern
- ⚡ **Real-time updates** - instant visual response
- 🔄 **Bidirectional sync** - canvas ↔ Frame Editor

**Editing is now visual and intuitive!** 🖱️✨

**Keyboard-free frame editing - just click and drag!** 🚀
