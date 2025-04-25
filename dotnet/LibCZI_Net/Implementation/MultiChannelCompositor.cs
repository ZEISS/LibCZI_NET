// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LibCZI_Net.Interface;
    using LibCZI_Net.Interop;

    /// <summary> Implementation of the 'IMultiChannelCompositor' interface using the native library.</summary>
    internal class MultiChannelCompositor : IMultiChannelCompositor
    {
        /// <inheritdoc/>
        public MultiChannelCompositionChannelInfo GetMultiChannelCompositionChannelInfoFromDisplaySettings(IDisplaySettings displaySettings, int channelIndex, bool sixteenOrEightBitLut)
        {
            return LibCziApiInterop.Instance.CompositorFillOutCompositionChannelInfo(
                    ((IInternalObject)displaySettings).NativeObjectHandle,
                    channelIndex,
                    sixteenOrEightBitLut);
        }

        /// <inheritdoc/>
        public IBitmap ComposeMultiChannelImage(
            int channelCount,
            IEnumerable<(IBitmap, MultiChannelCompositionChannelInfo)> channelInfo)
        {
            var projection = channelInfo.Select(
                x => (((IInternalObject)x.Item1).NativeObjectHandle, x.Item2));

            IntPtr resultBitmapHandle = LibCziApiInterop.Instance.CompositorComposeMultiChannelImage(
                channelCount,
                projection.ToArray());
            return new Bitmap(resultBitmapHandle);
        }
    }
}
