using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace AllSkyAIRestServer.Net
{
    internal class Prediction
    {
        public string Label { get; set; }
        public float Confidence { get; set; }
    }

    internal class Classify
    {

        public string ClassifyImage(string url)
        {
            //string imageUrl = @"https://allsky.tristarobservatory.com/image.jpg";
            string saveLocation = @".\\tmp.jpg";

            Console.WriteLine($"Downloading AllSkyImage: {url}");

            HttpWebRequest lxRequest = (HttpWebRequest)WebRequest.Create(url);
            // returned values are returned as a stream, then read into a string
            String lsResponse = string.Empty;
            using (HttpWebResponse lxResponse = (HttpWebResponse)lxRequest.GetResponse())
            {
                using (BinaryReader reader = new BinaryReader(lxResponse.GetResponseStream()))
                {
                    Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                    using (FileStream lxFS = new FileStream(saveLocation, FileMode.Create))
                    {
                        lxFS.Write(lnByte, 0, lnByte.Length);
                    }
                }
            }
            Console.WriteLine("Download done, classifying image...");

            // Read paths
            string modelFilePath = @".\model\allskyai_tristar.onnx"; //args[0];
            //string imageFilePath = @"D:\Development\AllSkyAI_Dev\AllSkyAI_Local\images\heavy_clouds.jpg"; //args[1];

            // Read image
            Image<Rgb24> image = Image.Load<Rgb24>(saveLocation);
            image.Mutate(x => x.Resize(512, 512, KnownResamplers.Lanczos3));
            image.Mutate(x => x.Grayscale());

            // Preprocess image
            Tensor<float> input = new DenseTensor<float>(new[] { 1, 512, 512, 1 });
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, y, x, 0] = ((pixelSpan[x].R));
                    }
                }
            });

            // Setup inputs
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input", input)
            };

            // Run inference
            var session = new InferenceSession(modelFilePath);
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            // Postprocess to get softmax vector
            IEnumerable<float> output = results.First().AsEnumerable<float>();
            float sum = output.Sum(x => (float)Math.Exp(x));
            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / sum);

            // Extract top 10 predicted classes
            IEnumerable<Prediction> classificationScore = softmax.Select((x, i) => new Prediction { Label = LabelMap.Labels[i], Confidence = x })
                               .OrderByDescending(x => x.Confidence)
                               .Take(1);
            /*
            // Print results to console
            Console.WriteLine("Top 5 predictions for ResNet50 v2...");
            Console.WriteLine("--------------------------------------------------------------");
            foreach (var t in top10)
            {
                Console.WriteLine($"Label: {t.Label}, Confidence: {t.Confidence}");
            }
            */
            var conf = (classificationScore.First().Confidence*100f).ToString().Replace(",", ".");

            return ("{"+$"\"AllSkyAISky\": \"{classificationScore.First().Label}\", \"AllSkyAIConfidence:\" {conf}" + "}");

        }
        
    }
}
