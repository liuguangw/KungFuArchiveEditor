using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using KungFuArchiveEditor.GameConfig;
using KungFuArchiveEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace KungFuArchiveEditor.Views;
public partial class JingmaiView : UserControl
{
    /// <summary>
    /// 六边形的边长
    /// </summary>
    const double nodeSize = 40;
    const double iconWidth = 32;
    private JingmaiViewModel? JingmaiVm;
    /// <summary>
    /// 预先加载的图标资源
    /// </summary>
    private Bitmap[] ImageSourceList;
    /// <summary>
    /// 绘制的穴位图标记录 posKey => (nodeType, image)
    /// </summary>
    private readonly Dictionary<JingmaiNodePos, (int, Image)> ImageNodes = new();
    /// <summary>
    /// 当前选择的工具类型
    /// </summary>
    private int currentToolType = 2;
    public JingmaiView()
    {
        InitializeComponent();
        ImageSourceList = new Bitmap[]
        {
            new Bitmap(AssetLoader.Open(new Uri($"avares://KungFuArchiveEditor/Assets/icon/c1.png"))),
            new Bitmap(AssetLoader.Open(new Uri($"avares://KungFuArchiveEditor/Assets/icon/c2.png"))),
            new Bitmap(AssetLoader.Open(new Uri($"avares://KungFuArchiveEditor/Assets/icon/c3.png"))),
            new Bitmap(AssetLoader.Open(new Uri($"avares://KungFuArchiveEditor/Assets/icon/c4.png"))),
        };
    }

    private void SelectTool(Button btn, int nodeType)
    {
        var parentNode = btn.Parent;
        if (parentNode is Panel panel)
        {
            foreach (var element in panel.Children)
            {
                if (element is Button btnElement)
                {
                    btnElement.Classes.Clear();
                }
            }
        }
        btn.Classes.Add("active");
        currentToolType = nodeType;
    }

