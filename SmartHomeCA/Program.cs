using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeCA
{
    public class Temperature
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Status { get; set; }
    }
    class Program
    {   
        static HttpClient client = new HttpClient();
        

        static async Task<Temperature> UpdateTempAsync(Temperature temp)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync($"api/TemperaturesAPI/{temp.Id}", temp);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            temp = await response.Content.ReadAsAsync<Temperature>();

            return temp;
        }

        static async Task<List<Temperature>> GetAllTempsAsync()
        {
            List<Temperature> temps = new List<Temperature>();
            
            HttpResponseMessage response = client.GetAsync(client.BaseAddress + "api/TemperaturesAPI").Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<Temperature>>().Result;
                foreach (var d in dataObjects)
                {
                    temps.Add(d);
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return temps;
        }

        static Temperature UpdateTempValue(Temperature temp)
        {
            Random rnd = new Random();
            int dice = rnd.Next(1, 4);
            switch (dice)
            {
                case 1:
                    {
                        if (temp.Value < 35)
                        {
                            temp.Value += 1;
                        }
                        break;
                    }
                case 2:
                    {
                        if (temp.Value > -35)
                        {
                            temp.Value -= 1;
                        }
                        break;
                    }
                case 3:
                    {
                        break;
                    }
                default:
                    break;
            }

            return temp;
        }

        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("http://localhost:58335/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            for (;;)
            {
                try
                {
                    // TODO: fix await on GetAllTempsAsync();
                    // GET List data response.
                    var temps = await GetAllTempsAsync();
                    
                    if (temps.Count != 0)
                    {
                        for (int i = 0; i < temps.Count; i++)
                        {
                            if (temps[i].Status != false)
                            {
                                var newTemp = UpdateTempValue(temps[i]);
                                await UpdateTempAsync(newTemp);
                                Console.WriteLine(newTemp.Value);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Temp List equal empty");
                        break;
                    }

                    Thread.Sleep(5000);
                    Console.WriteLine("***************************");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            Console.Read();
        }
    }
}
