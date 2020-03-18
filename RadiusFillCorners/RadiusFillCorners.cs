using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;

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
        private int radiusValue = 0;
        private int rectangleTopCoordinate = 0;
        private int rectangleBottomCoordinate = 0;
        private int rectangleLeftCoordinate = 0;
        private int rectangleRightCoordinate = 0;
        private Rectangle marginBounds = Rectangle.Empty;
        private ColorBgra backColor = ColorBgra.Zero;
        private bool antiAlias = true;
        private bool transparent = true;

        private readonly BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

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
            Size selection = EnvironmentParameters.SelectionBounds.Size;
            int radiusMax = Math.Min(selection.Width, selection.Height) / 2;
            int radiusDefault = radiusMax / 2;

            IEnumerable<Property> props = new Property[]
            {
                new Int32Property(PropertyNames.Radius, radiusDefault, 1, radiusMax),
                new BooleanProperty(PropertyNames.AntiAliasing, true),
                new Int32Property(PropertyNames.Margin, 0, 0, radiusMax),
                new BooleanProperty(PropertyNames.TransparentBack, true),
                new Int32Property(PropertyNames.BackColor, ColorBgra.ToOpaqueInt32(EnvironmentParameters.PrimaryColor.NewAlpha(byte.MaxValue)), 0, 0xffffff)
            };

            IEnumerable<PropertyCollectionRule> propRules = new PropertyCollectionRule[]
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
            this.antiAlias = newToken.GetProperty<BooleanProperty>(PropertyNames.AntiAliasing).Value;
            this.transparent = newToken.GetProperty<BooleanProperty>(PropertyNames.TransparentBack).Value;
            this.backColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.BackColor).Value);

            int radius = newToken.GetProperty<Int32Property>(PropertyNames.Radius).Value;
            int margin = newToken.GetProperty<Int32Property>(PropertyNames.Margin).Value;

            Rectangle selection = EnvironmentParameters.SelectionBounds;
            marginBounds = Rectangle.FromLTRB(selection.Left + margin, selection.Top + margin, selection.Right - margin, selection.Bottom - margin);
            int radiusMax = Math.Min(selection.Width, selection.Height) / 2 - margin;
            radiusValue = Math.Min(radius, radiusMax);

            // create a rectangle that will be used to determine how the pixels should be rendered
            this.rectangleTopCoordinate = this.marginBounds.Top + this.radiusValue;
            this.rectangleBottomCoordinate = this.marginBounds.Bottom - 1 - this.radiusValue;
            this.rectangleLeftCoordinate = this.marginBounds.Left + this.radiusValue;
            this.rectangleRightCoordinate = this.marginBounds.Right - 1 - this.radiusValue;

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

        private void Render(Surface dst, Surface src, Rectangle rect)
        {
            ColorBgra currentPixel;
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

                    if (!this.marginBounds.Contains(x, y))
                    {
                        currentPixel.A = byte.MinValue;
                    }
                    // if point is Not outside of the radius, use original source pixel Alpha value
                    else if (!PointOutsideRadius(pointToTest, 0))
                    {
                        // Do nothing. Alpha channel stays the same
                    }
                    else if (this.antiAlias)
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

                    dst[x, y] = (this.transparent) ? currentPixel : normalOp.Apply(this.backColor, currentPixel);
                }
            }
        }
    }
}