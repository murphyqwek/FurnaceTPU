namespace FurnaceTest
{
    public class IOManagerTest
    {

        [Fact]
        public void SendTest()
        {
            var ioManager = new FurnaceCore.IOManager.IOManager();

            var mockPort1 = new Mock.MockPort(ioManager);
            var mockModule1 = new Mock.MockFurnaceModule(ioManager);

            var mockPort2 = new Mock.MockPort(ioManager);
            var mockModule2 = new Mock.MockFurnaceModule(ioManager);


            ioManager.RegisterModulePort(mockModule1, mockPort1);

            ioManager.RegisterModulePort(mockModule2, mockPort2);

            mockModule1.sendData("TEST1: Hello World");
            mockModule2.sendData("TEST2: Hello World");


            Assert.Equal("TEST1: Hello World", mockPort1.SentData.Last());
            Assert.Equal("TEST2: Hello World", mockPort2.SentData.Last());
        }

        [Fact]
        public void SendDataCommonPortTest()
        {
            var ioManager = new FurnaceCore.IOManager.IOManager();
            var mockPort = new Mock.MockPort(ioManager);
            var mockModule1 = new Mock.MockFurnaceModule(ioManager);
            var mockModule2 = new Mock.MockFurnaceModule(ioManager);

            ioManager.RegisterModulePort(mockModule1, mockPort);
            ioManager.RegisterModulePort(mockModule2, mockPort);
            mockModule1.sendData("TEST1: Hello World");

            Assert.Equal("TEST1: Hello World", mockPort.SentData.Last());

            mockModule2.sendData("TEST2: Hello World");

            Assert.Equal("TEST2: Hello World", mockPort.SentData.Last());
        }

        [Fact]
        public void ReceiveDataTest()
        {
            var ioManager = new FurnaceCore.IOManager.IOManager();
            var mockPort = new Mock.MockPort(ioManager);
            var mockModule = new Mock.MockFurnaceModule(ioManager);
            ioManager.RegisterModulePort(mockModule, mockPort);
            ioManager.RegisterFilter(mockModule);
            mockPort.ReceiveData("TEST: Incoming Data");
            Assert.Equal("TEST: Incoming Data", mockModule.LastReceived);
        }

        [Fact]
        public void RoutingTest()
        {
            var ioManager = new FurnaceCore.IOManager.IOManager();
            var mockPort = new Mock.MockPort(ioManager);
            var mockModule1 = new Mock.MockFurnaceModule(ioManager);
            var mockModule2 = new Mock.MockFurnaceModule(ioManager);

            mockModule1.Tag = "TEST1";
            mockModule2.Tag = "TEST2";

            ioManager.RegisterFilter(mockModule1);
            ioManager.RegisterFilter(mockModule2);

            mockPort.ReceiveData("TEST1: Hello, module1");
            mockPort.ReceiveData("TEST2: Hello, module2");

            Assert.Equal("TEST1: Hello, module1", mockModule1.LastReceived);
            Assert.Equal("TEST2: Hello, module2", mockModule2.LastReceived);
        }
    
    }
}