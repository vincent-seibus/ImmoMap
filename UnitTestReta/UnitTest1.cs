using System;
using Reta.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace UnitTestReta
{
    [TestClass]
    public class DesignManagerTest
    {
        [TestMethod]
        public void getColourDesignOfSpaceTest()
        {
            DesignManager spaceCtrl = new DesignManager();
            var colourExpected = "#123456";
            var config = new { Configuration = new { componentColor = colourExpected } };
            var colourReal = spaceCtrl.getColourDesignOfSpace(JsonConvert.SerializeObject(config));
            Assert.AreEqual(colourExpected, colourReal);
        }


        [TestMethod]
        public void CreateUpdateCssFileForSpaceTest()
        {
            DesignManager designManager = new DesignManager();
            var oldColour = "#123456";
            var oldConfig = new { Configuration = new { componentColor = oldColour } };
            var oldConfigString =  JsonConvert.SerializeObject(oldConfig);

            var newColour = "#aaaaaa";
            var newConfig = new { Configuration = new { componentColor = newColour } };
            var newConfigString = JsonConvert.SerializeObject(newConfig);

            var spaceid = "testunit";
            designManager.CreateUpdateCssFileForSpace(spaceid, designManager.getColourDesignOfSpace(newConfigString));

            Assert.IsTrue(System.IO.File.Exists("D:/Travail/Reta/RetaTrunk/Reta/Reta" + "/Files/CssSpecific/" + spaceid + ".css"));
        }

    }
}
