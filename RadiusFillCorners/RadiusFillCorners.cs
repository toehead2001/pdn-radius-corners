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

[assembly: AssemblyTitle("Radius Corners Plugin for Paint.NET")]
[assembly: AssemblyDescription("Round off square corners")]
[assembly: AssemblyConfiguration("radius|corners")]
[assembly: AssemblyCompany("dan9298 & toe_head2001")]
[assembly: AssemblyProduct("Radius Corners")]
[assembly: AssemblyCopyright("Copyright © dan9298 & toe_head2001")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion("1.5.0.0")]

namespace RadiusFillCornersEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author => ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
        public string Copyright => ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
        public string DisplayName => ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
        public Version Version => base.GetType().Assembly.GetName().Version;
        public Uri WebsiteUri => new Uri("http://www.getpaint.net/redirect/plugins.html");
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Radius Corners")]
    public class RadiusFillCornersEffectPlugin : PropertyBasedEffect
    {
        private const string StaticName = "Radius Corners";
        private static readonly Image StaticIcon = new Bitmap(typeof(RadiusFillCornersEffectPlugin), "RadiusFillCorners.png");

        public RadiusFillCornersEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuNames.Stylize, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4,
            Amount5
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            Size selection = EnvironmentParameters.GetSelection(EnvironmentParameters.SourceSurface.Bounds).GetBoundsInt().Size;
            int radiusMax = Math.Min(selection.Width, selection.Height) / 2;
            int radiusDefault = radiusMax / 2;

            List<Property> props = new List<Property>
            {
                new Int32Property(PropertyNames.Amount1, radiusDefault, 1, radiusMax),
                new Int32Property(PropertyNames.Amount5, 0, 0, radiusMax),
                new BooleanProperty(PropertyNames.Amount2, true),
                new Int32Property(PropertyNames.Amount3, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff),
                new BooleanProperty(PropertyNames.Amount4, true)
            };

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToBooleanRule(PropertyNames.Amount3, PropertyNames.Amount2, false)
            };

            return new PropertyCollection(props, propRules);
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.Amount1, ControlInfoPropertyNames.DisplayName, "Radius");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.DisplayName, "Background Fill");
            configUI.SetPropertyControlValue(PropertyNames.Amount2, ControlInfoPropertyNames.Description, "Transparent");
            configUI.SetPropertyControlValue(PropertyNames.Amount3, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlType(PropertyNames.Amount3, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.Amount4, ControlInfoPropertyNames.Description, "Anti-aliasing");
            configUI.SetPropertyControlValue(PropertyNames.Amount5, ControlInfoPropertyNames.DisplayName, "Margin");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            this.Amount2 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount2).Value;
            this.Amount3 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount3).Value);
            this.Amount4 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount4).Value;
            this.Amount5 = newToken.GetProperty<Int32Property>(PropertyNames.Amount5).Value;

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

        int Amount1 = 3; // [1,500] Radius
        bool Amount2 = true; // [0,1] Transparent
        ColorBgra Amount3 = ColorBgra.FromBgr(0, 0, 0); // 
        bool Amount4 = true; // [0,1] Anti-aliasing
        int Amount5 = 0; // Margin

        private readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
            ColorBgra currentPixel;
            ColorBgra fillColor = Amount3;
            if (Amount2)
                fillColor.A = 0;
            int margin = Amount5;
            int radiusMax = Math.Min(selection.Width, selection.Height) / 2 - margin;
            radiusValue = (Amount1 > radiusMax) ? radiusMax : Amount1;

            // create a rectangle that will be used to determine how the pixels should be rendered
            rectangleTopCoordinate = selection.Top + margin + radiusValue;
            rectangleBottomCoordinate = selection.Bottom - margin - 1 - radiusValue;
            rectangleLeftCoordinate = selection.Left + margin + radiusValue;
            rectangleRightCoordinate = selection.Right- margin - 1 - radiusValue;

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

                    // if point is Not outside of the radius, use original source pixel Alpha value
                    if (!PointOutsideRadius(pointToTest, 0))
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

                    // Trim the margins
                    if (margin > 0 && (x < selection.Left + margin || x > selection.Right - margin - 1 || y < selection.Top + margin || y > selection.Bottom - margin - 1))
                        currentPixel.A = byte.MinValue;

                    dst[x, y] = normalOp.Apply(fillColor, currentPixel);
                }
            }
        }
    }
}