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
[assembly: AssemblyVersion("1.3.*")]

namespace RadiusFillCornersEffect
{
    public class PluginSupportInfo : IPluginSupportInfo
    {
        public string Author
        {
            get
            {
                return ((AssemblyCopyrightAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0]).Copyright;
            }
        }
        public string Copyright
        {
            get
            {
                return ((AssemblyDescriptionAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0]).Description;
            }
        }

        public string DisplayName
        {
            get
            {
                return ((AssemblyProductAttribute)base.GetType().Assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0]).Product;
            }
        }

        public Version Version
        {
            get
            {
                return base.GetType().Assembly.GetName().Version;
            }
        }

        public Uri WebsiteUri
        {
            get
            {
                return new Uri("http://www.getpaint.net/redirect/plugins.html");
            }
        }
    }

    [PluginSupportInfo(typeof(PluginSupportInfo), DisplayName = "Radius Corners")]
    public class RadiusFillCornersEffectPlugin : PropertyBasedEffect
    {
        public static string StaticName
        {
            get
            {
                return "Radius Corners";
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return new Bitmap(typeof(RadiusFillCornersEffectPlugin), "RadiusFillCorners.png");
            }
        }

        public static string SubmenuName
        {
            get
            {
                return SubmenuNames.Stylize;
            }
        }

        public RadiusFillCornersEffectPlugin()
            : base(StaticName, StaticIcon, SubmenuName, EffectFlags.Configurable)
        {
        }

        public enum PropertyNames
        {
            Amount1,
            Amount2,
            Amount3,
            Amount4
        }

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            double width = base.EnvironmentParameters.SourceSurface.Width;
            double height = base.EnvironmentParameters.SourceSurface.Height;
            int radiusMax = (height > width) ? (int)Math.Ceiling(width / 2.0) : (int)Math.Ceiling(height / 2.0);
            int radiusDefault = radiusMax / 2;

            List<Property> props = new List<Property>();

            props.Add(new Int32Property(PropertyNames.Amount1, radiusDefault, 1, radiusMax));
            props.Add(new BooleanProperty(PropertyNames.Amount2, true));
            props.Add(new Int32Property(PropertyNames.Amount3, ColorBgra.ToOpaqueInt32(ColorBgra.FromBgra(EnvironmentParameters.PrimaryColor.B, EnvironmentParameters.PrimaryColor.G, EnvironmentParameters.PrimaryColor.R, 255)), 0, 0xffffff));
            props.Add(new BooleanProperty(PropertyNames.Amount4, true));

            List<PropertyCollectionRule> propRules = new List<PropertyCollectionRule>();

            propRules.Add(new ReadOnlyBoundToBooleanRule(PropertyNames.Amount3, PropertyNames.Amount2, false)); 

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

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            this.Amount1 = newToken.GetProperty<Int32Property>(PropertyNames.Amount1).Value;
            this.Amount2 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount2).Value;
            this.Amount3 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(PropertyNames.Amount3).Value);
            this.Amount4 = newToken.GetProperty<BooleanProperty>(PropertyNames.Amount4).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected override unsafe void OnRender(Rectangle[] rois, int startIndex, int length)
        {
            if (length == 0) return;
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Render(DstArgs.Surface, SrcArgs.Surface, rois[i]);
            }
        }

        private int radiusValue = 0;
        private int rectangleTopCoordinate = 0;
        private int rectangleBottomCoordinate = 0;
        private int rectangleLeftCoordinate = 0;
        private int rectangleRightCoordinate = 0;

        private bool PointWithinRadius(System.Windows.Point pointToTest, double radiusAA)
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

        #region User Entered Code
        #region UICode
        int Amount1 = 3; // [1,500] Radius
        bool Amount2 = true; // [0,1] Transparent
        ColorBgra Amount3 = ColorBgra.FromBgr(0, 0, 0); // 
        bool Amount4 = true; // [0,1] Anti-aliasing
        #endregion

        private BinaryPixelOp normalOp = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal);

        void Render(Surface dst, Surface src, Rectangle rect)
        {
            ColorBgra sourceColor;

            ColorBgra imageColor;

            // Get a dedicated transparent color
            ColorBgra a0Color = new ColorBgra();
            a0Color.A = 0;

            // update Background Fill based on checkbox
            ColorBgra fillColor = new ColorBgra();
            if (Amount2)
            {
                fillColor.A = 0;
            }
            else
            {
                fillColor = Amount3;
            }

            radiusValue = Amount1;
            Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt(); 
            // create a rectangle that will be used to determine how the pixels should be rendered
            rectangleTopCoordinate = selection.Top + radiusValue;
            rectangleBottomCoordinate = selection.Bottom - 1 - radiusValue;
            rectangleLeftCoordinate = selection.Left + radiusValue;
            rectangleRightCoordinate = selection.Right - 1 - radiusValue;

            // create point for testing how each pixel should be colored
            System.Windows.Point pointToTest = new System.Windows.Point();

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                if (IsCancelRequested) return;
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    sourceColor = src[x, y];
                    imageColor = a0Color;

                    // update point's coordinates
                    pointToTest.X = x;
                    pointToTest.Y = y;

                    // if point is not within the corner use original source pixel
                    if (!PointWithinRadius(pointToTest, 0))
                    {
                        imageColor = sourceColor;
                    }
                    else if (Amount4)
                    {
                        if (!PointWithinRadius(pointToTest, 0.333))
                        {
                            imageColor = sourceColor;
                            imageColor.A = (byte)(0.7 * sourceColor.A);
                        }
                        else if (!PointWithinRadius(pointToTest, 0.666))
                        {
                            imageColor = sourceColor;
                            imageColor.A = (byte)(0.4 * sourceColor.A);
                        }
                        else if (!PointWithinRadius(pointToTest, 1))
                        {
                            imageColor = sourceColor;
                            imageColor.A = (byte)(0.2 * sourceColor.A);
                        }
                    }

                    dst[x, y] = normalOp.Apply(fillColor, imageColor);
                }
            }
        }
        #endregion
    }
}