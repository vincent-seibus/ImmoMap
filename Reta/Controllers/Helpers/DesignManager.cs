using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Reta.Controllers
{
    public class DesignManager
    {
        public void CreateUpdateCssFileForSpace(string spaceId, string newColour)
        {
            if (string.IsNullOrEmpty(newColour))
                return;

            string filesDirectoryPath = HostingEnvironment.MapPath("~/Files");
            if (!Directory.Exists(filesDirectoryPath + "/CssSpecific"))
                Directory.CreateDirectory(filesDirectoryPath + "/CssSpecific");

            string fileContent = "";
            if (!System.IO.File.Exists(HostingEnvironment.MapPath("~/Files/CssSpecific/" + spaceId + ".css")))
                fileContent = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Angular/css/specific.css"));
            else
                fileContent = System.IO.File.ReadAllText(HostingEnvironment.MapPath("~/Files/CssSpecific/" + spaceId + ".css"));

            string quote = "/*####*/";
            string currentColour = IsolateColourBetweenQuote(quote, fileContent);
            fileContent = fileContent.Replace(quote + currentColour + quote, quote + "#" + newColour + quote);

            System.IO.File.WriteAllText(HostingEnvironment.MapPath("~/Files/CssSpecific/" + spaceId + ".css"), fileContent);
        }

        public string getColourDesignOfSpace(string configuration)
        {
            try
            {
                string result = JObject.Parse(configuration)["componentColor"].ToString();
                result = result.Replace("#", "");
                return result;
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public string IsolateColourBetweenQuote(string Quote, string Text)
        {            
            string[] splitedText = Text.Split(new string[] { Quote }, StringSplitOptions.None);
            return splitedText[1];
        }
    }
}