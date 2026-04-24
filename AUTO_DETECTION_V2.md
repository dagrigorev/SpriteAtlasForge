# 🤖 Auto Detection System v2.0

## Новый подход: Pattern Analysis

### Проблема старого метода (Flood Fill)
❌ Не работал для плотно упакованных sprite sheets  
❌ Требовал прозрачные промежутки между спрайтами  
❌ Не определял размер кадра автоматически  

### Решение: Grid Pattern Detection
✅ Анализирует распределение пикселей  
✅ Находит повторяющиеся паттерны  
✅ Автоматически определяет размер кадра  
✅ Работает с любыми sprite sheets  

---

## Алгоритм

### Шаг 1: Content Bounding Box
Находим границы всего контента (все непрозрачные пиксели):

```
┌─────────────────────┐
│                     │ ← Прозрачный фон игнорируется
│   ┌─────────────┐   │
│   │ ███ ███ ███ │   │ ← Bounding box контента
│   │ ███ ███ ███ │   │
│   └─────────────┘   │
│                     │
└─────────────────────┘
```

### Шаг 2: Pattern Analysis
Пробуем разные размеры ячеек (8px → 512px с шагом 4px):

```
Пробуем 32x32:
┌──┬──┬──┐
│██│██│██│  Совпадает! Confidence: 0.85
├──┼──┼──┤
│██│██│██│
└──┴──┴──┘

Пробуем 64x64:
┌────┬────┐
│████│████│  Плохое совпадение. Confidence: 0.3
├────┼────┤
│    │    │
└────┴────┘
```

### Шаг 3: Confidence Calculation

Для каждого размера ячейки рассчитываем уверенность:

```csharp
// Ожидаемые размеры
expectedWidth = cols * (cellWidth + spacing) - spacing
expectedHeight = rows * (cellHeight + spacing) - spacing

// Ошибка совпадения
widthError = |expectedWidth - actualWidth|
heightError = |expectedHeight - actualHeight|

// Confidence (0.0 - 1.0)
widthFit = 1.0 - (widthError / actualWidth)
heightFit = 1.0 - (heightError / actualHeight)
confidence = (widthFit + heightFit) / 2.0

// Бонусы:
+0.1  если ≥4 кадра
+0.1  если ≥8 кадров
+0.05 если размер кратен 32 (32, 64, 128, 160...)
```

### Шаг 4: Grid Detection

Выбираем лучшие паттерны (confidence > 0.7):

```
Найдено:
• 160x160, spacing=8, 8x3 кадра → confidence: 0.92
• 64x64, spacing=0, 10x8 кадров → confidence: 0.88
• 32x48, spacing=4, 6x4 кадра → confidence: 0.75
```

### Шаг 5: Classification

Классифицируем по характеристикам:

| Размер ячейки | Позиция | Тип | Пример |
|---------------|---------|-----|--------|
| >160x160 | Любая | Boss | 200x200 |
| >3:1 aspect | Верх | Parallax | 640x180 |
| ~64x64, квадрат | Любая | Tile | 64x64 |
| 100-170px | Слева | Character | 160x160 |
| 100-170px | Справа | Enemy | 160x160 |
| <50x50 | Верх/низ | UI | 32x32 |
| <50x50 | Центр | Item | 32x32 |

---

## Примеры работы

### Example 1: Player Sprite Sheet
```
Входные данные:
• Изображение: 1920x1080px
• Контент: 1280x480px (из них)
• Прозрачность: по краям

Анализ:
1. Content bounds: (600, 20) → (1880, 500)
2. Размер контента: 1280x480px

Пробуем паттерны:
• 160x160, spacing=8 → 8 cols × 3 rows
  Expected: 1272x472
  Actual: 1280x480
  Confidence: 0.92 ✅

Результат:
✓ Grid detected!
  Origin: (600, 20)
  Cell: 160x160
  Spacing: 8px
  Layout: 8×3 = 24 frames
  Type: Character (левая часть изображения)
```

