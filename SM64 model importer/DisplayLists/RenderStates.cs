﻿using System;
using System.Collections.Generic;
using System.Text;
using SM64RAM;

namespace SM64_model_importer
{
    public class RenderStates
    {
        public enum Parameter
        {
            EnvironmentColor = 0,
            FogColor = 1,
            FogIntensity = 2,
        }

        public static int MaxParameter { get; private set; }

        public delegate DisplayList.Command ParameterSource(Parameter param);
        public ParameterSource[] getParameter = new ParameterSource[3];

        #region Blend Modes
        public enum AlphaCompare
        {
            None = 0,
            BlendColor = 1,
            Random = 3
        }
        public enum CoverageDestination
        {
            Clamp = 0,
            Wrap = 1,
            Full = 2,
            Save = 3
        }
        public enum ZMode
        {
            Opaque = 0,
            Interpenetrating = 1,
            Translucent = 2,
            Decal = 3
        }

        public int otherModesLow = 0x2078;
        public int blendMode = 0xC811;

        public AlphaCompare alphaCompare
        {
            get { return (AlphaCompare)Utility.GetBits(otherModesLow, 2, 0); }
            set { Utility.SetBits(ref otherModesLow, 2, 0, (int)value); }
        }
        public bool zSrcSelect
        {
            get { return Utility.GetBit(otherModesLow, 2); }
            set { Utility.SetBit(ref otherModesLow, 2, value); }
        }
        public bool antiAliasing
        {
            get { return Utility.GetBit(otherModesLow, 3); }
            set { Utility.SetBit(ref otherModesLow, 3, value); }
        }
        public bool zCompare
        {
            get { return Utility.GetBit(otherModesLow, 4); }
            set { Utility.SetBit(ref otherModesLow, 4, value); }
        }
        public bool zUpdate
        {
            get { return Utility.GetBit(otherModesLow, 5); }
            set { Utility.SetBit(ref otherModesLow, 5, value); }
        }
        public bool accessCoverage
        {
            get { return Utility.GetBit(otherModesLow, 6); }
            set { Utility.SetBit(ref otherModesLow, 6, value); }
        }
        public bool clearOnCoverage
        {
            get { return Utility.GetBit(otherModesLow, 7); }
            set { Utility.SetBit(ref otherModesLow, 7, value); }
        }
        public CoverageDestination coverageDest
        {
            get { return (CoverageDestination)Utility.GetBits(otherModesLow, 2, 8); }
            set { Utility.SetBits(ref otherModesLow, 2, 8, (int)value); }
        }
        public ZMode zMode
        {
            get { return (ZMode)Utility.GetBits(otherModesLow, 2, 10); }
            set { Utility.SetBits(ref otherModesLow, 2, 10, (int)value); }
        }
        public bool coverageXAlpha
        {
            get { return Utility.GetBit(otherModesLow, 12); }
            set { Utility.SetBit(ref otherModesLow, 12, value); }
        }
        public bool alphaCoverageSelect
        {
            get { return Utility.GetBit(otherModesLow, 13); }
            set { Utility.SetBit(ref otherModesLow, 13, value); }
        }
        public bool forceBlending
        {
            get { return Utility.GetBit(otherModesLow, 14); }
            set { Utility.SetBit(ref otherModesLow, 14, value); }
        }
        #endregion

        #region B7 flags
        public int RCPBits = 0x32000;
        public bool RCP_Shade
        {
            get { return Utility.GetBit(RCPBits, 2); }
            set { Utility.SetBit(ref RCPBits, 2, value); }
        }
        public bool RCP_FlatVtxRGBA
        {
            get { return Utility.GetBit(RCPBits, 9); }
            set { Utility.SetBit(ref RCPBits, 9, value); }
        }
        public bool RCP_CullFront
        {
            get { return Utility.GetBit(RCPBits, 12); }
            set { Utility.SetBit(ref RCPBits, 12, value); }
        }
        public bool RCP_CullBack
        {
            get { return Utility.GetBit(RCPBits, 13); }
            set { Utility.SetBit(ref RCPBits, 13, value); }
        }
        public bool RCP_Fog
        {
            get { return Utility.GetBit(RCPBits, 16); }
            set { Utility.SetBit(ref RCPBits, 16, value); }
        }
        public bool RCP_Lighting
        {
            get { return Utility.GetBit(RCPBits, 17); }
            set { Utility.SetBit(ref RCPBits, 17, value); }
        }
        public bool RCP_TexGen
        {
            get { return Utility.GetBit(RCPBits, 18); }
            set { Utility.SetBit(ref RCPBits, 18, value); }
        }
        public bool RCP_TexGenLinear
        {
            get { return Utility.GetBit(RCPBits, 19); }
            set { Utility.SetBit(ref RCPBits, 19, value); }
        }
        #endregion

