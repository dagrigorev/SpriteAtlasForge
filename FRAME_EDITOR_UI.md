# 🎨 Frame Editor UI - Quick Guide

## Новый UI для редактирования индивидуальных кадров

---

## 📍 Где находится

**Правая панель → Individual Frames** (после Frame Optimization)

```
Right Panel:
├── Grid Definition
├── Frame Optimization
│   ├── ✂️ Auto-Trim Frames
│   ├── ↺ Reset Trim
│   └── 🎨 Toggle Background Removal
│
├── Individual Frames ← НОВЫЙ РАЗДЕЛ!
│   ├── Frame List (200px высота)
│   └── Selected Frame Editor
│
└── Frame Preview
```

---

## 📋 Frame List

### Отображает все кадры группы:

```
Frame List:
┌────────────────────────────────┐
│ Player_001   W:64  H:64  ■     │ ← Выбранный
│ Player_002   W:60  H:58  ■     │
│ Player_003   W:62  H:60  ■     │
│ ...                            │
└────────────────────────────────┘

Columns:
- Name: имя кадра
- W:XX: ширина
- H:XX: высота
- ■: цвет фона (если обнаружен)
```

### Действия:

```
1. Click на кадре → выбрать
2. Selected Frame Editor появляется ниже
3. Preview обновляется
```

---

## ✏️ Selected Frame Editor

### Показывается только когда кадр выбран

**Поля редактирования:**

### 1. Name (имя)
```
TextBox: "Player_001"
→ Можно изменить имя кадра
```

### 2. Position (позиция на sprite sheet)
```
X: [100]    Y: [50]
→ Координаты на исходном изображении
```

### 3. Size (размер)
```
Width: [64]    Height: [64]
→ Индивидуальный размер кадра
→ Можно изменить для каждого кадра!
```

### 4. Trim (обрезка в пикселях)
```
L: [10]  R: [12]  T: [5]  B: [8]
│        │        │       │
Left     Right    Top     Bottom

→ Сколько пикселей обрезать с каждой стороны
→ Установлено Auto-Trim автоматически
→ Можно настроить вручную!
```

### 5. Background Color (цвет фона)
```
TextBox: "#FF00FF"
→ Hex цвет фона (автоматически обнаружен)
→ Можно изменить вручную
→ Watermark: "#FF00FF" (пример)
```

### 6. Remove Background (удалить фон)
```
☑ Remove Background
→ Включить/выключить удаление фона для этого кадра
```

### 7. Effective Size (эффективный размер)
```
┌─────────────────────────────────┐
│ Effective: 64-10-12×64-5-8      │
│            ↑  ↑  ↑  ↑  ↑ ↑      │
│            W  L  R  H  T B      │
└─────────────────────────────────┘

Formula:
Effective Width = Width - TrimLeft - TrimRight
Effective Height = Height - TrimTop - TrimBottom

Example:
64 - 10 - 12 = 42 pixels
64 - 5 - 8 = 51 pixels
→ Effective size: 42×51
```

---

## 🎨 Preview Updates

### Preview теперь показывает обрезанный кадр!

**Визуальные индикаторы:**

```
Border color:
🟢 Green (Lime) = кадр обрезан (trim > 0)
🔵 Cyan = оригинальный размер (no trim)

Info text:
"1/48 - Player_001"
  ↓ with trim:
"1/48 - Player_001 (trimmed: 42×51)"
```

---

## 🎯 Workflow Examples

### Example 1: Просмотр auto-trim результатов

```
1. Select group (после Auto Detect)
2. Individual Frames → Frame List
3. Click на кадре
4. Selected Frame Editor показывает:
   - Trim values: L10 R12 T5 B8
   - Background: #00FF00
   - Effective size: 42×51
5. Preview показывает обрезанный кадр (зелёная рамка)
```

### Example 2: Ручная настройка размера

```
1. Select frame в Frame List
2. Change Width: 64 → 80
3. Change Height: 64 → 96
4. ✅ Кадр теперь 80×96 (индивидуальный размер!)
5. Preview обновляется
```

