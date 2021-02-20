using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BoardgamSolver
{
    public class RecognizeText
    {

        // Add your Computer Vision subscription key and endpoint to your environment variables.
        static string subscriptionKey = @"7125e360887e43c2a54575adf7f40f2a";

        static string endpoint = @"https://boardgame.cognitiveservices.azure.com/";

        // the OCR method endpoint
        static string uriBase = endpoint + "vision/v2.1/recognizeText";

        /// <summary>
        /// Gets the text visible in the specified image file by using
        /// the Computer Vision REST API.
        /// </summary>
        /// <param name="imageFilePath">The image file with printed text.</param>
        public static async Task<TextRecognitionOperationResult> MakeOCRRequest(MemoryStream imageStream)
        {
            try
            {
                HttpClient client = new HttpClient();

                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                var requestParameters = "mode=Printed";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageStream);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseUrl = string.Empty;

                    foreach (var h in response.Headers)
                    {
                        if (h.Key == "Operation-Location")
                        {

                            responseUrl = h.Value.First();

                            break;

                        }
                    }


                    string status = string.Empty;
                    TextRecognitionOperationResult screen;

                    do
                    {
                        await Task.Delay(50);

                        response = await client.GetAsync(responseUrl);

                        string contentString = await response.Content.ReadAsStringAsync();

                        screen = JsonConvert.DeserializeObject<TextRecognitionOperationResult>(contentString);



                    } while (screen.Status != TextRecognitionOperationStatus.Succeeded && screen.Status != TextRecognitionOperationStatus.Failed);

                    return screen;

                }
                else
                {
                    string contentString = await response.Content.ReadAsStringAsync();

                    MessageBox.Show(contentString);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return null;
        }

        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(MemoryStream imageStream)
        {
            // Read the file's contents into a byte array.
            BinaryReader binaryReader = new BinaryReader(imageStream);
            return binaryReader.ReadBytes((int)imageStream.Length);
        }
    }
}