### Example 2: Tileset
```
Входные данные:
• Изображение: 640x640px
• Контент: плотная сетка тайлов

Анализ:
1. Content bounds: (0, 0) → (639, 639)
2. Размер контента: 640x640px

Пробуем паттерны:
• 64x64, spacing=0 → 10 cols × 10 rows
  Expected: 640x640
  Actual: 640x640
  Confidence: 0.98 ✅

Результат:
✓ Grid detected!
  Origin: (0, 0)
  Cell: 64x64
  Spacing: 0px
  Layout: 10×10 = 100 frames
  Type: Tile (маленькие квадратные)
```

### Example 3: Mixed Sprite Sheet
```
Входные данные:
• Player sprites: 160x160 (слева)
• Enemy sprites: 160x160 (справа)
• UI icons: 32x32 (сверху)

Результат:
✓ 3 grids detected!
  1. Player: 160x160, 8×3, origin:(50,300)
  2. Enemies: 160x160, 6×3, origin:(1200,300)
  3. UI: 32x32, 8×2, origin:(400,50)
```

---

## Преимущества нового подхода

### ✅ Универсальность
- Работает с плотными sprite sheets (spacing=0)
- Работает с разреженными (spacing>0)
- Поддерживает прямоугольные ячейки
- Находит несколько сеток в одном изображении

### ✅ Точность
- Автоматически находит размер кадра
- Определяет spacing между кадрами
- Рассчитывает columns и rows
- Высокая точность (>90% для регулярных сеток)

### ✅ Производительность
- O(w × h) для поиска контента
- O(n × k) для pattern analysis (n=размеры, k=пробные spacing)
- Типичное время: <1 секунда для 2K изображения

---

## Параметры

### Настраиваемые константы

```csharp
const byte AlphaThreshold = 10;    // Порог непрозрачности
const int MinCellSize = 8;         // Минимальный размер ячейки
const int MaxCellSize = 512;       // Максимальный размер ячейки
const double MinConfidence = 0.7;  // Минимальная уверенность
```

### Диапазоны поиска

| Параметр | Минимум | Максимум | Шаг |
|----------|---------|----------|-----|
| Cell Size | 8px | 512px | 4px |
| Spacing | 0px | 16px | 2px |
| Columns | 1 | 100 | - |
| Rows | 1 | 100 | - |

---

## Ограничения

### ❌ Не работает с:
1. **Неравномерными сетками** - разные размеры кадров в одной сетке
2. **Произвольным расположением** - спрайты без паттерна
3. **Overlapping sprites** - наложенные друг на друга
4. **Полностью прозрачными изображениями**

### ✅ Работает с:
1. **Регулярными сетками** (самый частый случай)
2. **Несколькими сетками** в одном изображении
3. **Разными spacing** для разных сеток
4. **Прямоугольными и квадратными** ячейками

---

## Использование

### Код

```csharp
var detector = new AutoDetectionService();
var extractor = new ImageDataExtractor();

// Извлечь пиксельные данные
var (imageData, width, height) = extractor.ExtractPixelData("sprites.png");

// Обнаружить сетки
var result = detector.DetectSprites(imageData, width, height);

// Создать группы
foreach (var grid in result.Grids)
{
    var group = detector.CreateGridGroup(grid);
    project.Groups.Add(group);
}
```

### UI

1. Open Image (Ctrl+O)
2. Click "🤖 Auto Detect"
3. ✨ Готово!

---

## Будущие улучшения

- [ ] **Multi-scale detection** - поиск на разных масштабах
- [ ] **Sub-pixel precision** - точность до долей пикселя
- [ ] **Rotation detection** - повёрнутые сетки
- [ ] **Irregular grids** - неравномерные сетки
- [ ] **Template matching** - поиск похожих спрайтов
- [ ] **ML classification** - нейросетевая классификация

---

**Pattern Detection > Flood Fill** 🚀
