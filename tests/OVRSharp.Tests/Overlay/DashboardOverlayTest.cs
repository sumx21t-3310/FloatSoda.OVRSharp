using System;
using NUnit.Framework;
using OVRSharp.Exceptions;
using Valve.VR;

namespace OVRSharp.Tests.Overlay
{
    [TestFixture]
    public class DashboardOverlayTest
    {
        private Application _app;

        [OneTimeSetUp]
        public void Setup()
        {
            try
            {
                _app = new Application(Application.ApplicationType.Overlay);
            }
            catch (OpenVRSystemException<EVRApplicationError> e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public void Test_LaunchOverlay()
        {
            
        }
    }
}