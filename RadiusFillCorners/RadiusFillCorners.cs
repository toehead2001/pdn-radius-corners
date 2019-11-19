using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Media;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;

[assembly: AssemblyTitle("Radius Corners plugin for paint.net")]
[assembly: AssemblyDescription("Round off square corners")]
[assembly: AssemblyConfiguration("radius|corners")]
[assembly: AssemblyCompany("toe_head2001")]
[assembly: AssemblyProduct("Radius Corners")]
[assembly: AssemblyCopyright("Copyright © 2018 toe_head2001 & dan9298")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.6.0.0")]

namespace RadiusFillCornersEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => base.GetType().Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        public string Copyright => base.GetType().Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
        public string DisplayName => base.GetType().Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("https://forums.getpaint.net/index.php?showtopic=31574");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Radius Corners")]
    public class RadiusFillCornersEffectPlugin : PropertyBasedEffect
    {
        private static readonly Image StaticIcon = new Bitmap(typeof(RadiusFillCornersEffectPlugin), "RadiusFillCorners.png");

        public RadiusFillCornersEffectPlugin()
            : base("Radius Corners", StaticIcon, SubmenuNames.Stylize, new EffectOptions { Flags = EffectFlags.Configurable })
        {
        }

        private enum PropertyNames
        {
            Radius,
            TransparentBack,
            BackColor,
            AntiAliasing,
            Margin
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            Size selection = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt().Size;
            int radiusMax = Math.Min(selection.Width, selection.Height) / 2;
            int radiusDefault = radiusMax / 2;

            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Radius, radiusDefault, 1, radiusMax),
                new BooleanProperty(PropertyNames.AntiAliasing, true),
                new Int32Property(PropertyNames.Margin, 0, 0, radiusMax),
                new BooleanProperty(PropertyNames.TransparentBack, true),
                new Int32Property(PropertyNames.BackColor, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff)
            };

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToBooleanRule(PropertyNames.BackColor, PropertyNames.TransparentBack, false)
            };

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Radius, ControlInfoPropertyNames.DisplayName, "Radius");

            configUI.SetPropertyControlValue(PropertyNames.AntiAliasing, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.AntiAliasing, ControlInfoPropertyNames.Description, "Anti-aliasing");

            configUI.SetPropertyControlValue(PropertyNames.Margin, ControlInfoPropertyNames.DisplayName, "Margin");

            configUI.SetPropertyControlValue(PropertyNames.TransparentBack, ControlInfoPropertyNames.DisplayName, "Background Fill");
            configUI.SetPropertyControlValue(PropertyNames.TransparentBack, ControlInfoPropertyNames.Description, "Transparent");

            configUI.SetPropertyControlValue(PropertyNames.BackColor, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlType(PropertyNames.BackColor, PropertyControlType.ColorWheel);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            this.Amount2 = newToken.GetProperty<BooleanProperty>(PropertyNames.TransparentBack).Value;
            this.Amount3 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.BackColor).Value);
            this.Amount4 = newToken.GetProperty<BooleanProperty>(PropertyNames.AntiAliasing).Value;
            this.Amount5 = newToken.GetProperty<Int32Property>(PropertyNames.Margin).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, renderRects[i]);
            }
        }

        private int radiusValue = 0;
        private int rectangleTopCoordinate = 0;
        private int rectangleBottomCoordinate = 0;
        private int rectangleLeftCoordinate = 0;
        private int rectangleRightCoordinate = 0;

        private bool PointOutsideRadius(System.Windows.Point pointToTest, double radiusAA)
        {
            // determine if point's x and y coordinates are within the area that we want to modify 
            if (pointToTest.X > rectangleLeftCoordinate && pointToTest.X < rectangleRightCoordinate)
                return false;
            if (pointToTest.Y > rectangleTopCoordinate && pointToTest.Y < rectangleBottomCoordinate)
                return false;

            // create geometry objects for testing
            System.Windows.Point circleCenter = new System.Windows.Point();
            EllipseGeometry circle = new EllipseGeometry();

            // update circle's values
            circle.RadiusX = (double)radiusValue + radiusAA;
            circle.RadiusY = (double)radiusValue + radiusAA;

            // create 4 center points that will be used to draw circles
            circleCenter.X = rectangleLeftCoordinate;
            circleCenter.Y = rectangleTopCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // update circle's values
            circleCenter.X = rectangleRightCoordinate;
            circleCenter.Y = rectangleTopCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // update circle's values
            circleCenter.X = rectangleLeftCoordinate;
            circleCenter.Y = rectangleBottomCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // update circle's values
            circleCenter.X = rectangleRightCoordinate;
            circleCenter.Y = rectangleBottomCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // all other condition's passed, so return true
            return true;
        }

        private int Amount1 = 3; // [1,500] Radius
        private bool Amount2 = true; // [0,1] Transparent
        private ColorBgra Amount3 = ColorBgra.FromBgr(0, 0, 0); // 
        private bool Amount4 = true; // [0,1] Anti-aliasing
        private int Amount5 = 0; // Margin

        private readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        private void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
            ColorBgra currentPixel;
            ColorBgra fillColor = Amount3;
            if (Amount2)
                fillColor.A = 0;
            Rectangle marginBounds = Rectangle.FromLTRB(selection.Left + Amount5, selection.Top + Amount5, selection.Right - Amount5, selection.Bottom - Amount5);
            int radiusMax = Math.Min(selection.Width, selection.Height) / 2 - Amount5;
            radiusValue = (Amount1 > radiusMax) ? radiusMax : Amount1;

            // create a rectangle that will be used to determine how the pixels should be rendered
            rectangleTopCoordinate = marginBounds.Top + radiusValue;
            rectangleBottomCoordinate = marginBounds.Bottom - 1 - radiusValue;
            rectangleLeftCoordinate = marginBounds.Left + radiusValue;
            rectangleRightCoordinate = marginBounds.Right - 1 - radiusValue;

            // create point for testing how each pixel should be colored
            System.Windows.Point pointToTest = new System.Windows.Point();

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    // update point's coordinates
                    pointToTest.X = x;
                    pointToTest.Y = y;

                    currentPixel = src[x, y];

                    if (!marginBounds.Contains(x, y))
                    {
                        currentPixel.A = byte.MinValue;
                    }
                    // if point is Not outside of the radius, use original source pixel Alpha value
                    else if (!PointOutsideRadius(pointToTest, 0))
                    {
                        // Do nothing. Alpha channel stays the same
                    }
                    else if (Amount4)
                    {
                        if (!PointOutsideRadius(pointToTest, 0.333))
                        {
                            currentPixel.A = (byte)(0.7 * currentPixel.A);
                        }
                        else if (!PointOutsideRadius(pointToTest, 0.666))
                        {
                            currentPixel.A = (byte)(0.4 * currentPixel.A);
                        }
                        else if (!PointOutsideRadius(pointToTest, 1))
                        {
                            currentPixel.A = (byte)(0.2 * currentPixel.A);
                        }
                        else
                        {
                            currentPixel.A = byte.MinValue;
                        }
                    }
                    else
                    {
                        currentPixel.A = byte.MinValue;
                    }

                    dst[x, y] = normalOp.Apply(fillColor, currentPixel);
                }
            }
        }
    }
}