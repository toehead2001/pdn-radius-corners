using System;

namespace RadiusFillCorners
{
    public class EffectPluginConfigToken : PaintDotNet.Effects.EffectConfigToken
    {
        public int radius;
        public FillType fillType;

        public EffectPluginConfigToken()
            : base()
        {
            // Set default variables here
            this.radius = 50;
            this.fillType = FillType.Transparent;
        }

        protected EffectPluginConfigToken(EffectPluginConfigToken copyMe)
            : base(copyMe)
        {
            // update token vars
            this.radius = copyMe.radius;
            this.fillType = copyMe.fillType;
        }

        public override object Clone()
        {
            return new EffectPluginConfigToken(this);
        }
    }
}