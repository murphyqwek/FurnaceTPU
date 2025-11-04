using FurnaceCore.utlis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FurnaceTest
{
    public class HexTest
    {
        [Theory]
        [InlineData("48656C6C6F", new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F })]
        [InlineData("FF", new byte[] { 0xFF })]
        [InlineData("00", new byte[] { 0x00 })]
        [InlineData("010203", new byte[] { 0x01, 0x02, 0x03 })]
        public void TryHexStringToByteArray_ValidHex_ReturnsTrueAndCorrectBytes(string hex, byte[] expected)
        {
            bool result = HexConverter.TryHexStringToByteArray(hex, out byte[]? actual);

            Assert.True(result);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("0x48656C6C6F")]
        [InlineData("48-65-6C-6C-6F")]
        [InlineData("48 65 6C 6C 6F")]
        [InlineData("0x48-65-6C-6C-6F")]
        public void TryHexStringToByteArray_HexWithFormatting_ReturnsTrueAndCorrectBytes(string formattedHex)
        {
            byte[] expected = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F };

            bool result = HexConverter.TryHexStringToByteArray(formattedHex, out byte[]? actual);

            Assert.True(result);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("GG")]
        [InlineData("48G5")]
        [InlineData("ZZ")]
        [InlineData("12 3G")]
        [InlineData("0x12ZZ")]
        [InlineData("12 3")]
        [InlineData("123")]
        public void TryHexStringToByteArray_InvalidCharacters_ReturnsFalse(string invalidCharsHex)
        {
            bool result = HexConverter.TryHexStringToByteArray(invalidCharsHex, out byte[]? actual);

            Assert.False(result);
            Assert.Null(actual);
        }
    }
}
