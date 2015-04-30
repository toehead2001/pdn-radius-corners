using System;
using System.Collections;
using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;

using System.Windows.Media;

namespace RadiusFillCorners
{
    public class EffectPlugin
        : PaintDotNet.Effects.Effect
    {
        private int radiusValue = 0;
        private int rectangleTopCoordinate = 0;
        private int rectangleBottomCoordinate = 0;
        private int rectangleLeftCoordinate = 0;
        private int rectangleRightCoordinate = 0;

        public static string StaticName
        {
            get
            {
                return "Radius Fill Corners";
            }
        }

        public static Bitmap StaticIcon
        {
            get
            {
                return new Bitmap(typeof(EffectPlugin), "EffectPluginIcon.png");
            }
        }

        public static string StaticSubMenuName
        {
            get
            {
                //return null; // Use for no submenu
                return "Fill";
            }
        }

        public EffectPlugin()
            : base(EffectPlugin.StaticName, EffectPlugin.StaticIcon, EffectPlugin.StaticSubMenuName, true)
        {

        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            return new EffectPluginConfigDialog(EnvironmentParameters.PrimaryColor.ToColor(), EnvironmentParameters.SecondaryColor.ToColor());
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            // update local vars from token values
            this.radiusValue = ((EffectPluginConfigToken)parameters).radius;
            FillType fillType = ((EffectPluginConfigToken)parameters).fillType;

            // initialize temporary variables
            ColorBgra newColor = new ColorBgra();
            PdnRegion selectionRegion = EnvironmentParameters.GetSelection(srcArgs.Bounds);
            Rectangle rect = new Rectangle();
 
            // create a rectangle that will be used to determine how the pixels should be rendered
            RectangleF rectangleF = selectionRegion.GetBounds();
            rectangleTopCoordinate = (int)rectangleF.Top + radiusValue;
            rectangleBottomCoordinate = (int)rectangleF.Bottom - radiusValue;
            rectangleLeftCoordinate = (int)rectangleF.Left + radiusValue;
            rectangleRightCoordinate = (int)rectangleF.Right - radiusValue;

            // create point for testing how each pixel should be colored
            System.Windows.Point pointToTest = new System.Windows.Point();

            // update new color based on fill type
            switch (fillType)
            {
                case FillType.Primary:
                    newColor = (ColorBgra)EnvironmentParameters.PrimaryColor;
                    break;
                case FillType.Secondary:
                    newColor = (ColorBgra)EnvironmentParameters.SecondaryColor;
                    break;
                default:
                    newColor.A = 0;
                    break;
            }   

            // standard render loop, iterate over pixels
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                rect = rois[i];
                
                for (int y = rect.Top; y < rect.Bottom; ++y)
                {
                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        // update point's coordinates
                        pointToTest.X = x;
                        pointToTest.Y = y;

                        // determine if point is within the selected radius
                        if (this.PointWithinRadius(pointToTest))
                        {
                            // set the pixel to our new color
                            dstArgs.Surface[x, y] = newColor;
                        }
                        else
                        {
                            // set the pixel to it's original value
                            dstArgs.Surface[x, y] = srcArgs.Surface[x, y];
                        }
                    }
                }
            }
        }


        private bool PointWithinRadius(System.Windows.Point pointToTest)
        {
            // determine if point's x and y coordinates are within the area that we want to modify 
            if (pointToTest.X > this.rectangleLeftCoordinate && pointToTest.X < this.rectangleRightCoordinate) 
                return false;
            if (pointToTest.Y > this.rectangleTopCoordinate && pointToTest.Y < this.rectangleBottomCoordinate) 
                return false;

            // create geometry objects for testing
            System.Windows.Point circleCenter = new System.Windows.Point();
            EllipseGeometry circle = new EllipseGeometry();

            // create 4 center points that will be used to draw circles
            circleCenter.X = this.rectangleLeftCoordinate;
            circleCenter.Y = this.rectangleTopCoordinate;

            // update circle's values
            circle.Center = circleCenter;
            circle.RadiusX = (double)this.radiusValue;
            circle.RadiusY = (double)this.radiusValue;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // update circle's values
            circleCenter.X = this.rectangleRightCoordinate;
            circleCenter.Y = this.rectangleTopCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // update circle's values
            circleCenter.X = this.rectangleLeftCoordinate;
            circleCenter.Y = this.rectangleBottomCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;

            // update circle's values
            circleCenter.X = this.rectangleRightCoordinate;
            circleCenter.Y = this.rectangleBottomCoordinate;
            circle.Center = circleCenter;

            // check to see if our test point is contained with the current circle
            if (circle.FillContains(pointToTest)) return false;
    
            // all other condition's passed, so return true
            return true;
        }
    }

    // enum for type of fill to use
    public enum FillType
    { 
        Transparent = 0,
        Primary = 1,
        Secondary = 2
    }
}