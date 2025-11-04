using FurnaceCore.Filters;
using FurnaceCore.IOManager;
using FurnaceTest.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FurnaceTest
{
    public class FilterTest
    {
        [Theory]
        [InlineData("03 02 11 FF 22", new byte[] { 0x03, 0x02 })]
        public void HexConverterConsumeTest(string inputValue, byte[] address)
        {
            IOManager ioManager = new IOManager();
            Mock.MockPort mockPort = new MockPort(ioManager);
            HandleFurnaceModule mockModule = new HandleFurnaceModule();

            ModbusAddressFilter filter = new ModbusAddressFilter(address[0], address[1], mockModule);

            ioManager.RegisterFilter(filter);

            mockPort.ReceiveData(inputValue);
            Assert.Equal(inputValue, mockModule.LastReceived);
        }

        [Theory]
        [InlineData("03 02 11 FF 22", new byte[] { 0x02, 0x02 })]
        public void HexConverterNotConsumeTest(string inputValue, byte[] address)
        {
            IOManager ioManager = new IOManager();
            Mock.MockPort mockPort = new MockPort(ioManager);
            HandleFurnaceModule mockModule = new HandleFurnaceModule();

            ModbusAddressFilter filter = new ModbusAddressFilter(address[0], address[1], mockModule);

            ioManager.RegisterFilter(filter);

            mockPort.ReceiveData(inputValue);
            Assert.Equal("", mockModule.LastReceived);
        }

        [Theory]
        [InlineData("03 02 11 FF 22", new byte[] { 0x03, 0x02 }, "03 03 22 33 00", new byte[] { 0x03, 0x03 })]
        public void HexConverterSeveralConsumeTest(string firstInput, byte[] firstAddress, string secondInput, byte[] secondAddress)
        {
            IOManager ioManager = new IOManager();
            Mock.MockPort mockPort = new MockPort(ioManager);
            HandleFurnaceModule firstHandle = new HandleFurnaceModule();
            HandleFurnaceModule secondHandle = new HandleFurnaceModule();

            ModbusAddressFilter firstFilter = new ModbusAddressFilter(firstAddress[0], firstAddress[1], firstHandle);
            ModbusAddressFilter secondFilter = new ModbusAddressFilter(secondAddress[0], secondAddress[1], secondHandle);

            ioManager.RegisterFilter(firstFilter);
            ioManager.RegisterFilter(secondFilter);
            mockPort.ReceiveData(firstInput);

            Assert.Equal(firstInput, firstHandle.LastReceived);
            Assert.Equal("", secondHandle.LastReceived);
            
            mockPort.ReceiveData(secondInput);
            Assert.Equal(firstInput, firstHandle.LastReceived);
            Assert.Equal(secondInput, secondHandle.LastReceived);
        }
    }
}
