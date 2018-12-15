using System;
using System.Collections.Generic;
using System.Linq;

namespace OnnxConsumer.Models
{
    public class YoloResponse
    {

        private readonly List<ObjectFound> ObjectsFound;
        private readonly Dictionary<(int cx, int cy), Block> Blocks = new Dictionary<(int cx, int cy), Block>();
        public YoloResponse(List<List<List<float>>> input)
        {
            // YOLO response expected format [None,125,13,13]

            // https://github.com/hollance/Forge/blob/master/Examples/YOLO/YOLO/YOLO.swift
            // The 416x416 image is divided into a 13x13 grid. Each of these grid cells
            // will predict 5 bounding boxes (boxesPerCell). A bounding box consists of 
            // five data items: x, y, width, height, and a confidence score. Each grid 
            // cell also predicts which class each bounding box belongs to.
            //
            // The "features" array therefore contains (numClasses + 5)*boxesPerCell 
            // values for each grid cell, i.e. 125 channels. The total features array
            // contains 13x13x125 elements

            for(var i = 0; i < 125; i++)
            {
                for (var a = 0; a < 13; a++)
                {
                    for (var b = 0; b < 13; b++)
                    {
                        if (!Blocks.ContainsKey((b, a)))
                            Blocks[(b, a)] = new Block(b, a);

                        Blocks[(b, a)].SetEntry(i, input[i][a][b]);
                    }
                }
            }


            ObjectsFound = Blocks.Values.SelectMany(a => a.GetObjectsFoundInBlock()).ToList();
        }

        public List<ObjectFound> GetTopObjectsDetected(int maxItems = 5)
        {
            return ObjectsFound.OrderByDescending(a => a.Confidence).Take(maxItems).ToList();
        }

    }

    public class Block
    {
        private readonly string[] Labels = new string[]
        {
                "aeroplane",
                "bicycle",
                "bird",
                "boat",
                "bottle",
                "bus",
                "car",
                "cat",
                "chair",
                "cow",
                "diningtable",
                "dog",
                "horse",
                "motorbike",
                "person",
                "pottedplant",
                "sheep",
                "sofa",
                "train",
                "tvmonitor"
        };
        private readonly float[] Anchors = new[] { 1.08f, 1.19f, 3.42f, 4.41f, 6.63f, 11.38f, 9.42f, 5.11f, 16.62f, 10.52f };
        private readonly float[] _elements;
        private readonly int _gridCellX;
        private readonly int _gridCellY;
        public Block(int gridCellX, int gridCellY)
        {
            _gridCellX = gridCellX;
            _gridCellY = gridCellY;
            _elements = new float[125];
        }

        public void SetEntry(int index, float value)
        {
            _elements[index] = value;
        }
        private float Sigmoid(float x)
        {
            return (float)(1 / (1 + Math.Exp(-x)));
        }

        public List<ObjectFound> GetObjectsFoundInBlock()
        {
            var confidenceThreshold = 0.4;
            var blockSize = 32;
            var skip = 0;
            var objects = new List<ObjectFound>();
            for (int i = 0; i < 5; i++)
            {
                var data = _elements.Skip(skip).Take(25).ToList();
                skip += 25;

                var x = data[0];
                var y = data[1];

                x = (_gridCellX + Sigmoid(x)) * blockSize;
                y = (_gridCellY + Sigmoid(y)) * blockSize;

                var tw = data[2];
                var th = data[3];

                var width = (float)Math.Exp(tw) * Anchors[2 * i] * blockSize;
                var height = (float)Math.Exp(th) * Anchors[2 * i + 1] * blockSize;

                var confidence = Sigmoid(data[4]);

                (var classLabel, var bestClassScore) = GetLabelAndScore(data.Skip(5).Take(20).ToArray());

                var confidenceInClass = bestClassScore * confidence;

                if (confidenceInClass < confidenceThreshold)
                    continue;

                objects.Add(new ObjectFound
                {
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height,
                    ObjectLabel = classLabel,
                    Confidence = confidenceInClass

                });
            }
            return objects;
        }

        private (string, float) GetLabelAndScore(IList<float> classScores)
        {
            var eToPowerX = classScores.Select(a => (float)Math.Exp(a));
            var sum = eToPowerX.Sum();

            var softMax = eToPowerX.Select(a => a / sum);
            var maxSoftMax = softMax.Max();
            return (Labels[softMax.ToList().IndexOf(maxSoftMax)], maxSoftMax);
        }
    }
    public class ObjectFound
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string ObjectLabel { get; set; }
        public float Confidence { get; set; }
    }
}
