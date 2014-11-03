using Bing.Maps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Canada_Air_Quality_Health_Index
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            myMap.SetView(new Bing.Maps.Location(49.767500, -96.809722), 3.5);

            await Populate();



        }

        private async Task Populate()
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();

            var xmlDoc = await DownloadList().ConfigureAwait(false);


            XmlNodeList regionNodes = xmlDoc.GetElementsByTagName("region");



            foreach (var node in regionNodes)
            //Parallel.ForEach(regionNodes,  async (node) =>
            {
                XmlNamedNodeMap attributes = node.Attributes;

                try
                {
                    var item = new FileInfo();
                    IXmlNode cgndbeAttribute = attributes.GetNamedItem("cgndb");
                    item.cgndb = cgndbeAttribute.InnerText;

                    IXmlNode nameEnAttribute = attributes.GetNamedItem("nameEn");
                    item.nameEn = nameEnAttribute.InnerText;

                    item.forecLink = node.SelectSingleNode("pathToCurrentForecast").InnerText;
                    item.obsLink = node.SelectSingleNode("pathToCurrentObservation").InnerText;

                    // get the air quality stuff, get coordinates as asynchronous tasks

                    var coordsTask = GetCoordsTask(item.cgndb);
                    var observationTask = GetObservationTask(item.obsLink);
                    var forecastTask = GetForecastTask(item.forecLink);

                    var coordsText = await coordsTask.ConfigureAwait(false);

                    item.Lat = double.Parse(coordsText.Split(new char[] { ',' }, StringSplitOptions.None)[0]);
                    item.Lng = double.Parse(coordsText.Split(new char[] { ',' }, StringSplitOptions.None)[1]);

                    var observationText = await observationTask;
                    var forecastText = await forecastTask;

                    await Task.Factory.StartNew(() =>
                     {
                         // add pushpin
                         Bing.Maps.Location loc = new Bing.Maps.Location
                         {

                             Latitude = item.Lat,
                             Longitude = item.Lng

                         };
                         Pushpin pushpin = new Pushpin();


                         var idx = Math.Round(double.Parse(observationText), 0);
                         var status = "";
                         var clr = Colors.Black;
                         switch ((int)idx)
                         {
                             case 0:
                                 status = "Low Risk";
                                 clr = Color.FromArgb(255, 153, 204, 255);
                                 break;
                             case 1:
                                 status = "Low Risk";
                                 clr = Color.FromArgb(255, 153, 204, 255);
                                 break;
                             case 2:
                                 status = "Low Risk";
                                 clr = Color.FromArgb(255, 102, 204, 255);
                                 break;
                             case 3:
                                 status = "Low Risk";
                                 clr = Color.FromArgb(255, 0, 204, 255);
                                 break;
                             case 4:
                                 status = "Moderate Risk";
                                 clr = Color.FromArgb(255, 153, 204, 204);
                                 break;
                             case 5:
                                 status = "Moderate Risk";
                                 clr = Color.FromArgb(255, 153, 153, 153);
                                 break;
                             case 6:
                                 status = "Moderate Risk";
                                 clr = Color.FromArgb(255, 153, 153, 102);
                                 break;
                             case 7:
                                 status = "High Risk";
                                 clr = Color.FromArgb(255, 153, 102, 0);
                                 break;
                             case 8:
                                 status = "High Risk";
                                 clr = Color.FromArgb(255, 153, 102, 51);
                                 break;
                             case 9:
                                 status = "High Risk";
                                 clr = Color.FromArgb(255, 153, 51, 0);
                                 break;
                             case 10:
                                 status = "High Risk";
                                 clr = Color.FromArgb(255, 102, 0, 0);
                                 break;
                             default:
                                 status = "Extreme High Risk";
                                 clr = Color.FromArgb(255, 255, 0, 0);
                                 break;


                         }

                         pushpin.Background = new SolidColorBrush(clr);


                         pushpin.Text = observationText;
                         MapLayer.SetPosition(pushpin, loc);
                         myMap.Children.Add(pushpin);

                         var dialogMessage = observationText + " for " + item.nameEn + " (" + status + ")" + System.Environment.NewLine;
                         dialogMessage += forecastText;

                         pushpin.Tapped += async (s, e) =>
                         {
                             MessageDialog dialog = new MessageDialog(dialogMessage);
                             await dialog.ShowAsync();
                         };


                     }, CancellationToken.None, TaskCreationOptions.None, ui).ConfigureAwait(false);




                    /*
                    Task<string>[] taks = { observationTask, forecastTask , coordsTask };

                    

                    await Task.WhenAll(taks).ContinueWith((antecedent) =>
                    {

                        if (antecedent.Exception == null)
                        {


                            var observationHere = antecedent.Result[0];
                            var fcast = antecedent.Result[1];
                            var coordsText = antecedent.Result[2];

                            // add pushpin
                            Bing.Maps.Location loc = new Bing.Maps.Location
                            {
                                
                                Latitude = double.Parse(coordsText.Split(new char[] { ',' }, StringSplitOptions.None)[0]),
                                Longitude = double.Parse(coordsText.Split(new char[] { ',' }, StringSplitOptions.None)[1])
                                
                                
                                //Latitude = item.Lat,
                                //Longitude = item.Lng
                                
                            };

                            Pushpin pushpin = new Pushpin();


                            var idx = Math.Round(double.Parse(observationHere), 0);
                            var status = "";
                            var clr = Colors.Black;
                            switch ((int)idx)
                            {
                                case 0:
                                    status = "Low Risk";
                                    clr = Color.FromArgb(255, 153, 204, 255);
                                    break;
                                case 1:
                                    status = "Low Risk";
                                    clr = Color.FromArgb(255, 153, 204, 255);
                                    break;
                                case 2:
                                    status = "Low Risk";
                                    clr = Color.FromArgb(255, 102, 204, 255);
                                    break;
                                case 3:
                                    status = "Low Risk";
                                    clr = Color.FromArgb(255, 0, 204, 255);
                                    break;
                                case 4:
                                    status = "Moderate Risk";
                                    clr = Color.FromArgb(255, 153, 204, 204);
                                    break;
                                case 5:
                                    status = "Moderate Risk";
                                    clr = Color.FromArgb(255, 153, 153, 153);
                                    break;
                                case 6:
                                    status = "Moderate Risk";
                                    clr = Color.FromArgb(255, 153, 153, 102);
                                    break;
                                case 7:
                                    status = "High Risk";
                                    clr = Color.FromArgb(255, 153, 102, 0);
                                    break;
                                case 8:
                                    status = "High Risk";
                                    clr = Color.FromArgb(255, 153, 102, 51);
                                    break;
                                case 9:
                                    status = "High Risk";
                                    clr = Color.FromArgb(255, 153, 51, 0);
                                    break;
                                case 10:
                                    status = "High Risk";
                                    clr = Color.FromArgb(255, 102, 0, 0);
                                    break;
                                default:
                                    status = "Extreme High Risk";
                                    clr = Color.FromArgb(255, 255, 0, 0);
                                    break;


                            }

                            pushpin.Background = new SolidColorBrush(clr);


                            pushpin.Text = observationHere;
                            MapLayer.SetPosition(pushpin, loc);
                            myMap.Children.Add(pushpin);

                            var dialogMessage = observationHere + " for " + item.nameEn + " (" + status + ")" + System.Environment.NewLine;
                            dialogMessage += fcast;

                            pushpin.Tapped += async (s, e) =>
                            {
                                MessageDialog dialog = new MessageDialog(dialogMessage);
                                await dialog.ShowAsync();
                            };


                        }
                        else
                        {
                            //Debug.WriteLine(antecedent.Exception.Flatten().Message);
                        }

                    }, ui).ConfigureAwait(false);
                    */


                }
                catch (Exception ex)
                {
                    var t = ex.Message; //touch it
                    Debug.WriteLine(t);
                }



            }

            MessageDialog doneDlg = new MessageDialog("Done Loading...");
            await doneDlg.ShowAsync();
        }



        private async Task<XmlDocument> DownloadList()
        {
            Uri uri = new Uri("http://dd.weather.gc.ca/air_quality/doc/AQHI_XML_File_List.xml");
            XmlDocument xmlDocument = await XmlDocument.LoadFromUriAsync(uri);
            return xmlDocument;
        }


        private async Task<string> GetCoordsTask(string cgnb)
        {
            var coordsDoc = await DownloadCoords(cgnb).ConfigureAwait(false);
            IXmlNode latnode = coordsDoc.GetElementsByTagName("latdec").FirstOrDefault();

            IXmlNode lonnode = coordsDoc.GetElementsByTagName("londec").FirstOrDefault();
            return latnode.InnerText + "," + lonnode.InnerText;

        }

        private async Task<XmlDocument> DownloadCoords(string cgnb)
        {
            var link = string.Format("http://www.nrcan.gc.ca/earth-sciences/api?cgndbKey={0}&output=xml", cgnb);
            Uri uri = new Uri(link);
            return await XmlDocument.LoadFromUriAsync(uri);

        }

        private async Task<XmlDocument> DownloadLink(string link)
        {
            Uri uri = new Uri(link);
            XmlDocument xmlDocument = await XmlDocument.LoadFromUriAsync(uri);
            return xmlDocument;
        }

        private async Task<string> GetObservationTask(string link)
        {
            var observationDoc = await DownloadLink(link).ConfigureAwait(false);
            IXmlNode observationNode = observationDoc.GetElementsByTagName("airQualityHealthIndex").FirstOrDefault();
            return observationNode.InnerText;
        }

        private async Task<string> GetForecastTask(string link)
        {
            var results = new StringBuilder();
            results.AppendLine("Forecast:");
            try
            {
                var forecastDoc = await DownloadLink(link).ConfigureAwait(false);

                XmlNodeList fcastNodes = forecastDoc.GetElementsByTagName("forecast");

                foreach (var node in fcastNodes)
                {
                    var aqhi = node.SelectSingleNode("airQualityHealthIndex").InnerText;
                    var period = node.SelectSingleNode("period");
                    XmlNamedNodeMap attributes = period.Attributes;
                    var name = attributes.GetNamedItem("forecastName").InnerText;
                    name += " (" + period.InnerText + ")";
                    results.Append(name);
                    results.AppendLine(" Index:" + aqhi);

                }
            }
            catch { }

            return results.ToString();

        }

    }
}
