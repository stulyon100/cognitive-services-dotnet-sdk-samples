﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Samples.ComputerVision.AnalyzeImage
{
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
    using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;


    class Program
    {

        static void Main(string[] args)
        {
            // Add your Computer Vision subscription key and endpoint to your environment variables
            string subscriptionKey = "582a22b95cad4f14aba95fe74560d1f7"; 
            string endpoint = "https://stu-exam2-cv.cognitiveservices.azure.com/";

            try
            {
                AnalyzeImageSample.RunAsync(endpoint, subscriptionKey).Wait(5000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("\nPress ENTER to exit.");
            Console.ReadLine();
        }
    }

    public class AnalyzeImageSample
    {
        public static async Task RunAsync(string endpoint, string key)
        {
            ComputerVisionClient computerVision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            {
                Endpoint = endpoint
            };

            string localImagePath = @"Images\celebrities.jpg"; // See this repo's readme.md for info on how to get these images. Alternatively, you can just set the path to any appropriate image on your machine.
            //string remoteImageUrl = "https://github.com/Azure-Samples/cognitive-services-sample-data-files/raw/master/ComputerVision/Images/landmark.jpg";
            string remoteImageUrl = "https://static.independent.co.uk/s3fs-public/thumbnails/image/2015/06/06/15/Chris-Pratt.jpg";

            List<VisualFeatureTypes> features = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

            Console.WriteLine("Images being analyzed ...");
            await AnalyzeFromUrlAsync(computerVision, remoteImageUrl, features);
            //await AnalyzeLocalAsync(computerVision, localImagePath, features);
        }

        // Analyze a remote image
        private static async Task AnalyzeFromUrlAsync(ComputerVisionClient computerVision, string imageUrl, List<VisualFeatureTypes> features)
        {
            if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                Console.WriteLine("\nInvalid remote image url:\n{0} \n", imageUrl);
                return;
            }

            ImageAnalysis analysis = await computerVision.AnalyzeImageAsync(imageUrl, features);

            //specific calls
            TagResult res = await computerVision.TagImageAsync(imageUrl);
            var ok1 = res.Tags[0].Name;
            var ok2 = res.Tags[0].Confidence;

            ImageDescription res2 = await computerVision.DescribeImageAsync(imageUrl);
            var ok3 = res2.Captions[0];
            var ok4 = res2.Tags[0];

            DomainModelResults res3 = await computerVision.AnalyzeImageByDomainAsync("landmarks", imageUrl, null);
            var ermmmm = res3.Result;

            using (var stream = await computerVision.GenerateThumbnailAsync(100, 200, imageUrl))
            {
                //write stream to file
            }


            DisplayResults(analysis, imageUrl);

        }

        // Analyze a local image
        private static async Task AnalyzeLocalAsync(ComputerVisionClient computerVision, string imagePath, List<VisualFeatureTypes> features)
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine("\nUnable to open or read local image path:\n{0} \n", imagePath);
                return;
            }

            using (Stream imageStream = File.OpenRead(imagePath))
            {
                ImageAnalysis analysis = await computerVision.AnalyzeImageInStreamAsync(imageStream, features);
                DisplayResults(analysis, imagePath);
            }
        }

        private static void DisplayResults(ImageAnalysis analysis, string imagePath)
        {
            Console.WriteLine(imagePath);
            //DisplayImageDescription(analysis);
            //DisplayImageCategoryResults(analysis);
            DisplayTagResults(analysis);
            //DisplayObjectDetectionResults(analysis);
            //DisplayFaceResults(analysis);
            //DisplayBrandDetectionResults(analysis);
            //DisplayAdultResults(analysis);
            //DisplayColorSchemeResults(analysis);
            //DisplayDomainSpecificResults(analysis);
            //DisplayImageTypeResults(analysis);
        }

        private static void DisplayFaceResults(ImageAnalysis analysis)
        {
            //faces
            Console.WriteLine("Faces:");
            foreach (var face in analysis.Faces)
            {
                Console.WriteLine("A {0} of age {1} at location {2},{3},{4},{5}", face.Gender, face.Age,
                    face.FaceRectangle.Left, face.FaceRectangle.Top,
                    face.FaceRectangle.Left + face.FaceRectangle.Width,
                    face.FaceRectangle.Top + face.FaceRectangle.Height);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayAdultResults(ImageAnalysis analysis)
        {
            //racy content
            Console.WriteLine("Adult:");
            Console.WriteLine("Is adult content: {0} with confidence {1}", analysis.Adult.IsAdultContent, analysis.Adult.AdultScore);
            Console.WriteLine("Has racy content: {0} with confidence {1} ", analysis.Adult.IsRacyContent, analysis.Adult.RacyScore);
            Console.WriteLine("\n");
        }

        private static void DisplayTagResults(ImageAnalysis analysis)
        {
            //image tags
            Console.WriteLine("Tags, Confidence:");
            foreach (var tag in analysis.Tags)
            {
                Console.WriteLine("{0} ({1})", tag.Name, tag.Confidence);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayImageDescription(ImageAnalysis analysis)
        {
            //captioning
            Console.WriteLine("Captions:");
            foreach (var caption in analysis.Description.Captions)
            {
                Console.WriteLine("{0} with confidence {1}", caption.Text, caption.Confidence);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayObjectDetectionResults(ImageAnalysis analysis)
        {
            //objects
            Console.WriteLine("Objects:");
            foreach (var obj in analysis.Objects)
            {
                Console.WriteLine("{0} with confidence {1} at location {2},{3},{4},{5}",
                    obj.ObjectProperty, obj.Confidence,
                    obj.Rectangle.X, obj.Rectangle.X + obj.Rectangle.W,
                    obj.Rectangle.Y, obj.Rectangle.Y + obj.Rectangle.H);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayBrandDetectionResults(ImageAnalysis analysis)
        {
            //brands
            Console.WriteLine("Brands:");
            foreach (var brand in analysis.Brands)
            {
                Console.WriteLine("Logo of {0} with confidence {1} at location {2},{3},{4},{5}",
                    brand.Name, brand.Confidence,
                    brand.Rectangle.X, brand.Rectangle.X + brand.Rectangle.W,
                    brand.Rectangle.Y, brand.Rectangle.Y + brand.Rectangle.H);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayDomainSpecificResults(ImageAnalysis analysis)
        {
            //celebrities
            Console.WriteLine("Celebrities:");
            foreach (var category in analysis.Categories)
            {
                if (category.Detail?.Celebrities != null)
                {
                    foreach (var celeb in category.Detail.Celebrities)
                    {
                        Console.WriteLine("{0} with confidence {1} at location {2},{3},{4},{5}",
                            celeb.Name, celeb.Confidence,
                            celeb.FaceRectangle.Left, celeb.FaceRectangle.Top,
                            celeb.FaceRectangle.Height, celeb.FaceRectangle.Width);
                    }
                }
            }

            //landmarks
            Console.WriteLine("Landmarks:");
            foreach (var category in analysis.Categories)
            {
                if (category.Detail?.Landmarks != null)
                {
                    foreach (var landmark in category.Detail.Landmarks)
                    {
                        Console.WriteLine("{0} with confidence {1}", landmark.Name, landmark.Confidence);
                    }
                }
            }
            Console.WriteLine("\n");
        }

        private static void DisplayColorSchemeResults(ImageAnalysis analysis)
        {
            //color scheme
            Console.WriteLine("Color Scheme:");
            Console.WriteLine("Is black and white?: " + analysis.Color.IsBWImg);
            Console.WriteLine("Accent color: " + analysis.Color.AccentColor);
            Console.WriteLine("Dominant background color: " + analysis.Color.DominantColorBackground);
            Console.WriteLine("Dominant foreground color: " + analysis.Color.DominantColorForeground);
            Console.WriteLine("Dominant colors: " + string.Join(",", analysis.Color.DominantColors));
        }

        private static void DisplayImageCategoryResults(ImageAnalysis analysis)
        {
            //categorize
            Console.WriteLine("Categories:\n");
            foreach (var category in analysis.Categories)
            {
                Console.WriteLine("{0} with confidence {1}", category.Name, category.Score);
            }
            Console.WriteLine("\n");
        }

        private static void DisplayImageTypeResults(ImageAnalysis analysis)
        {
            //image types
            Console.WriteLine("Image Type:"); //please look at the API documentation to know more about what the scores mean
            Console.WriteLine("Clip Art Type: " + analysis.ImageType.ClipArtType);
            Console.WriteLine("Line Drawing Type: " + analysis.ImageType.LineDrawingType);
            Console.WriteLine("\n");
        }
    }
}
