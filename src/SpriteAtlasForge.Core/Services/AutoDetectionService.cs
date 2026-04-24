using SpriteAtlasForge.Core.Models;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriteAtlasForge.Core.Services;

/// <summary>
/// OpenCV-based sprite detection - 100-1000x faster than pixel loops
/// Uses professional computer vision algorithms
/// </summary>
public class AutoDetectionService
{
    private const double SimilarityThreshold = 0.80;  // Lowered from 0.85
    private const int MinSpriteArea = 16; // Lowered from 64 (4x4 minimum)

    public class DetectionResult
    {
        public List<SpriteCluster> Clusters { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
        public int TotalFrames { get; set; }
    }

    public class SpriteCluster
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<SpriteInstance> Instances { get; set; } = new();
        public GridGroupType Type { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class SpriteInstance
    {
        public int X { get; set; }
        public int Y { get; set; }
        public double Similarity { get; set; }
    }

    /// <summary>
    /// Main detection using OpenCV - FAST!
    /// </summary>
    /// <param name="imageData">RGBA image data</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <param name="roi">Optional region of interest (null = whole image)</param>
    public DetectionResult DetectSprites(byte[] imageData, int width, int height, Rect? roi = null)
    {
        var result = new DetectionResult();

        try
        {
            System.Diagnostics.Debug.WriteLine($"[AutoDetect] Starting detection on {width}x{height} image");
            
            // Convert to OpenCV Mat (zero-copy if possible)
            using var mat = ByteArrayToMat(imageData, width, height);
            
            if (mat.Empty())
            {
                result.Summary = "Failed to load image";
                return result;
            }

            // Apply ROI if specified
            Mat workingMat;
            int offsetX = 0, offsetY = 0;
            
            if (roi.HasValue)
            {
                var roiRect = new Rect(
                    Math.Max(0, roi.Value.X),
                    Math.Max(0, roi.Value.Y),
                    Math.Min(roi.Value.Width, width - roi.Value.X),
                    Math.Min(roi.Value.Height, height - roi.Value.Y));
                    
                workingMat = new Mat(mat, roiRect);
                offsetX = roiRect.X;
                offsetY = roiRect.Y;
                System.Diagnostics.Debug.WriteLine($"[AutoDetect] Using ROI: {roiRect}");
            }
            else
            {
                workingMat = mat;
            }

            // Extract alpha channel
            using var alpha = ExtractAlphaChannel(workingMat);

            // TRY METHOD 1: Connected Components (works for isolated sprites)
            System.Diagnostics.Debug.WriteLine($"[AutoDetect] Method 1: Connected Components");
            var components = FindConnectedComponents(alpha);
            System.Diagnostics.Debug.WriteLine($"[AutoDetect] Found {components.Count} components");
            
            List<SpriteCluster> clusters;
            
            if (components.Count >= 2)
            {
                // Method 1 worked - use template matching
                var templates = ExtractSpriteTemplates(workingMat, alpha, components);
                System.Diagnostics.Debug.WriteLine($"[AutoDetect] Extracted {templates.Count} templates");
                
                clusters = ClusterSimilarSprites(workingMat, alpha, templates);
                System.Diagnostics.Debug.WriteLine($"[AutoDetect] Method 1 created {clusters.Count} clusters");
            }
            else
            {
                // Method 1 failed - try Method 2: Grid Scanning (works for dense sprite sheets)
                System.Diagnostics.Debug.WriteLine($"[AutoDetect] Method 2: Grid Scanning (fallback)");
                clusters = DetectByGridScanning(workingMat, alpha);
                System.Diagnostics.Debug.WriteLine($"[AutoDetect] Method 2 created {clusters.Count} clusters");
            }

            // Adjust coordinates if ROI was used
            if (roi.HasValue)
            {
                foreach (var cluster in clusters)
                {
                    foreach (var instance in cluster.Instances)
                    {
                        instance.X += offsetX;
                        instance.Y += offsetY;
                    }
                }
            }

            // Classify
            foreach (var cluster in clusters)
            {
                ClassifyCluster(cluster, width, height);
            }

            result.Clusters = clusters;
            result.TotalFrames = clusters.Sum(c => c.Instances.Count);
            result.Summary = $"Found {clusters.Count} sprite type(s) with {result.TotalFrames} frames";
            
            System.Diagnostics.Debug.WriteLine($"[AutoDetect] Final: {result.Summary}");

            if (roi.HasValue)
                workingMat.Dispose();

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoDetect] ERROR: {ex.Message}");
            result.Summary = $"Detection error: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Convert RGBA byte array to OpenCV Mat
    /// </summary>
    private Mat ByteArrayToMat(byte[] imageData, int width, int height)
    {
        // Create Mat from byte array (RGBA)
        var mat = new Mat(height, width, MatType.CV_8UC4, imageData);
        return mat.Clone(); // Clone to ensure data ownership
    }

    /// <summary>
    /// Extract alpha channel
    /// </summary>
    private Mat ExtractAlphaChannel(Mat rgba)
    {
        var channels = rgba.Split();
        var alpha = channels[3].Clone(); // Alpha is 4th channel
        
        foreach (var ch in channels)
            ch.Dispose();
        
        return alpha;
    }

    /// <summary>
    /// Find connected components using OpenCV (FAST!)
    /// </summary>
    private List<Rect> FindConnectedComponents(Mat alpha)
    {
        var components = new List<Rect>();

        // Threshold alpha channel (lowered to 5 for better detection)
        using var binary = new Mat();
        Cv2.Threshold(alpha, binary, 5, 255, ThresholdTypes.Binary);

        // Find contours (super fast native code)
        Cv2.FindContours(
            binary,
            out var contours,
            out var hierarchy,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        // Get bounding rectangles
        foreach (var contour in contours)
        {
            var rect = Cv2.BoundingRect(contour);
            
            // Filter by size
            if (rect.Width * rect.Height >= MinSpriteArea)
            {
                components.Add(rect);
            }
            
            //contour.Dispose();
        }

        return components;
    }

    private class SpriteTemplate
    {
        public Rect BoundingBox { get; set; }
        public Mat Image { get; set; } = new Mat();
        public Mat Mask { get; set; } = new Mat();
        public double[] Histogram { get; set; } = Array.Empty<double>();
    }

    /// <summary>
    /// Extract sprite templates with histograms
    /// </summary>
    private List<SpriteTemplate> ExtractSpriteTemplates(Mat image, Mat alpha, List<Rect> components)
    {
        var templates = new List<SpriteTemplate>();

        // Group by size (±10% tolerance)
        var sizeGroups = components
            .GroupBy(r => (r.Width / 10 * 10, r.Height / 10 * 10))
            .Where(g => g.Count() >= 2) // Only sizes with multiple instances
            .ToList();

        foreach (var group in sizeGroups)
        {
            // Take first instance as template
            var rect = group.First();
            
            // Extract template region
            using var templateImg = new Mat(image, rect);
            using var templateMask = new Mat(alpha, rect);

            // Calculate color histogram for similarity matching
            var histogram = CalculateHistogram(templateImg, templateMask);

            templates.Add(new SpriteTemplate
            {
                BoundingBox = rect,
                Image = templateImg.Clone(),
                Mask = templateMask.Clone(),
                Histogram = histogram
            });
        }

        return templates;
    }

    /// <summary>
    /// Calculate color histogram (for fast similarity check)
    /// </summary>
    private double[] CalculateHistogram(Mat image, Mat mask)
    {
        // Convert to HSV for better color representation
        using var hsv = new Mat();
        Cv2.CvtColor(image, hsv, ColorConversionCodes.BGRA2BGR);
        Cv2.CvtColor(hsv, hsv, ColorConversionCodes.BGR2HSV);

        // Calculate histogram
        using var hist = new Mat();
        int[] histSize = { 50, 60 }; // H and S bins
        Rangef[] ranges = { new Rangef(0, 180), new Rangef(0, 256) };
        int[] channels = { 0, 1 }; // H and S channels

        Cv2.CalcHist(
            new[] { hsv },
            channels,
            mask,
            hist,
            2,
            histSize,
            ranges);

        // Normalize
        Cv2.Normalize(hist, hist, 0, 1, NormTypes.MinMax);

        // Convert to array
        var histArray = new double[histSize[0] * histSize[1]];
        for (int i = 0; i < histSize[0]; i++)
        {
            for (int j = 0; j < histSize[1]; j++)
            {
                histArray[i * histSize[1] + j] = hist.At<float>(i, j);
            }
        }

        return histArray;
    }

    /// <summary>
    /// Cluster similar sprites using template matching
    /// </summary>
    private List<SpriteCluster> ClusterSimilarSprites(Mat image, Mat alpha, List<SpriteTemplate> templates)
    {
        var clusters = new List<SpriteCluster>();

        foreach (var template in templates)
        {
            var cluster = new SpriteCluster
            {
                Width = template.BoundingBox.Width,
                Height = template.BoundingBox.Height,
                Instances = new List<SpriteInstance>()
            };

            // Template matching (OpenCV native - super fast!)
            using var result = new Mat();
            Cv2.MatchTemplate(
                image,
                template.Image,
                result,
                TemplateMatchModes.CCoeffNormed,
                template.Mask);

            // Find all matches above threshold
            while (true)
            {
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out var maxLoc);

                if (maxVal < SimilarityThreshold)
                    break;

                cluster.Instances.Add(new SpriteInstance
                {
                    X = maxLoc.X,
                    Y = maxLoc.Y,
                    Similarity = maxVal
                });

                // Mark this region as used (draw rectangle with -1)
                Cv2.Rectangle(
                    result,
                    new Rect(
                        maxLoc.X - template.BoundingBox.Width / 2,
                        maxLoc.Y - template.BoundingBox.Height / 2,
                        template.BoundingBox.Width,
                        template.BoundingBox.Height),
                    Scalar.All(-1),
                    -1);
            }

            if (cluster.Instances.Count >= 2)
            {
                clusters.Add(cluster);
            }

            template.Image.Dispose();
            template.Mask.Dispose();
        }

        return clusters;
    }

    /// <summary>
    /// FALLBACK METHOD: Grid scanning for dense sprite sheets
    /// Used when Connected Components fails
    /// </summary>
    private List<SpriteCluster> DetectByGridScanning(Mat image, Mat alpha)
    {
        var allCandidates = new List<(int x, int y, int size)>();
        
        // Try different sprite sizes
        var sizes = new[] { 16, 24, 32, 48, 64, 96, 128, 160, 192 };
        
        foreach (var size in sizes)
        {
            if (size > alpha.Width || size > alpha.Height)
                continue;
            
            // Scan with step = size (no overlap for speed)
            for (int y = 0; y <= alpha.Height - size; y += size)
            {
                for (int x = 0; x <= alpha.Width - size; x += size)
                {
                    // Check if this region has content
                    var roi = new Rect(x, y, size, size);
                    using var regionAlpha = new Mat(alpha, roi);
                    
                    // Count opaque pixels
                    int opaqueCount = 0;
                    int totalPixels = size * size;
                    
                    for (int ry = 0; ry < size && opaqueCount < totalPixels * 0.1; ry++)
                    {
                        for (int rx = 0; rx < size; rx++)
                        {
                            if (regionAlpha.At<byte>(ry, rx) > 10)
                                opaqueCount++;
                        }
                    }
                    
                    // If region has at least 10% opaque pixels, it's a candidate
                    if (opaqueCount >= totalPixels * 0.1)
                    {
                        allCandidates.Add((x, y, size));
                    }
                }
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"[GridScanning] Found {allCandidates.Count} candidate regions");
        
        if (allCandidates.Count == 0)
            return new List<SpriteCluster>();
        
        // Group by size (±10% tolerance)
        var clusters = new List<SpriteCluster>();
        var groupedBySize = allCandidates
            .GroupBy(c => (c.size / 10) * 10)  // Round to nearest 10
            .Where(g => g.Count() >= 2)  // At least 2 instances
            .ToList();
        
        foreach (var group in groupedBySize)
        {
            var avgSize = (int)group.Average(c => c.size);
            
            var cluster = new SpriteCluster
            {
                Width = avgSize,
                Height = avgSize,
                Instances = group.Select(c => new SpriteInstance
                {
                    X = c.x,
                    Y = c.y,
                    Similarity = 1.0
                }).ToList()
            };
            
            clusters.Add(cluster);
            System.Diagnostics.Debug.WriteLine($"[GridScanning] Cluster: {cluster.Width}x{cluster.Height} with {cluster.Instances.Count} instances");
        }
        
        return clusters;
    }

    /// <summary>
    /// Classify cluster by characteristics
    /// </summary>
    private void ClassifyCluster(SpriteCluster cluster, int imageWidth, int imageHeight)
    {
        int area = cluster.Width * cluster.Height;
        double aspectRatio = (double)cluster.Width / cluster.Height;
        
        var avgX = cluster.Instances.Average(i => i.X);
        var avgY = cluster.Instances.Average(i => i.Y);
        
        var xPositions = cluster.Instances.Select(i => i.X).Distinct().Count();
        var yPositions = cluster.Instances.Select(i => i.Y).Distinct().Count();
        
        cluster.Name = $"{xPositions}x{yPositions}";

        // Classification logic
        if (area > 25000)
        {
            cluster.Type = GridGroupType.Boss;
            cluster.Name = $"Boss {cluster.Name}";
        }
        else if (aspectRatio > 3.0 && avgY < imageHeight * 0.3)
        {
            cluster.Type = GridGroupType.Parallax;
            cluster.Name = $"Parallax {cluster.Name}";
        }
        else if (area < 4096 && aspectRatio > 0.8 && aspectRatio < 1.2)
        {
            cluster.Type = GridGroupType.Tile;
            cluster.Name = $"Tiles {cluster.Name}";
        }
        else if (area >= 10000 && area <= 30000)
        {
            if (avgX < imageWidth * 0.5)
            {
                cluster.Type = GridGroupType.Character;
                cluster.Name = $"Player {cluster.Name}";
            }
            else
            {
                cluster.Type = GridGroupType.Enemy;
                cluster.Name = $"Enemy {cluster.Name}";
            }
        }
        else if (area < 2500)
        {
            cluster.Type = GridGroupType.Item;
            cluster.Name = $"Items {cluster.Name}";
        }
        else
        {
            cluster.Type = GridGroupType.Effect;
            cluster.Name = $"Effects {cluster.Name}";
        }
    }

    /// <summary>
    /// Convert cluster to GridGroup with optional background detection
    /// </summary>
    public GridGroup CreateGridGroup(SpriteCluster cluster, byte[]? imageData = null, int imageWidth = 0, int imageHeight = 0)
    {
        var group = new GridGroup(cluster.Name, cluster.Type);

        // Sort instances
        var sorted = cluster.Instances
            .OrderBy(i => i.Y)
            .ThenBy(i => i.X)
            .ToList();

        // Find unique positions
        var xPositions = sorted.Select(i => i.X).Distinct().OrderBy(x => x).ToList();
        var yPositions = sorted.Select(i => i.Y).Distinct().OrderBy(y => y).ToList();

        int originX = xPositions.First();
        int originY = yPositions.First();

        // Calculate spacing
        int spacing = 0;
        if (xPositions.Count > 1)
        {
            var xGaps = xPositions.Zip(xPositions.Skip(1), (a, b) => b - a - cluster.Width)
                .Where(gap => gap >= 0);
            spacing = xGaps.Any() ? (int)xGaps.Average() : 0;
        }

        group.GridDefinition = new GridDefinition
        {
            OriginX = originX,
            OriginY = originY,
            CellWidth = cluster.Width,
            CellHeight = cluster.Height,
            Spacing = spacing,
            Padding = 0,
            Columns = xPositions.Count,
            Rows = yPositions.Count
        };

        // Generate frames
        for (int i = 0; i < sorted.Count; i++)
        {
            var instance = sorted[i];
            var frame = new SpriteFrame(
                $"{cluster.Name}_{i:D3}",
                instance.X,
                instance.Y,
                cluster.Width,
                cluster.Height
            );
            group.Frames.Add(frame);
        }

        // Auto-detect background and apply trim if image data provided
        if (imageData != null && imageWidth > 0 && imageHeight > 0 && group.Frames.Count > 0)
        {
            var bgService = new BackgroundRemovalService();
            bgService.AutoTrimFrames(imageData, imageWidth, imageHeight, group.Frames, detectBackground: true);
            
            System.Diagnostics.Debug.WriteLine($"[AutoDetect] Applied auto-trim to {group.Frames.Count} frames in {group.Name}");
        }

        return group;
    }
}
