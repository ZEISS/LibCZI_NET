// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System.Collections.Generic;

    /// <summary>
    /// In this interface functionality for composing multi-channel images is gathered.
    /// </summary>
    public interface IMultiChannelCompositor
    {
        /// <summary>
        /// This method is used to construct a <see cref="MultiChannelCompositionChannelInfo"/> object from
        /// the given display settings. I.e. it extracts the information required to compose a multi-channel image
        /// from the specified display-settings and prepares the specialized information for the actual composition.
        /// This step may include the calculation of a LUT for the channel. If a LUT needs to be constructed,
        /// the parameter 'sixteenOrEightBitLut' specifies whether a 16-bit or 8-bit LUT should be created. The choice
        /// here depends on the pixel type of the image data that will be used for the composition - for 16-bit data,
        /// a 16 bit LUT is required, for 8-bit data, an 8-bit LUT is sufficient.
        /// </summary>
        /// <param name="displaySettings">The display settings to retrieve the channel information from.</param>
        /// <param name="channelIndex">The index of the channel to retrieve the information from (this indexes the 'displaySettings' object).</param>
        /// <param name="sixteenOrEightBitLut">A boolean indicating whether to use a 16-bit or 8-bit LUT.</param>
        /// <returns>A <see cref="MultiChannelCompositionChannelInfo"/> object containing the multi-channel-composition-information.</returns>
        MultiChannelCompositionChannelInfo GetMultiChannelCompositionChannelInfoFromDisplaySettings(IDisplaySettings displaySettings, int channelIndex, bool sixteenOrEightBitLut);

        /// <summary>
        /// Composes a set of bitmaps into a multi-channel-composite image.
        /// The enumeration contains tuples of bitmaps and their corresponding channel information. At most 'channelCount'
        /// items will be retrieved from the enumeration.
        /// </summary>
        /// <param name="channelCount">The number of elements .</param>
        /// <param name="channelInfo">An enumeration of tuples containing the bitmap and its corresponding channel information.</param>
        /// <returns>An <see cref="IBitmap"/> containing the composed multi-channel-composition image.</returns>
        IBitmap ComposeMultiChannelImage(
            int channelCount,
            IEnumerable<(IBitmap, MultiChannelCompositionChannelInfo)> channelInfo);
    }
}