        #region Combiner
        public CombinerStates combiner = new CombinerStates();
        public double textureScaleX = 1, textureScaleY = 1;
        #endregion

        public BlenderControl.CycleModes cycleType = BlenderControl.CycleModes.TwoCycle;

        static RenderStates()
        {
            MaxParameter = Enum.GetValues(typeof(Parameter)).Length;
        }

        public static DisplayList.Command CreateParameterCommand(Parameter param, int value)
        {
            switch (param)
            {
                case RenderStates.Parameter.EnvironmentColor:
                    return new DisplayList.Command(0xFB, 0, value);
                case RenderStates.Parameter.FogColor:
                    return new DisplayList.Command(0xF8, 0, value);
                case RenderStates.Parameter.FogIntensity:
                    return new DisplayList.Command(0xBC, 8, value);
                default:
                    throw new Exception("Invalid Parameter " + param.ToString());
            }
        }

        public void AppendCommands(List<DisplayList.Command> targetList, int layer)
        {
            unchecked
            {
                targetList.Add(new DisplayList.Command(0xE7, 0, 0));

                if (cycleType == BlenderControl.CycleModes.TwoCycle)
                    targetList.Add(new DisplayList.Command(0xBA, 0x1402, 0x00100000)); //Set cycle type to two cycle mode
                int blender = ((blendMode & 0xFFFF) << 0x10) | (otherModesLow & 0xFFFF);
                targetList.Add(new DisplayList.Command(0xB9, 0x31D, blender)); //Set blender
                targetList.Add(new DisplayList.Command(0xB9, 0x201, 0x0)); //Disable ZSelect (lol)

                SetParameter(targetList, Parameter.EnvironmentColor);
                if (RCP_Fog)
                {
                    SetParameter(targetList, Parameter.FogColor);
                    SetParameter(targetList, Parameter.FogIntensity);
                }

                targetList.Add(new DisplayList.Command(0xB6, 0x0, (~RCPBits) & 0x7FFFFFFE)); //Unset RCP bits
                targetList.Add(new DisplayList.Command(0xB7, 0x0, RCPBits & 0x7FFFFFFE)); //Set RCP bits

                targetList.Add(new DisplayList.Command(0xFC, (int)(combiner.state >> 0x20) & 0xFFFFFF, (int)(combiner.state & 0xFFFFFFFF))); //Set combiner

                targetList.Add(Commands.G_SETTILE(TextureImage.TextureFormat.G_IM_FMT_RGBA, TextureImage.BitsPerPixel.G_IM_SIZ_16b, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0));
                targetList.Add(new DisplayList.Command(0xBB, 0x1, (Math.Min(((int)(textureScaleX * (1 << 16))), 0xFFFF) << 0x10) | Math.Min((int)(textureScaleY * (1 << 16)), 0xFFFF)));
            }
        }

        void SetParameter(List<DisplayList.Command> targetList, Parameter param)
        {
            int a = (int)param;
            if (getParameter[a] == null)
                return;
            targetList.Add(getParameter[a](param));
        }

        public void Reset(List<DisplayList.Command> targetList, int layer)
        {
            if (cycleType != BlenderControl.CycleModes.OneCycle)
                targetList.Add(new DisplayList.Command(0xBA, 0x1402, 0x00000000));
            targetList.Add(new DisplayList.Command(0xB6, 0x0, RCPBits & 0x7FFFFFFE)); //Set RCP bits
            foreach (DisplayList.Command cmd in DisplayList.defaultCommands[layer])
                targetList.Add(cmd);
        }
    }
}