    private void ToolC2Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            SelectTool(btn, 2);
        }
    }

    private void ToolC3Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            SelectTool(btn, 3);
        }
    }

    private void ToolC4Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            SelectTool(btn, 4);
        }
    }

    private void ToolClearClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            SelectTool(btn, 0);
        }
    }

    private void JingmaiView_Loaded(object? sender, RoutedEventArgs e)
    {
        //只需要调用一次
        if (JingmaiVm != null)
        {
            return;
        }
        if (DataContext is JingmaiViewModel viewModel)
        {
            JingmaiVm = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DrawMap();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "JingmaiMap")
        {
            DrawMap();
        }
    }

    /// <summary>
    /// 重新绘制
    /// </summary>
    private void DrawMap()
    {
        if (JingmaiVm != null)
        {
            if (JingmaiVm.JingmaiMap.Count > 0)
            {
                DrawMap(JingmaiVm.SelectedMapSize.Value, JingmaiVm.JingmaiMap);
            }
        }
    }

    private void DrawMap(int mapSize, Dictionary<JingmaiNodePos, JingmaiNodeConfig> mapData)
    {
        //Debug.WriteLine($"DrawMap mapSize={mapSize}");
        MainCanvas.Height = (mapSize * 2 + 1) * Math.Sqrt(3) * nodeSize;
        MainCanvas.Width = (3 * mapSize + 2) * nodeSize;
        //清理
        MainCanvas.Children.Clear();
        ImageNodes.Clear();
        //绘制时计算穴位数量
        JingmaiVm!.SlotCount = 0;
        int rBegin = 0;
        for (int q = -mapSize; q <= 0; q++)
        {
            for (int r = rBegin; r <= mapSize; r++)
            {
                DrawPolygon(MainCanvas, mapSize, q, r, mapData);
                //Debug.Write($"q={q},r={r} ");
            }
            //Debug.WriteLine("\n=====");
            rBegin--;
        }
        rBegin = -mapSize;
        int rEnd = mapSize - 1;
        for (int q = 1; q <= mapSize; q++)
        {
            for (int r = rBegin; r <= rEnd; r++)
            {
                DrawPolygon(MainCanvas, mapSize, q, r, mapData);
                //Debug.Write($"q={q},r={r} ");
            }
            //Debug.WriteLine("\n=====");
            rEnd--;
        }
    }

    /// <summary>
    /// 用于调试时显示格子坐标
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private TextBlock BuildLabel(string text)
    {
        var label = new TextBlock();
        label.Text = text;
        label.Width = Math.Sqrt(3) * nodeSize;
        label.Height = nodeSize;
        label.LineHeight = nodeSize;
        label.FontSize = 16;
        label.Foreground = Brushes.Red;
        //label.Background = Brushes.GreenYellow;
        label.TextAlignment = TextAlignment.Center;
        return label;
    }

    /// <summary>
    /// 绘制六边形
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="mapSize"></param>
    /// <param name="q"></param>
    /// <param name="r"></param>
    /// <param name="mapData">参考数据,根据此数据判断当前坐标是否是穴位</param>
    private void DrawPolygon(Canvas canvas, int mapSize, int q, int r, Dictionary<JingmaiNodePos, JingmaiNodeConfig> mapData)
    {
        var polygon = BuildPolygon();
        //六边形的左上角的坐标
        double x = (r + mapSize) * 3 * nodeSize / 2;
        //double y = (mapSize-q)* Math.Sqrt(3) * nodeSize;
        //y-=r* Math.Sqrt(3) * nodeSize / 2;
        double y = (2 * (mapSize - q) - r) * Math.Sqrt(3) * nodeSize / 2;
        Canvas.SetLeft(polygon, x);
        Canvas.SetTop(polygon, y);
        canvas.Children.Add(polygon);
        //
        var posKey = new JingmaiNodePos(q, r);
        AttachEventAction(canvas, polygon, x, y, posKey);
        //debug
        /*var label = BuildLabel(posKey.ToString());
        Canvas.SetLeft(label, x);
        Canvas.SetTop(label, y + (Math.Sqrt(3) * nodeSize - label.FontSize) / 2);
        canvas.Children.Add(label);
        AttachEventAction(canvas, polygon, label, x, y, posKey);*/
        //丹田
        if (posKey.IsZero())
        {
            var image = DrawIconImage(canvas, x, y, 1);
            ImageNodes.Add(posKey, (1, image));
            AttachEventAction(canvas, polygon, image, x, y, posKey);
        }
        else if (mapData.TryGetValue(posKey, out var nodeConfig))
        {
            var nodeType = nodeConfig.NodeType;
            if (nodeType == 2 || nodeType == 3 || nodeType == 4)
            {
                var image = DrawIconImage(canvas, x, y, nodeType);
                ImageNodes.Add(posKey, (nodeType, image));
                AttachEventAction(canvas, polygon, image, x, y, posKey);
                JingmaiVm!.SlotCount++;
            }
        }
    }

    /// <summary>
    /// 六边形被点击的处理
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="polygon"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="posKey"></param>
    private void OnPosTap(Canvas canvas, Polygon polygon, double x, double y, JingmaiNodePos posKey)
    {
        //Debug.WriteLine($"{posKey.Item1}|{posKey.Item2}|{posKey.Item3}");
        if (JingmaiVm == null)
        {
            return;
        }
        if (posKey.IsZero())
        {
            return;
        }
        ReplaceIconImage(canvas, polygon, x, y, posKey, currentToolType);
        //修改类型
        if (JingmaiVm.JingmaiMap.TryGetValue(posKey, out var nodeConfig))
        {
            if (nodeConfig.NodeType == currentToolType)
            {
                return;
            }
            nodeConfig.NodeType = currentToolType;
            if (currentToolType == 0)
            {
                //判断是否为孤立点
                var isNullLink = true;
                foreach (var linkConfig in nodeConfig.PageLinkConfigs)
                {
                    if (linkConfig.LineIndex >= 0)
                    {
                        isNullLink = false;
                        break;
                    }
                }
                if (isNullLink)
                {
                    JingmaiVm.JingmaiMap.Remove(posKey);
                }
            }
        }
        //新添加的穴位
        else if (currentToolType > 0)
        {
            //计算总页数
            var zeroPos = JingmaiNodePos.Zero();
            var zeroPosConfig = JingmaiVm.JingmaiMap[zeroPos];
            int pageNum = zeroPosConfig.PageLinkConfigs.Length;
            //构建节点对象
            var linkConfigs = new JingmaiNodeLinkInfo[pageNum];
            for (int i = 0; i < pageNum; i++)
            {
                linkConfigs[i] = JingmaiNodeLinkInfo.NullLink();
            }
            JingmaiVm.JingmaiMap.Add(posKey, new(currentToolType, linkConfigs));

        }
        JingmaiVm.RebuildJsonData();
    }

    /// <summary>
    /// 画上穴位图标
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nodeType"></param>
    /// <returns></returns>
    private Image DrawIconImage(Canvas canvas, double x, double y, int nodeType)
    {
        var image = new Image()
        {
            Width = iconWidth,
            Height = iconWidth,
            Source = ImageSourceList[nodeType - 1],
        };
        Canvas.SetLeft(image, nodeSize - (iconWidth / 2) + x);
        Canvas.SetTop(image, (Math.Sqrt(3) * nodeSize - iconWidth) / 2 + y);
        canvas.Children.Add(image);
        return image;
    }


    /// <summary>
    /// 替换图标
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="polygon"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="posKey"></param>
    /// <param name="nodeType"></param>
    private void ReplaceIconImage(Canvas canvas, Polygon polygon, double x, double y, JingmaiNodePos posKey, int nodeType)
    {
        //图标已经存在
        if (ImageNodes.TryGetValue(posKey, out var posItem))
        {
            var (tNodeType, tImage) = posItem;
            //类型一样,无需替换
            if (tNodeType == nodeType)
            {
                return;
            }
            //移除原来的图标
            canvas.Children.Remove(tImage);
            ImageNodes.Remove(posKey);
            JingmaiVm!.SlotCount -= 1;
        }
        //画上新图标
        if (nodeType > 0)
        {
            var image = DrawIconImage(canvas, x, y, nodeType);
            ImageNodes.Add(posKey, (nodeType, image));
            AttachEventAction(canvas, polygon, image, x, y, posKey);
            JingmaiVm!.SlotCount += 1;
        }
    }

    private static Polygon BuildPolygon()
    {
        var item = new Polygon()
        {
            Width = 2 * nodeSize,
            Height = Math.Sqrt(3) * nodeSize,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            Points = new Points()
            {
                new(0,Math.Sqrt(3) * nodeSize/2),
                new(nodeSize/2,0),
                new(nodeSize*3/2,0),
                new(2*nodeSize,Math.Sqrt(3) * nodeSize/2),
                new(nodeSize*3/2,Math.Sqrt(3) * nodeSize),
                new(nodeSize/2,Math.Sqrt(3) * nodeSize),
            },
            Fill = Brushes.AliceBlue,
        };
        return item;
    }

    private void AttachEventAction(Canvas canvas, Polygon polygon, double x, double y, JingmaiNodePos posKey)
    {
        AttachEventAction(canvas, polygon, polygon, x, y, posKey);
    }


    /// <summary>
    /// 六边形和上面的element的事件绑定
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="polygon">六边形</param>
    /// <param name="element"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="posKey"></param>
    private void AttachEventAction(Canvas canvas, Polygon polygon, InputElement element, double x, double y, JingmaiNodePos posKey)
    {
        void enteredAction()
        {
            polygon.Fill = Brushes.GreenYellow;
        }
        void exitedAction()
        {
            polygon.Fill = Brushes.AliceBlue;
        }
        void tappedAction()
        {
            OnPosTap(canvas, polygon, x, y, posKey);
        }
        element.PointerEntered += (o, e) =>
        {
            enteredAction();
        };
        element.PointerExited += (o, e) =>
        {
            exitedAction();
        };
        element.Tapped += (o, e) =>
        {
            tappedAction();
        };
    }
}
