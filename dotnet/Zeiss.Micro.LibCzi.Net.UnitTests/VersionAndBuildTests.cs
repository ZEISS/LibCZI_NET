// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Net.Interface;
    using Xunit;

    public class VersionAndBuildTests
    {
        [Fact]
        public void GetVersionInfoAndCheckResult()
        {
            VersionInfo versionInfo = Factory.GetVersionInfo();
            Assert.True(versionInfo.Major >= 0);
            Assert.True(versionInfo.Minor >= 0);
            Assert.True(versionInfo.Patch >= 0);
            Assert.True(versionInfo.Tweak >= 0);
        }

        [Fact]
        public void GetBuildInformationAndCheckResult()
        {
            BuildInfo buildInfo = Factory.GetBuildInformation();
            // Check that at least one of the fields is not empty - there is not much more we can do here
            Assert.False(
                string.IsNullOrEmpty(buildInfo.CompilerIdentification) &&
                string.IsNullOrEmpty(buildInfo.RepositoryUrl) &&
                string.IsNullOrEmpty(buildInfo.RepositoryBranch) &&
                string.IsNullOrEmpty(buildInfo.RepositoryTag));
        }
    }
}