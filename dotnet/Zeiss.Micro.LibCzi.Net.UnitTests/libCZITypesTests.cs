// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Net.Interface;
    using Xunit;

    public class LibCZITypesTests
    {
        [Fact]
        public void DimensionBoundsRemoveTest()
        {
            DimensionBounds bounds = new DimensionBounds();
            bounds.SetDimension(DimensionIndex.C, 0, 10);
            bounds.SetDimension(DimensionIndex.T, 0, 5);

            Assert.True(bounds.RemoveDimension(DimensionIndex.C));
            Assert.True(bounds.RemoveDimension(DimensionIndex.T));
            Assert.False(bounds.RemoveDimension(DimensionIndex.C));
            Assert.False(bounds.RemoveDimension(DimensionIndex.T));

            bounds.SetDimension(DimensionIndex.T, 0, 3);
            Assert.True(bounds.RemoveDimension(DimensionIndex.T));
            Assert.False(bounds.RemoveDimension(DimensionIndex.T));
        }
    }
}