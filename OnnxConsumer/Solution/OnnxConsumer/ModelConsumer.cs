using Newtonsoft.Json;
using OnnxConsumer.Models;
using RestSharp;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;

namespace OnnxConsumer
{
    public class ModelConsumer
    {
        public YoloResponse PredictObjects(string imageFilePath)
        {
            var webServiceUrl = ConfigurationManager.AppSettings["WebServiceUrl"];
            var client = new RestClient(webServiceUrl);

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            var payload = PrepareWebRequest(imageFilePath);
            request.AddJsonBody(payload);

            var response = client.Execute(request);
            var content = response.Content;

            var root = JsonConvert.DeserializeObject<WebServiceResponse>(response.Content);
            var result = JsonConvert.DeserializeObject<Result>(root.result);

            var parsedResponse = new YoloResponse(result.__ndarray__[0]);
            return parsedResponse;
        }

        public WebServiceRequest PrepareWebRequest(string imageFilePath)
        {
            using (var image = new Bitmap(imageFilePath))
            {
                var request = new WebServiceRequest();

                request.data = new List<List<List<List<int>>>>();

                var sample = new List<List<List<int>>>();
                request.data.Add(sample);


                var features = new List<float>(image.Width * image.Height * 3);
                for (var c = 0; c < 3; c++)
                {
                    var colorChannel = new List<List<int>>();
                    sample.Add(colorChannel);

                    for (var h = 0; h < image.Height; h++)
                    {
                        var pixels = new List<int>();
                        colorChannel.Add(pixels);
                        for (var w = 0; w < image.Width; w++)
                        {
                            var pixel = image.GetPixel(w, h);
                            var v = c == 0 ? pixel.R : c == 1 ? pixel.G : pixel.B;
                            pixels.Add(v);
                        }
                    }
                }

                return request;
            }

        }
    }
}
