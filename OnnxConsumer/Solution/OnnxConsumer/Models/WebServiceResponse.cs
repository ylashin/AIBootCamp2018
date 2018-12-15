using System.Collections.Generic;

namespace OnnxConsumer.Models
{
    public class WebServiceResponse
    {
        public string result { get; set; }
        public List<double> time_in_sec { get; set; }
    }

    public class Result
    {
        public List<List<List<List<float>>>> __ndarray__ { get; set; }
        public string dtype { get; set; }
        public List<int> shape { get; set; }
        public bool Corder { get; set; }
    }
}
