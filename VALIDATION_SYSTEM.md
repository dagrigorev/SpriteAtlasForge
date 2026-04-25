# ⚠️ Validation System - Complete Guide

## Overview

SpriteAtlasForge включает мощную систему валидации которая проверяет проект на ошибки и предупреждения в реальном времени.

---

## 🎯 Где отображаются ошибки

### 1. Панель валидации (Правая панель)

```
┌─────────────────────────────────┐
│ ⚠️ Validation            [3]    │
├─────────────────────────────────┤
│ [🔍 Validate Project]           │
├─────────────────────────────────┤
│ ❌ Frame 'idle_01' is outside   │
│    image bounds                 │
│    Group: Characters → Frame:   │
│    idle_01                      │
├─────────────────────────────────┤
│ ⚠️ Group has no frames          │
│    Group: Enemies               │
├─────────────────────────────────┤
│ ℹ️ Group export is disabled     │
│    Group: Disabled_Group        │
└─────────────────────────────────┘
```

### 2. Панель логов (Под canvas)

```
12:34:56.123 ℹ️  Validation  Starting project validation...
12:34:56.234 ❌ Validation  Frame 'idle_01' is outside image bounds
12:34:56.345 ⚠️  Validation  Group has no frames (Group: Enemies)
12:34:56.456 ⚠️  Validation  Validation found 1 error(s), 2 warning(s)
```

### 3. Status Bar (Внизу окна)

```
⚠️ Validation: 1 error(s), 2 warning(s)
```

---

## 📋 Типы сообщений валидации

