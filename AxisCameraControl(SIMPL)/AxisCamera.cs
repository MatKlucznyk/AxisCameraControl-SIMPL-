using System;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronIO;

namespace AxisCameraControl_SIMPL_
{
    public class AxisCamera
    {
        private string ipA;
        private string username;
        private string password;
        private CTimer find;

        public delegate void IsInitialized(SimplSharpString status);
        public IsInitialized isInitialized { get; set; }

        public string IpAddress
        {
            get { return ipA; }
            set
            {
                if (ipA != value)
                {
                    ipA = value;
                    Initialize();
                }
            }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        private int zoomPos = 0;

        private void Initialize()
        {
            if(find != null)
            {
                find.Stop();
            }

            find = new CTimer(Find, this, 0, 10000);

        }

        private void Find(object o)
        {
            try
            {
                SSMono.Net.WebRequest request = SSMono.Net.WebRequest.Create(string.Format("http://{0}/axis-cgi/com/ptz.cgi?query=position", ipA));
                request.Timeout = 5;
                request.Credentials = new SSMono.Net.NetworkCredential(username, password);
                CrestronConsole.PrintLine("URL Path:{0}", request.RequestUri);
                //request.Method = "POST";
                SSMono.Net.WebResponse response = request.GetResponse();

                if (response.ContentLength > 0)
                {
                    find.Stop();
                    find.Dispose();

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line.Contains("zoom"))
                            {
                                var zoom = line.Split('=');
                                zoomPos = Convert.ToInt16(zoom[1]);
                                break;
                            }
                        }
                    }

                    if (isInitialized != null)
                    {
                        isInitialized("init-complete");
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Exception occured in AxisCamera {0} Find - ", ipA), e);
            }
        }

        public void PTZ(string type)
        {
            try
            {
                CrestronConsole.PrintLine("PTZ Type:{0} is being sent", type);
                SSMono.Net.WebRequest request = SSMono.Net.WebRequest.Create(string.Format("http://{0}/axis-cgi/com/ptz.cgi?move={1}", ipA, type));
                request.Credentials = new SSMono.Net.NetworkCredential(username, password);
                CrestronConsole.PrintLine("URL Path:{0}", request.RequestUri);
                //request.Method = "POST";
                request.GetResponse();
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Exception occured in AxisCamera {0} PTZ - ", ipA), e);
            }
            
        }

        public void SavePreset(ushort number)
        {
            try
            {
                SSMono.Net.WebRequest request = SSMono.Net.WebRequest.Create(string.Format("http://{0}/axis-cgi/com/ptz.cgi?setserverpresetname={1}", ipA, number));
                request.Credentials = new SSMono.Net.NetworkCredential(username, password);
                //request.Method = "POST";
                request.GetResponse();
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Exception occured in AxisCamera {0} SavePreset - ", ipA), e);
            }
        }

        public void RecallPreset(ushort number)
        {
            try
            {
                SSMono.Net.WebRequest request = SSMono.Net.WebRequest.Create(string.Format("http://{0}/axis-cgi/com/ptz.cgi?gotoserverpresetname={1}", ipA, number));
                request.Credentials = new SSMono.Net.NetworkCredential(username, password);
                //request.Method = "POST";
                request.GetResponse();
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Exception occured in AxisCamera {0} RecallPreset - ", ipA), e);
            }
        }

        public void Zoom(string type)
        {
            try
            {
                if (zoomPos + 200 < 10000 && type == "tele")
                {
                    zoomPos = zoomPos + 200;
                }
                else if (zoomPos != 9999 && type == "tele")
                {
                    zoomPos = 9999;
                }
                else if (zoomPos - 200 >= 0 && type == "wide")
                {
                    zoomPos = zoomPos - 200;
                }
                else if (zoomPos != 0 && type == "wide")
                {
                    zoomPos = 0;
                }

                SSMono.Net.WebRequest request = SSMono.Net.WebRequest.Create(string.Format("http://{0}/axis-cgi/com/ptz.cgi?zoom={1}", ipA, zoomPos));
                request.Credentials = new SSMono.Net.NetworkCredential(username, password);
                //request.Method = "POST";
                request.GetResponse();
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Exception occured in AxisCamera {0} Zoom - ", ipA), e);
            }
        }
    }
}
