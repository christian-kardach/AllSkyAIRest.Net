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
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        InferenceSession session;
        Configuration configuration = new Configuration();

        public Classify()
        {
            string modelFilePath = ".\\model\\"+configuration.Model; 
            session = new InferenceSession(modelFilePath);
        }

        public string ClassifyImage(string url)
        {
            string saveLocation = @".\\tmp.jpg";

            Console.WriteLine($"Downloading AllSkyImage: {url}");

            watch.Reset();
            watch.Start();
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
            watch.Stop();
            Console.WriteLine($"Download done in {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s, classifying image...");

            watch.Reset();
            watch.Start();

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
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

            // Postprocess to get softmax vector
            IEnumerable<float> output = results.First().AsEnumerable<float>();
            float sum = output.Sum(x => (float)Math.Exp(x));
            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / sum);

            // Extract top 10 predicted classes
            IEnumerable<Prediction> classificationScore = softmax.Select((x, i) => new Prediction { Label = LabelMap.Labels[i], Confidence = x })
                               .OrderByDescending(x => x.Confidence)
                               .Take(1);

            watch.Stop();

            var conf = (classificationScore.First().Confidence * 100f).ToString().Replace(",", ".");
            
            Console.WriteLine($"Label: {classificationScore.First().Label}, Confidence: {classificationScore.First().Confidence}");
            Console.WriteLine($"Classification done in {TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds}s");
            
            return ("{"+$"\"AllSkyAISky\": \"{classificationScore.First().Label}\", \"AllSkyAIConfidence:\" {conf}" + "}");

        }
        
    }
}