### ❌ Error (Критические ошибки)
**Цвет:** Красный (#E81123)  
**Иконка:** ❌  
**Значение:** Экспорт невозможен до исправления

**Примеры:**
- `Frame 'sprite_01' has invalid dimensions`
- `Frame 'player_jump' is outside image bounds (2048,512,64,64)`
- `Group name is empty`
- `Animation 'run' has invalid FPS: 0`
- `Parallax layer has no name`

### ⚠️ Warning (Предупреждения)
**Цвет:** Оранжевый (#FFB900)  
**Иконка:** ⚠️  
**Значение:** Экспорт возможен, но есть проблемы

**Примеры:**
- `Group has no frames`
- `All frames are disabled`
- `Frames 'sprite_1' and 'sprite_2' overlap`
- `Character group has no animations defined`
- `3 tile frame(s) have no tile kind assigned`
- `Parallax layer 'bg' has unusual scroll factor: 15`

### ℹ️ Info (Информация)
**Цвет:** Синий (#3794FF)  
**Иконка:** ℹ️  
**Значение:** Просто к сведению

**Примеры:**
- `Group export is disabled`
- `Animation 'idle' has no frames`

---

## 🔍 Что проверяется

### Project Level
```
✓ Source image loaded
✓ At least one group exists
```

### Group Level
```
✓ Group has name
✓ Group has frames (if export enabled)
✓ Enabled frames exist
✓ No overlapping frames (if AllowOverlap=false)
```

### Frame Level
```
✓ Frame has valid dimensions (width > 0, height > 0)
✓ Frame is within image bounds
✓ Frame coordinates are positive
```

### Type-Specific Validation

#### Character/Enemy/Boss Groups
```
✓ Has animations defined
✓ Each animation has frames
✓ Animation FPS > 0
```

#### Tile Groups
```
✓ Tiles have TileKind assigned
```

#### Parallax Groups
```
✓ Has layers defined
✓ Each layer has name
✓ ScrollFactor in reasonable range (0-10)
✓ Opacity in valid range (0-1)
```

---

## 🎮 Как использовать

### 1. Автоматическая валидация при экспорте

```csharp
Export Atlas (Ctrl+E)
  ↓
Автоматически запускается валидация
  ↓
Если есть ошибки:
  ❌ Экспорт отменён
  ⚠️ Показаны все проблемы
  
Если только warnings:
  ✅ Экспорт продолжается
  ℹ️ Warnings логируются
```

### 2. Ручная валидация

```
Tools → Validate Project (Ctrl+Shift+V)
или
Click "🔍 Validate Project" в правой панели
```

### 3. Просмотр результатов

**В панели валидации:**
- Все сообщения с иконками и цветами
- Группировка по severity
- Показывает Group и Frame где проблема

**В панели логов:**
- Timestamp каждой проблемы
- Фильтруемые по уровню
- История всех валидаций

---

## 📊 Примеры валидации

### Пример 1: Успешная валидация

```
Validation Panel:
┌──────────────────────────┐
│ ⚠️ Validation         [0]│
├──────────────────────────┤
│ [🔍 Validate Project]    │
├──────────────────────────┤
│ ✓ No validation issues   │
└──────────────────────────┘

Logs Panel:
12:00:00.000 ℹ️  Validation  Starting project validation...
12:00:00.100 ✅ Validation  Validation passed - no issues found

Status Bar:
✓ Validation passed - no issues found
```

### Пример 2: Ошибки найдены

```
Validation Panel:
┌────────────────────────────────────┐
│ ⚠️ Validation                  [5] │
├────────────────────────────────────┤
│ [🔍 Validate Project]              │
├────────────────────────────────────┤
│ ❌ Frame 'jump_03' is outside      │
│    image bounds (2048,0,64,64)     │
│    Group: Player → Frame: jump_03  │
├────────────────────────────────────┤
│ ⚠️ Group has no frames             │
│    Group: Enemies                  │
├────────────────────────────────────┤
│ ⚠️ All frames are disabled         │
│    Group: Disabled_Sprites         │
├────────────────────────────────────┤
│ ℹ️ Group export is disabled        │
│    Group: Work_In_Progress         │
├────────────────────────────────────┤
│ ⚠️ Character group has no          │
│    animations defined              │
│    Group: NPC                      │
└────────────────────────────────────┘

Logs Panel:
12:01:00.000 ℹ️  Validation  Starting project validation...
12:01:00.050 ❌ Validation  Frame 'jump_03' is outside image bounds (Group: Player → Frame: jump_03)
12:01:00.100 ⚠️  Validation  Group has no frames (Group: Enemies)
12:01:00.150 ⚠️  Validation  All frames are disabled (Group: Disabled_Sprites)
12:01:00.200 ℹ️  Validation  Group export is disabled (Group: Work_In_Progress)
12:01:00.250 ⚠️  Validation  Character group has no animations defined (Group: NPC)
12:01:00.300 ⚠️  Validation  Validation found 1 error(s), 3 warning(s)

Status Bar:
⚠️ Validation: 1 error(s), 3 warning(s)
```

### Пример 3: Валидация при экспорте

```
Logs Panel:
12:02:00.000 ℹ️  Export  Starting export to: atlas.json
12:02:00.050 ℹ️  Validation  Starting project validation...
12:02:00.100 ❌ Validation  Frame 'sprite_99' is outside image bounds
12:02:00.150 ❌ Export  Validation failed

Status Bar:
❌ Validation failed: 1 error(s)

Result:
Export cancelled, fix errors first!
```

---

## 🔧 Исправление ошибок

### ❌ "Frame is outside image bounds"

**Причина:** Frame координаты выходят за границы изображения

**Исправление:**
1. Выберите проблемный frame
2. Проверьте координаты (X, Y, Width, Height)
3. Убедитесь что X+Width ≤ Image Width
4. Убедитесь что Y+Height ≤ Image Height
5. Исправьте координаты вручную или удалите frame

### ⚠️ "Group has no frames"

**Причина:** Группа создана но в ней нет ни одного frame

**Исправление:**
1. Используйте Auto Detect для автоматического создания frames
2. Или используйте Generate Frames с Grid Settings
3. Или удалите пустую группу

### ⚠️ "All frames are disabled"

**Причина:** Все frames в группе отключены (Enabled=false)

**Исправление:**
1. Включите нужные frames
2. Или включите Export для группы
3. Или удалите группу если она не нужна

### ❌ "Group name is empty"

**Причина:** У группы нет имени

**Исправление:**
1. Выберите группу
2. Введите имя в поле "Name"

---

## 🎨 UI Элементы

### Validation Panel Components

```xml
<!-- Header with badge -->
⚠️ Validation    [5]
                  ↑
         Number of issues

<!-- Validate Button -->
[🔍 Validate Project]
  ↓
Runs validation and updates panel

<!-- Message Item -->
┌────────────────────────────┐
│ ❌ Message text here        │ ← Color coded
│    Group: X → Frame: Y     │ ← Source location
└────────────────────────────┘

<!-- Empty State -->
✓ No validation issues
```

---

## 💡 Best Practices

### 1. Validate Before Export
```
Always run validation before exporting:
Ctrl+Shift+V → Check for issues → Fix → Export
```

### 2. Fix Errors First
```
Priority order:
1. ❌ Errors (must fix)
2. ⚠️ Warnings (should fix)
3. ℹ️ Info (optional)
```

### 3. Use Auto Detect
```
Auto Detect helps prevent:
- Frames outside bounds
- Invalid dimensions
- Overlapping frames
```

### 4. Check Logs
```
Logs show validation history:
- What was checked
- When errors occurred
- What was fixed
```

---

## 🚀 Advanced Features

### Validation on Save
Currently validation runs on:
- Manual trigger (Ctrl+Shift+V)
- Before export (automatic)

Future: Auto-validate on save

### Custom Validation Rules
Future feature: Add custom validation rules

### Batch Validation
Future feature: Validate multiple projects

---

## 📖 Summary

**Validation система помогает:**

✅ Найти ошибки до экспорта  
✅ Увидеть предупреждения о проблемах  
✅ Убедиться что проект корректный  
✅ Получить чёткие указания где проблема  
✅ Логировать историю валидации  

**Три места где видны ошибки:**
1. 📋 Validation Panel (правая панель)
2. 📝 Log Panel (под canvas)
3. 📊 Status Bar (внизу)

**Используйте Ctrl+Shift+V для валидации!** ⚠️✨
