using System;
using System.Collections.Generic;

namespace Function.MediaQ.Model
{
    public class PredictionTimes
    {
        public DateTime Created { get; set; }
        public List<Prediction> Predictions { get; set; }
    }
    public class Prediction
    {
        public string Tag { get; set; }
        public double Probability { get; set; }
    }
}