### Example 3: Настройка trim вручную

```
1. Select frame
2. Adjust Trim values:
   L: 10 → 15 (обрезать больше слева)
   R: 12 → 8 (обрезать меньше справа)
   T: 5 → 10
   B: 8 → 5
3. Effective size пересчитывается:
   64-15-8 × 64-10-5 = 41×49
4. Preview показывает новую обрезку
```

### Example 4: Изменение фона

```
1. Select frame
2. Background Color: "#00FF00" → "#FF00FF"
3. ☑ Remove Background: ON
4. Save/Export
5. ✅ Фон изменён на розовый
```

### Example 5: Отключение trim для одного кадра

```
1. Select specific frame
2. Reset trim values:
   L: 10 → 0
   R: 12 → 0
   T: 5 → 0
   B: 8 → 0
3. ✅ Этот кадр теперь без обрезки
4. Preview показывает полный размер (cyan рамка)
```

---

## 🔍 UI Layout

```
┌───────────────────────────────────┐
│ Individual Frames                 │
├───────────────────────────────────┤
│ Frame List (scrollable)           │
│ ┌─────────────────────────────┐   │
│ │ Player_001  W:64 H:64  ■    │◀──┤ Selected
│ │ Player_002  W:60 H:58  ■    │   │
│ │ Player_003  W:62 H:60  ■    │   │
│ │ ...                         │   │
│ └─────────────────────────────┘   │
├───────────────────────────────────┤
│ Selected Frame Properties:        │
│                                   │
│ Name:                             │
│ [Player_001________________]      │
│                                   │
│ X: [100]      Y: [50]             │
│                                   │
│ Width: [64]   Height: [64]        │
│                                   │
│ Trim (pixels):                    │
│ L:[10] R:[12] T:[5] B:[8]         │
│                                   │
│ Background Color:                 │
│ [#00FF00___________________]      │
│                                   │
│ ☑ Remove Background               │
│                                   │
│ ┌───────────────────────────┐     │
│ │ Effective: 64-10-12×64-5-8│     │
│ └───────────────────────────┘     │
└───────────────────────────────────┘
```

---

## ⌨️ Keyboard Navigation

```
Frame List:
↑/↓ Arrow keys: Navigate frames
Enter: Edit selected frame name
Tab: Move between fields in editor

Editor:
Tab: Next field
Shift+Tab: Previous field
+/- (NumericUpDown): Increment/Decrement
```

---

## 💡 Tips

### Batch vs Individual

```
Batch operations (кнопки):
✂️ Auto-Trim Frames → все кадры группы
↺ Reset Trim → все кадры группы
🎨 Toggle BG Removal → все кадры группы

Individual (editor):
→ Один кадр за раз
→ Точная настройка
→ Разные размеры
```

### When to use Individual Editor

```
✅ Use when:
- Один кадр имеет другой размер
- Нужна точная настройка trim
- Кадр имеет другой фон
- Нужно проверить конкретный кадр

❌ Don't use when:
- Все кадры одинаковые
- Нужна массовая операция
→ Используйте batch кнопки!
```

### Preview Tips

```
Preview indicators:
🟢 Green border = trimmed
🔵 Cyan border = original

Info text shows:
- Frame number: 1/48
- Frame name: Player_001
- Size (if trimmed): (trimmed: 42×51)

Play animation:
▶ Play button → see all frames
→ Check trim consistency
→ Verify sizes
```

---

## 🎉 Summary

**Frame Editor UI provides:**
- 📋 **Frame List** - все кадры группы
- ✏️ **Individual Editor** - точная настройка каждого кадра
- 📐 **Size control** - индивидуальные размеры
- ✂️ **Trim control** - точная обрезка
- 🎨 **Background control** - цвет и удаление
- 👁️ **Preview** - визуальная проверка
- ⚡ **Real-time updates** - мгновенная обратная связь

**Now you have full control over individual frames!** ✨🎨
